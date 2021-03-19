using System;
using System.Collections.Generic;
using System.Linq;
using Prolog.Engine;
using Prolog.Engine.Miscellaneous;
using Ginger.Runner.Solarix;
using System.Text.RegularExpressions;

namespace Ginger.Runner
{
    using static DomainApi;
    using static MayBe;

    using ConcreteUnderstander = Func<Either<Word, Quotation>, MayBe<UnderstoodSentence>>;

    internal sealed record GenerativePattern(string PatternId, string PatternText, IReadOnlyCollection<Rule> Meaning)
    {
        public IReadOnlyCollection<ConcreteUnderstander> GenerateConcreteUnderstanders(
            IRussianGrammarParser grammarParser,
            IRussianLexicon russianLexicon) 
        =>
            ConcretePatternBuilder.TryParse(PatternText, grammarParser, russianLexicon)
                .Map(concretePatternBuilder =>
                    Enumerable
                        .Range(1, MaximumNumberOfElementsInEnumerations)
                        .Select(elementsNumber => 
                                PatternBuilder.BuildPattern(
                                    PatternId, 
                                    grammarParser.ParseAnnotated(
                                        DisambiguatedPattern.Create(
                                            concretePatternBuilder.GenerateConcretePatternText(
                                                russianLexicon, 
                                                elementsNumber),
                                            russianLexicon)), 
                                    concretePatternBuilder.ReplicateMeaning(Meaning, elementsNumber),
                                    russianLexicon))
                        .AsImmutable())
                .OrElse(() => new[]
                            { 
                                PatternBuilder.BuildPattern(
                                    PatternId, 
                                    grammarParser.ParseAnnotated(
                                        DisambiguatedPattern.Create(PatternText, russianLexicon)), 
                                    Meaning,
                                    russianLexicon)
                            });

        internal IReadOnlyCollection<string> GenerateConcretePatterns(
            IRussianGrammarParser grammarParser,
            IRussianLexicon russianLexicon) 
        =>
            ConcretePatternBuilder.TryParse(PatternText, grammarParser, russianLexicon)
                .Map(concretePatternBuilder =>
                    Enumerable
                        .Range(1, MaximumNumberOfElementsInEnumerations)
                        .Select(elementsNumber => 
                            concretePatternBuilder.GenerateConcretePatternText(russianLexicon, elementsNumber))
                        .AsImmutable())
                .OrElse(() => 
                    throw Failure($"Could not parse generative pattern {PatternText}"));

        private const int MaximumNumberOfElementsInEnumerations = 4;

        private record ConcretePatternBuilder(
            string PatternText,
            string ConcretePatternWithJustReplicatableWord,
            ReplicatableWord ReplicatableWord,
            IReadOnlyCollection<PluralitySensitiveElement> PluralitySensitiveElements)
        {
            public string GenerateConcretePatternText(IRussianLexicon russianLexicon, int elementsNumber) =>
                elementsNumber == 1 
                    ? ConcretePatternWithJustReplicatableWord
                    : new[] { ReplicatableWord.BuildPatternModifier(elementsNumber, russianLexicon) }
                        .Concat(PluralitySensitiveElements.Select(it => it.BuildPatternModifier(russianLexicon)))
                        .OrderByDescending(operation => operation.Location.Start)
                        .Aggregate(
                            PatternText, 
                            (pattern, patternModifier) => patternModifier.ApplyTo(pattern));

            public IReadOnlyCollection<Rule> ReplicateMeaning(
                IReadOnlyCollection<Rule> meaningForJustReplicatableWord, 
                int n) 
            =>
                Enumerable
                    .Range(1, n)
                    .SelectMany(i => i == 1 
                                ? meaningForJustReplicatableWord 
                                : ReplicatableWord.AdjustMeaning(meaningForJustReplicatableWord, i))
                    .AsImmutable();

            public static MayBe<ConcretePatternBuilder> TryParse(
                string annotatedPatternText, 
                IRussianGrammarParser grammarParser,
                IRussianLexicon russianLexicon)
            {
                var generationElements = (
                        from m in GenerationElementRegex.Matches(annotatedPatternText)
                        let word = m.Groups[1].Value
                        let wordLocation = new SubstringLocation(m.Groups[1].Index, m.Groups[1].Length)
                        let lemmaDisambiguator = m.Groups[2].Success 
                                ? LemmaVersionDisambiguatorDefinition.Create(m.Groups[2].Value, russianLexicon) 
                                : null
                        let generationHint = ParseHint(m.Groups[3].Value)
                        let generationHintLocation = new SubstringLocation(m.Groups[3].Index, m.Groups[3].Length)
                        let generationElement = new GenerationElementLocation(wordLocation, generationHintLocation)
                        select new { word, generationHint, generationElement, lemmaDisambiguator }
                    ).ToArray();

                var patternWithGenerationHintsRemoved = generationElements
                        .OrderByDescending(ge => ge.generationElement.WordLocation.Start)
                        .Select(ge => ge.generationElement.HintLocation)
                        .Aggregate(
                            annotatedPatternText,
                            (pattern, hint) => pattern.Remove(hint.Start, hint.Length))
                        .Trim();

                var parsedPattern = grammarParser.ParseAnnotated(
                     DisambiguatedPattern.Create(
                         patternWithGenerationHintsRemoved,
                         russianLexicon)).Sentence;
                if (!parsedPattern.IsLeft) 
                {   
                    return None; // it's a quote, hence it can't be a generative pattern
                }

                if (generationElements
                        .Where(ge => ge.generationHint == GenerationHint.Replicatable)
                        .Any(ge => ge.lemmaDisambiguator != null))
                {
                    throw Failure(
                        "Lemma version disambiguators for replicatable words are not supported. Please reformulate the pattern.");
                }

                var replicatableWords = (
                            from ge in generationElements
                            where ge.generationHint == GenerationHint.Replicatable
                            let wordElement = FindWordInParsedPattern(
                                                patternWithGenerationHintsRemoved, 
                                                parsedPattern.Left!, 
                                                ge.word, 
                                                ge.generationHint)
                            select new ReplicatableWord(
                                ge.generationElement,
                                GetSingleLemmaVersion(wordElement, patternWithGenerationHintsRemoved, ge.generationHint))
                        ).AsImmutable();

                switch (replicatableWords.Count)
                {
                    case 0: return None;
                    case 1: break;
                    default: throw Failure(
                        $"Only single replicatable word marked with {ReplicatableHintText} is supported. " + 
                        $"In pattern '{annotatedPatternText}' there are {replicatableWords.Count}.");
                }
                
                var pluralitySensitiveElements = (
                        from ge in generationElements
                        where ge.generationHint == GenerationHint.PluralitySensitive
                        let wordElement = FindWordInParsedPattern(
                                            patternWithGenerationHintsRemoved,
                                            parsedPattern.Left!, 
                                            ge.word, 
                                            ge.generationHint)
                        select new PluralitySensitiveElement(
                            ge.generationElement,
                            EnsureMasculine(
                                GetSingleLemmaVersion(
                                    wordElement, 
                                    patternWithGenerationHintsRemoved, 
                                    ge.generationHint,
                                    ignoreGender: true),
                                ge.word),
                            ge.lemmaDisambiguator)
                                
                        ).AsImmutable();

                return Some(
                    new ConcretePatternBuilder(
                        annotatedPatternText,
                        patternWithGenerationHintsRemoved,
                        replicatableWords.Single(),
                        pluralitySensitiveElements));

                GenerationHint ParseHint(string hintText) =>
                    hintText switch
                    {
                        ReplicatableHintText => GenerationHint.Replicatable,
                        "{мн.}" => GenerationHint.PluralitySensitive,
                        _ => throw Failure($"Unknown generation hint: '{hintText}'")
                    };

                Word FindWordInParsedPattern(
                    string patternText,
                    Word sentenceElement, 
                    string word, 
                    GenerationHint hintType) 
                =>
                    sentenceElement
                        .LocateWord(
                            word,
                            errorText => Failure(
                                $" {errorText}. The {HintToString(hintType)} '{word}' should appear in the pattern '{patternText}' exactly once."));

                LemmaVersion GetSingleLemmaVersion(
                    Word wordElement,
                    string patternText,
                    GenerationHint hintType,
                    bool ignoreGender = false) 
                =>
                    (!ignoreGender || wordElement.LemmaVersions.Count == 1
                        ? wordElement.LemmaVersions
                        : wordElement.LemmaVersions
                            .Where(lv => (lv.Characteristics.TryGetGender() ?? Gender.Мужской) == Gender.Мужской))
                        .TrySingleOrDefault(_ => 
                            Failure(
                                $"{HintToString(hintType)} '{wordElement.Content}' should have a single lemma version in the phrase '{patternText}'. " + 
                                "Please reformulate the pattern's text."));

                string HintToString(GenerationHint hintType) =>
                    hintType switch
                    {
                        GenerationHint.Replicatable => "replicatable word",
                        GenerationHint.PluralitySensitive => "plurality sensitive word",
                        _ => throw Failure($"Unsupported hint type {hintType}")
                    };

                LemmaVersion EnsureMasculine(LemmaVersion lemmaVersion, string word) =>
                    (lemmaVersion.Characteristics.TryGetGender() ?? Gender.Мужской) == Gender.Мужской
                        ? lemmaVersion
                        : throw Failure(
                            $"'{word}{ReplicatableHintText}': Слова, переходящие во множественное число при репликации, " +
                            "всегда должны иметь мужской род");
            }

            private static readonly Regex GenerationElementRegex = new (@"([а-яА-Я]+)\s*(\([^\)]+\))?\s*({[^}]+})", RegexOptions.Compiled);
            private const string ReplicatableHintText = "{и проч.}";
        }

        private static InvalidOperationException Failure(string message) => new (message);

        private record ReplicatableWord(GenerationElementLocation GenerationElement, LemmaVersion WordLemma)
        {
            public StringAdjustingOperation BuildPatternModifier(int numberOfReplicas, IRussianLexicon russianLexicon) =>
                new (
                    new SubstringLocation(
                        GenerationElement.WordLocation.End, 
                        GenerationElement.HintLocation.End - GenerationElement.WordLocation.End),
                    numberOfReplicas switch
                    {
                        2 => $" и {GetElement(2, russianLexicon)}",
                        _ => ", " + string.Join(", ", GetElements(numberOfReplicas - 1, russianLexicon)) + 
                             $" и {GetElement(numberOfReplicas, russianLexicon)}"
                    });

            public IReadOnlyCollection<Rule> AdjustMeaning(IReadOnlyCollection<Rule> singleElementMeaning, int nthReplication)
            {
                return singleElementMeaning.Select(AdjustRule).AsImmutable();

                Rule AdjustRule(Rule rule) =>
                    Rule(AdjustComplexTerm(rule.Conclusion), rule.Premises.Select(AdjustComplexTerm));

                ComplexTerm AdjustComplexTerm(ComplexTerm complexTerm) =>
                    complexTerm with { Arguments = new (complexTerm.Arguments.Select(AdjustTerm)) };

                Term AdjustTerm(Term term) =>
                    term switch
                    {
                        Atom atom when atom.Characters.Equals(
                            WordLemma.Lemma,
                            StringComparison.OrdinalIgnoreCase) => Atom(GetElement(nthReplication)),
                        ComplexTerm ct => AdjustComplexTerm(ct),
                        _ => term
                    };
            }

            private IEnumerable<string> GetElements(int n, IRussianLexicon russianLexicon) =>
                Enumerable.Range(2, n - 1).Select(i => GetElement(i, russianLexicon));

            private string GetElement(int i, IRussianLexicon russianLexicon) =>
                russianLexicon.GenerateWordForm(GetElement(i), WordLemma.PartOfSpeech, WordLemma.Characteristics);

            private string GetElement(int i) =>
                WordsForReplication.TryGetValue(WordLemma.PartOfSpeech ?? PartOfSpeech.Существительное, out var words)
                    ? (i - 2 < words.Length 
                        ? words[i - 2] 
                        : throw ProgramLogic.Error($"WordsForReplication[{WordLemma.PartOfSpeech}] does not have enough alternatives"))
                    : throw ProgramLogic.Error(
                        $"Do not have words of {WordLemma.PartOfSpeech} to generate enumerations in patterns.");

            private static readonly IReadOnlyDictionary<PartOfSpeech, string[]> WordsForReplication = 
                new Dictionary<PartOfSpeech, string[]>
                {
                    [PartOfSpeech.Существительное] = new[] { "летчик", "наводчик", "поэт" },
                    [PartOfSpeech.Прилагательное] = new[] { "красный", "длинный", "теплый" }
                };
        }

        private record PluralitySensitiveElement(
            GenerationElementLocation GenerationElement, 
            LemmaVersion WordLemma,
            LemmaVersionDisambiguatorDefinition? DisambiguatorDefinition)
        {
            public StringAdjustingOperation BuildPatternModifier(IRussianLexicon russianLexicon)
            {
                var adjustingOperation = GenerationElement.MakeAdjustingOperation(
                            russianLexicon.GetPluralForm(WordLemma), 
                            GenerationElementPart.WholeSpot);
                return DisambiguatorDefinition == null
                            ? adjustingOperation
                            : adjustingOperation.InsertAtEnd(DisambiguatorDefinition.Remove(typeof(Gender)).Definition);
            }
        }

        private record GenerationElementLocation(SubstringLocation WordLocation, SubstringLocation HintLocation)
        {
            public StringAdjustingOperation MakeAdjustingOperation(string newContent, GenerationElementPart elementPart) =>
                new (
                    elementPart switch 
                    {
                        GenerationElementPart.WholeSpot => WordLocation with { Length = HintLocation.End - WordLocation.Start },
                        GenerationElementPart.GenerationHint => HintLocation,
                        _ => throw ProgramLogic.Error($"Unsupported GenerationElementPart {elementPart}")
                    },
                    newContent);
                
        }

        private record SubstringLocation(int Start, int Length)
        {
            public int End => Start + Length;
        }

        private record StringAdjustingOperation(SubstringLocation Location, string NewContent)
        {
            public StringAdjustingOperation InsertAtEnd(string extraContent) =>
                this with { NewContent = NewContent + extraContent };

            public string ApplyTo(string text) =>
                text
                    .Remove(Location.Start, Location.Length)
                    .Insert(Location.Start, NewContent);
        }

        private enum GenerationHint
        {
            Replicatable,
            PluralitySensitive
        }
 
        private enum GenerationElementPart
        {
            WholeSpot,
            GenerationHint
        }
   }
}