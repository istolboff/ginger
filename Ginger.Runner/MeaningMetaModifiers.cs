using System;
using System.Collections.Generic;
using System.Linq;
using Ginger.Runner.Solarix;
using Prolog.Engine;
using Prolog.Engine.Miscellaneous;

namespace Ginger.Runner
{
    using static DomainApi;
    using static MayBe;
    using static PatternBuilder;

    using WordOrQuotation = Either<Word, Quotation>;
    using SentenceMeaning = Either<IReadOnlyCollection<Rule>, IReadOnlyCollection<ComplexTerm>>;
    
    internal static class MeaningMetaModifiers
    {
        public const string Understand = "@understand";

        public static bool IsMetaModifier(FunctorBase functor) =>
                functor.Name.StartsWith('@');

        public static SentenceMeaning Preprocess(SentenceMeaning meaning) =>
            meaning.Fold2(rules => rules.ConvertAll(Preprocess), statements => statements.ConvertAll(Preprocess));

        public static Func<WordOrQuotation, MayBe<ComplexTerm>> MakeUnderstander(
            IRussianGrammarParser grammarParser,
            SentenceUnderstander sentenceUnderstander,
            Func<WordOrQuotation, string> relevantQuoteGetter) =>
            sentence => 
            {
                var quote = relevantQuoteGetter(sentence);
                var parsedQuote = grammarParser.ParsePreservingQuotes(quote);
                return sentenceUnderstander.Understand(parsedQuote)
                        .Map(it => it.Meaning.Fold(
                            _ => LogChecking(
                                    None, 
                                    $"understanding of '{quote}' produced one or more Rules. Only a set of statements is supported.",
                                    false),
                            statements => LogChecking(
                                Some(ComplexTerm(
                                        Functor(InlinerFunctorName, statements.Count),
                                        statements)),
                                $"undestanding of '{quote}'")))
                        .OrElse(() => LogChecking(
                                    None, 
                                    $"understanding of '{quote}'.", 
                                    false));
            };

        public static ComplexTerm AccomodateInlinedArguments(FunctorBase functor, IReadOnlyCollection<Term> arguments)
        {
            var inlinedArgumentsPresent = arguments.OfType<ComplexTerm>().Any(ct => ct.Functor.Name == InlinerFunctorName);
            if (!inlinedArgumentsPresent)
            {
                return ComplexTerm(functor, arguments);
            }

            var inlinedArguments = arguments
                    .SelectMany(arg => 
                        arg switch
                        {
                            ComplexTerm ct when ct.Functor.Name == InlinerFunctorName => 
                                ct.Arguments as IEnumerable<Term>,
                            _ => new[] { arg }
                        })
                    .AsImmutable();

            if (functor == Builtin.DotFunctor)
            {
                // skipping trailing empty list 
                return List(inlinedArguments.Reverse().Skip(1));  
            }

            if (inlinedArguments.Count == functor.Arity || !PatternBuilder.BuiltinPrologFunctors.Contains(functor.Name))
            {
                return ComplexTerm(functor with { Arity = inlinedArguments.Count }, inlinedArguments);
            }

            throw MetaModifierError(
                    "After inlining during processing meaning meta-modifiers, the number of arguments " + 
                    $"({inlinedArguments.Count}) became inconsistent with the built-in functor '{functor.Name}/{functor.Arity}'");
        }

        public static InvalidOperationException MetaModifierError(string message) =>  new (message);

        private static Rule Preprocess(Rule rule) => 
            Rule(Preprocess(rule.Conclusion), rule.Premises.Select(Preprocess));

        private static ComplexTerm Preprocess(ComplexTerm complexTerm) =>
            complexTerm with { Arguments = new (complexTerm.Arguments.Select(Preprocess)) };

        private static Term Preprocess(Term term) => 
            term switch 
            {
                ComplexTerm ct when ct.Functor.Name == "@variable" =>
                    Variable(
                        string.Join(
                            string.Empty, 
                            ct.Arguments
                                .Cast<Atom>(_ => 
                                    new InvalidOperationException($"Invalid use of {ct.Functor.Name} meta-modifier in {ct}. " + 
                                    "It can only accepts atoms as its arguments."))
                                .Select(a => Impl.Russian.TextInfo.ToTitleCase(a.Characters)))),
                _ => term
            };
                
        private static readonly string InlinerFunctorName = "I" + Guid.NewGuid().ToString("N");
   }
}