using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Prolog.Engine;
using Prolog.Engine.Miscellaneous;
using Ginger.Runner.Solarix;

namespace Ginger.Runner
{
    using WordOrQuotation = Either<Word, Quotation>;
    using SentenceMeaning = Either<IReadOnlyCollection<Rule>, IReadOnlyCollection<ComplexTerm>>;

    using static DomainApi;
    using static Either;
    using static MayBe;
    using static Prolog.Engine.Parsing.PrologParser;
    using static Impl;

    internal static class PatternBuilder
    {
        public static Func<WordOrQuotation, MayBe<UnderstoodSentence>> BuildPattern(
            string patternId, 
            AnnotatedSentence pattern,
            SentenceMeaning meaning,
            IRussianLexicon russianLexicon)
        {
            var allWordsUsedInMeaning = new HashSet<string>(
                meaning.Fold(
                    rules => rules.SelectMany(ListUsedWords), 
                    statements => statements.SelectMany(ListUsedWords)),
                RussianIgnoreCase);
            var (sentenceStructureChecker, pathesToWords) = BuildSentenceStructureChecker(
                pattern,
                allWordsUsedInMeaning,
                ImmutableStack.Create<int>(),
                new Dictionary<string, PathToWordBase>(RussianIgnoreCase),
                russianLexicon);
            
            return meaning.Fold(
                rules => 
                {
                    var ruleBuilders = rules.Select(rule => MakeRuleBuilder(pattern.Sentence, rule, pathesToWords)).AsImmutable();
                    return BuildPatternCore(sentence => Left(ruleBuilders.Select(rb => rb(sentence)).AsImmutable()));
                },
                statements =>
                {
                    var statementBuilders = statements.Select(statement => MakeComplexTermBuilder(pattern.Sentence, statement, pathesToWords)).AsImmutable();
                    return BuildPatternCore(sentence => Right(statementBuilders.Select(sb => sb(sentence)).AsImmutable()));
                });

            Func<WordOrQuotation, MayBe<UnderstoodSentence>> BuildPatternCore(
                Func<WordOrQuotation, SentenceMeaning> buildMeaning) 
            {
                PatternEstablished?.Invoke(patternId, pattern, meaning);
                return sentence => sentenceStructureChecker(new CheckableSentenceElement(sentence, sentence)) switch
                    {
                        true => Some(new UnderstoodSentence(patternId, buildMeaning(sentence))),
                        _ => None
                    };
            }
        }

        public static Exception PatternBuildingException(string message, bool invalidOperation = false) =>
            invalidOperation 
                ? new InvalidOperationException(message) 
                : new NotSupportedException(message);

        public static event Action<string, AnnotatedSentence, SentenceMeaning>? PatternEstablished;

        public static event Action<string, bool>? PatternRecognitionEvent;

        private static IEnumerable<string> ListUsedWords(Rule meaning) =>
            ListUsedWords(meaning.Conclusion).Concat(meaning.Premises.SelectMany(ListUsedWords));

        private static IEnumerable<string> ListUsedWords(ComplexTerm complexTerm) =>
            SplitTextAtUpperCharacters(complexTerm.Functor.Name)
            .Concat(complexTerm.Arguments.SelectMany(t =>
                t switch
                {
                    Atom atom => SplitTextAtUpperCharacters(atom.Characters),
                    Variable variable => SplitTextAtUpperCharacters(variable.Name),
                    ComplexTerm ct => ListUsedWords(ct),
                    _ => Enumerable.Empty<string>()
                }));

        private static (Func<CheckableSentenceElement, bool> Checker, PathesToWords Pathes)
            BuildSentenceStructureChecker(
                AnnotatedSentence pattern,
                IReadOnlySet<string> allWordsUsedInMeaning,
                ImmutableStack<int> wordIndexes,
                Dictionary<string, PathToWordBase> pathesToWords,
                IRussianLexicon russianLexicon)
        {
            return (pattern.Sentence.Fold(BuildWordChecker, BuildQuotationChecker), new (pathesToWords));

            Func<CheckableSentenceElement, bool> BuildWordChecker(Word currentNode)
            {
                if (currentNode.LemmaVersions.Count > 1)
                {
                    var annotationVariants = string.Join(";", BuildDisambiguatingAnnotations(currentNode.LemmaVersions));
                    throw PatternBuildingException(
                            $"There are several lemma versions of the word '{currentNode.Content}' in pattern '{pattern.Text}'. " +
                            $"You can annotate the word with one of the following variants {annotationVariants}, " + 
                            "or reformulate the pattern wording.", 
                            invalidOperation: true);
                }

                var lemmaVersion = currentNode.LemmaVersions.Single();
                var targetLemmaVersion = new TargetLemmaVersion(
                            lemmaVersion.PartOfSpeech, 
                            lemmaVersion.Characteristics);
                var isPunctuationSymbol = lemmaVersion.Lemma.IsOneOf(",", ":", ";");
                var currentNodeIsAnnotated = pattern.WordIsAnnotated(currentNode.Content, lemmaVersion);
                PathToWordBase? pathToExpectedWord = null;
                if (!isPunctuationSymbol && !pathesToWords.TryGetValue(lemmaVersion.Lemma, out pathToExpectedWord))
                {
                    pathesToWords.Add(lemmaVersion.Lemma, new PathToWord(wordIndexes, targetLemmaVersion));
                }

                var particularWordIsExpectedAtThisPlaceInSentence = 
                    isPunctuationSymbol || 
                    !allWordsUsedInMeaning.Contains(lemmaVersion.Lemma) ||
                    currentNodeIsAnnotated;

                var childCheckers = BuildChildCheckers(currentNode.Children);

                return checkableElement => 
                    checkableElement.Current.Fold(word => word, _ => default(Word)) is var currentWord &&
                    LogChecking(currentWord != null, log: "current element is not quotation") &&
                    LogChecking(log: currentWord!.Content) && 
                    LogChecking(
                        currentWord.Children.Count == childCheckers.Length, 
                        log: $"{childCheckers.Length} children expected") &&
                    LogChecking(
                        pathToExpectedWord == null ||
                        currentWord.LemmaVersions.Any(lv => lv.Lemma == pathToExpectedWord.GetWordFrom(checkableElement.Root)),
                        log: $"expecting '{pathToExpectedWord?.GetWordFrom(checkableElement.Root)}' at {checkableElement}") &&
                    LogChecking(
                        targetLemmaVersion.FindRelevantLemma(currentWord.LemmaVersions).HasValue, 
                        log: "Searching for relevant lemma. " + 
                            $"Expected: {targetLemmaVersion.PartOfSpeech}-{targetLemmaVersion.GrammarCharacteristics} ({lemmaVersion.Lemma}) " + 
                            $"Actual: {string.Join(";", currentWord.LemmaVersions.Select(lv => lv.PartOfSpeech + "-" + lv.Characteristics))} ({currentWord.Content})") &&
                    (
                    !particularWordIsExpectedAtThisPlaceInSentence || 
                    LogChecking(
                        currentWord.LemmaVersions.Any(
                            lv => lv.Lemma == lemmaVersion.Lemma && 
                            lv.Characteristics.CompatibleTo(lemmaVersion.Characteristics)), 
                        log: $"Particular lemma '{lemmaVersion}' is expected at {checkableElement}")
                    ) &&
                    childCheckers
                        .Zip(currentWord.Children)
                        .All(it => it.First(checkableElement with { Current = it.Second }));
            }

            Func<CheckableSentenceElement, bool> BuildQuotationChecker(Quotation quotation)
            {
                var childCheckers = BuildChildCheckers(quotation.Children);
                if (!pathesToWords.ContainsKey(quotation.Content))
                {
                    pathesToWords.Add(quotation.Content, new PathToQuotation(wordIndexes));
                }

                return checkableElement => 
                        checkableElement.Current.Fold(
                            _ => LogChecking(false, log: "current element is quotation"),
                            checkedQuotation => 
                                childCheckers
                                    .Zip(checkedQuotation.Children)
                                    .All(it => it.First(checkableElement with { Current = it.Second })));
            }

            Func<CheckableSentenceElement, bool>[] BuildChildCheckers(IReadOnlyList<WordOrQuotation> children)
            {
                var childCheckers = new Func<CheckableSentenceElement, bool>[children.Count];
                for (var i = 0; i != children.Count; ++i)
                {
                    childCheckers[i] = BuildSentenceStructureChecker(
                                            pattern with { Sentence = children[i] }, 
                                            allWordsUsedInMeaning, 
                                            wordIndexes.Push(i), 
                                            pathesToWords,
                                            russianLexicon)
                                        .Checker;
                }

                return childCheckers;
            }

            string BuildDisambiguatingAnnotations(IReadOnlyCollection<LemmaVersion> lemmaVersions) =>
                string.Join(
                    ", ", 
                    LemmaVersionDisambiguator.Create(lemmaVersions)
                        .ProposeDisambiguations(russianLexicon));
        }

        private static Func<WordOrQuotation, Rule> MakeRuleBuilder(
            WordOrQuotation pattern,
            Rule rule,
            PathesToWords words)
        {
            var conclusionBuilder = MakeComplexTermBuilder(pattern, rule.Conclusion, words);
            var premiseBuilders = rule.Premises.Select(premise => MakeComplexTermBuilder(pattern, premise, words)).AsImmutable();
            return sentence => Rule(conclusionBuilder(sentence), premiseBuilders.Select(pb => pb(sentence)));
        }

        private static Func<WordOrQuotation, ComplexTerm> MakeComplexTermBuilder(
            WordOrQuotation pattern, 
            ComplexTerm complexTerm,
            PathesToWords words)
        {
            var functorBuilder = MakeFunctorBuilder(complexTerm.Functor, words);
            var argumentBuilders = complexTerm.Arguments.Select(arg => MakeTermBuilder(pattern, arg, words)).AsImmutable();
            return sentence => ComplexTerm(functorBuilder(sentence), argumentBuilders.Select(ab => ab(sentence)));
        }

        private static Func<WordOrQuotation, FunctorBase> MakeFunctorBuilder(
            FunctorBase functor,
            PathesToWords words)
        {
            if (BuiltinPrologFunctors.Contains(functor.Name))
            {
                return _ => functor;
            }
            else if (functor is Functor f)
            {
                var functorNameGetter = words.LocateWord(f.Name);
                return sentence => Functor(functorNameGetter(sentence), functor.Arity);
            }

            throw PatternBuildingException($"Cannot handle functor '{functor.Name}' of type {functor.GetType().Name} in meanining pattern.");
        }

        private static Func<WordOrQuotation, Term> MakeTermBuilder(
            WordOrQuotation pattern, 
            Term term,
            PathesToWords words)
        {
            if (term is Atom atom)
            {
                var atomNameGetter = words.LocateWord(atom.Characters);
                return sentence => Atom(atomNameGetter(sentence));
            }

            if (term is Prolog.Engine.Number number)
            {
                var numberValueGetter = words.LocateWord(number.Value.ToString(CultureInfo.CurrentCulture));
                return sentence => Number(int.Parse(numberValueGetter(sentence), CultureInfo.CurrentCulture));
            }

            if (term is Variable variable)
            {
                var variableNameGetter = words.LocateWord(variable.Name);
                return sentence => Variable(variableNameGetter(sentence));
            }

            if (term is ComplexTerm complexTerm)
            {
                var complexTermBuilder = MakeComplexTermBuilder(pattern, complexTerm, words);
                return sentence => complexTermBuilder(sentence);
            }

            throw PatternBuildingException($"Term {term} is not supported.");
        }

        private static IEnumerable<string> SplitTextAtUpperCharacters(string text)
        {
            var currentWordStart = 0;
            for (var i = 1; i < text.Length; ++i)
            {
                if (char.IsUpper(text[i]))
                {
                    yield return text.Substring(currentWordStart, i - currentWordStart);
                    currentWordStart = i;
                }
            }

            if (currentWordStart < text.Length)
            {
                yield return text.Substring(currentWordStart);
            }
        }

        private static bool LogChecking(bool checkSucceeded = true, string? log = null)
        {
            PatternRecognitionEvent?.Invoke(log ?? string.Empty, checkSucceeded);
            return checkSucceeded;
        }

        private static readonly IReadOnlySet<string> BuiltinPrologFunctors = 
            new HashSet<string>(
                Builtin.Functors
                    .Concat(Builtin.Rules.Select(r => r.Conclusion.Functor))
                    .Select(f => f.Name));

        private record TargetLemmaVersion(PartOfSpeech? PartOfSpeech, GrammarCharacteristics GrammarCharacteristics)
        {
            public MayBe<LemmaVersion> FindRelevantLemma(IEnumerable<LemmaVersion> lemmaVersions) =>
                lemmaVersions.TryFirst(lm => 
                                PartOfSpeech == lm.PartOfSpeech &&
                                GrammarCharacteristics.CompatibleTo(lm.Characteristics));
        }

        private abstract record PathToWordBase(ImmutableStack<int> ChildrenIndexes)
        {
            public abstract string GetWordFrom(WordOrQuotation sentence);

            protected WordOrQuotation TravelToChild(WordOrQuotation sentence)
            {
                var currentNode = sentence;
                foreach (var childIndex in ChildrenIndexes.Reverse())
                {
                    currentNode = currentNode.Fold(word => word.Children, quotation => quotation.Children)[childIndex];
                }

                return currentNode;
            }
        }

#pragma warning disable CA1801 // Review unused parameters
        private sealed record PathToWord(ImmutableStack<int> ChildrenIndexes, TargetLemmaVersion LemmaVersion) : PathToWordBase(ChildrenIndexes)
#pragma warning restore CA1801
        {
            public override string GetWordFrom(WordOrQuotation sentence) =>
                TravelToChild(sentence).Fold(
                    word => LemmaVersion
                            .FindRelevantLemma(word.LemmaVersions)
                            .OrElse(() => throw PatternBuildingException(
                                $"Could not find {LemmaVersion.PartOfSpeech} lemma version " +
                                $"of type {LemmaVersion.GrammarCharacteristics.GetType().Name} " +
                                $"at element {word} in sentence {sentence}.",
                                invalidOperation: true))
                            .Lemma,
                    quotation => throw ProgramLogic.Error(
                        "Sentence structure checker for current pattern should have ensured that " + 
                        $"the SentenceElement at the path {string.Join(',', ChildrenIndexes)} is a regular word. " + 
                        $"In current senetence it's the {quotation}."));
        }

        private sealed record PathToQuotation(ImmutableStack<int> ChildrenIndexes) : PathToWordBase(ChildrenIndexes)
        {
            public override string GetWordFrom(WordOrQuotation sentence) =>
                TravelToChild(sentence).Fold(
                    word => throw ProgramLogic.Error(
                        "Sentence structure checker for current pattern should have ensured that " +
                        $"the SentenceElement at the path {string.Join(',', ChildrenIndexes)} is a quotation. " +
                        $"In current senetence it's the {word}."),
                    quotation => quotation.Content);
        }

        private record PathesToWords(IReadOnlyDictionary<string, PathToWordBase> Pathes)
        {
            public Func<WordOrQuotation, string> LocateWord(string text) =>
                PatternBuilder.SplitTextAtUpperCharacters(text)
                    .Select(LocateSingleWord)
                    .AsImmutable()
                    .Apply(wordLocators => wordLocators.Count switch
                        {
                            0 => _ => string.Empty,
                            1 => wordLocators.Single(),
                            _ => sentenceElement => string.Join(
                                                        string.Empty, 
                                                        wordLocators.Select((wl, i) => 
                                                            i == 0
                                                                ? wl(sentenceElement)
                                                                : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                                                                    wl(sentenceElement))))
                        });

            private Func<WordOrQuotation, string> LocateSingleWord(string word) =>
                Pathes
                    .TryFind(word)
                    .Map<Func<WordOrQuotation, string>>(p => p.GetWordFrom)
                    .OrElse(() => IsIntroducedVariable(word)
                        ? _ => word
                        : throw PatternBuildingException(
                            $"Could not find word {word} in the pattern sentence. " +
                            $"Only these words are present: [{string.Join(", ", Pathes.Keys)}]"));

            private static bool IsIntroducedVariable(string word) =>
                TryParseTerm(word).OrElse(() => Atom("a")) is Variable;
        }

        private record CheckableSentenceElement(WordOrQuotation Root, WordOrQuotation Current);
   }
}