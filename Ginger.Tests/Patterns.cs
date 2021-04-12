using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Prolog.Engine;
using Prolog.Engine.Miscellaneous;
using Ginger.Runner;
using Ginger.Runner.Solarix;
using Prolog.Engine.Parsing;

namespace Ginger.Tests
{
    using static MakeCompilerHappy;
    using static Prolog.Engine.Parsing.MonadicParsing;
    using static Prolog.Engine.Parsing.PrologParser;
    using static Prolog.Tests.VerboseReporting;

    using SentenceMeaning = Either<IReadOnlyCollection<Rule>, IReadOnlyCollection<ComplexTerm>>;

    [Binding]
#pragma warning disable CA1812 // Your class is an internal class that is apparently never instantiated on Derived class
    internal sealed class Patterns
#pragma warning restore CA1812
    {
        public Patterns(            
            ScenarioContext scenarioContext,
            IRussianGrammarParser grammarParser,
            IRussianLexicon russianLexicon) 
        =>
            (_scenarioContext, _grammarParser, _russianLexicon) = (scenarioContext, grammarParser, russianLexicon);

        [Given("the following Patterns")]
        public void DefinePatterns(Table patterns)
        {
            SentenceUnderstander = new SentenceUnderstander(
                from r in patterns.GetMultilineRows()
                let generativePattern = new GenerativePattern(r["Id"], r["Pattern"], ParseMeaning(r["Meaning"]))
                from recognizer in generativePattern.GenerateConcreteUnderstanders(_grammarParser, _russianLexicon)
                select recognizer);
        }

        [Then("the following understandings should be possible")]
        public void CheckUnderstandings(Table understandings)
        {
            var situations = understandings.GetMultilineRows()
                .Select(r => new 
                            { 
                                Sentence = r["Sentence"], 
                                ExpectedMeaning = r["Meaning"],
                                RecognizedWithPattern = r["Recognized with Pattern"]
                            });
                
            var wrongUnderstandings = (
                    from situation in situations
                    let expectedMeaning = ParseMeaning(situation.ExpectedMeaning)
                    let understoodSentence = SentenceUnderstander.Understand(_grammarParser.ParsePreservingQuotes(situation.Sentence).Single())
                    where !understoodSentence
                            .Map(r => MeaningsAreEqual(expectedMeaning, r.Meaning) && situation.RecognizedWithPattern == r.PatternId)
                            .OrElse(false)
                    select new 
                    { 
                        situation.Sentence, 
                        ExpectedMeaning = Dump(expectedMeaning),
                        ActualMeaning = understoodSentence.Map(r => Dump(r.Meaning)).OrElse("Understanding failed"),
                        ExpectedPatternId = situation.RecognizedWithPattern,
                        ActualPatternId = understoodSentence.Map(r => r.PatternId).OrElse("n/a")
                    }
                ).AsImmutable();

            Assert.IsFalse(
                wrongUnderstandings.Any(),
                "The following sentences were processed incorrectly:" + Environment.NewLine + 
                string.Join(Environment.NewLine, wrongUnderstandings));
        }

        [Then("the following sentences should fail to be understood")]
        public void SentencesShouldFailToBeUnderstood(Table sentences)
        {
            var unexpectedUnderstandings = (
                    from sentence in sentences.GetMultilineRows().Select(r => r["Sentence"])
                    let understanding = SentenceUnderstander.Understand(_grammarParser.ParsePreservingQuotes(sentence).Single())
                    where understanding.HasValue
                    let understoodSentence = understanding.Value
                    select new 
                    { 
                        Sentence = sentence, 
                        understoodSentence.PatternId,
                        Meaning = Dump(understoodSentence.Meaning) 
                    }
                ).AsImmutable();

            Assert.IsFalse(
                unexpectedUnderstandings.Any(),
                "The following sentences were successfully understood while they shouldn't have:" + Environment.NewLine +
                string.Join(Environment.NewLine, unexpectedUnderstandings));
        }

        [Then("the following concrete patterns should be generated from a given generative patterns")]
        public void CheckApplicationOfGenerativePatterns(Table cases)
        {
            var invalidGenerations = (
                    from singleCase in cases.GetMultilineRows()
                    let generativePatternText = StreamlineText(singleCase["Generative Pattern"])
                    let expectedConcretePatterns = StreamlineText(singleCase["Resulting Concrete Patterns"])
                                                        .Split(';', StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(s => s.Trim())
                    let actualConcretePatterns = MakeGenerativePattern(generativePatternText)
                                                    .GenerateConcretePatterns(_grammarParser, _russianLexicon)
                                                    .Select(it => it.Pattern)
                    where !expectedConcretePatterns.SequenceEqual(actualConcretePatterns)
                    select new 
                    { 
                        GenerativePatternText = generativePatternText + Environment.NewLine, 
                        IncorrectPatterns = Environment.NewLine + string.Join(
                                                                    Environment.NewLine + Environment.NewLine, 
                                                                    expectedConcretePatterns
                                                                        .Zip(actualConcretePatterns)
                                                                        .Where(it => it.First != it.Second)
                                                                        .Select(it => $" +  !{it.First}!{Environment.NewLine} -  !{it.Second}!"))
                    }
                ).AsImmutable();

            Assert.IsFalse(
                invalidGenerations.Any(),
                "The following generative patterns produced invalid concrete pattern text " + Environment.NewLine + 
                string.Join(Environment.NewLine, invalidGenerations));

            string StreamlineText(string text) =>
                text.Replace(Environment.NewLine, " ").Trim();
        }

        [Then("Parsing of the following generative patterns should fail")]
        public void CheckExpectedParsingFailures(Table situations)
        {
            var invalidSituations = (
                from situation in situations.GetMultilineRows(singleLines: true)
                let patternText = situation["Pattern"]
                let expectedParsingError = situation["Failure reason"]
                let actualParsingError = GetParsingError(patternText)
                where !actualParsingError.Contains(expectedParsingError, StringComparison.OrdinalIgnoreCase)
                select new { patternText, expectedParsingError, actualParsingError }
                ).AsImmutable();

            Assert.IsFalse(
                invalidSituations.Any(),
                "The parsing of the following generative patterns was supposed to fail with a particural error message, " + 
                "but those patterns were either successfully parsed or the parsing error was not as expected:" + Environment.NewLine +
                string.Join(Environment.NewLine, invalidSituations));

            string GetParsingError(string pattern)
            {
                try
                {
                    SuppressCa1806(
                        MakeGenerativePattern(pattern)
                            .GenerateConcreteUnderstanders(_grammarParser, _russianLexicon));
                    return string.Empty;
                }
                catch (InvalidOperationException exception)
                {
                    return exception.Message;
                }
            }
        }

        private SentenceUnderstander SentenceUnderstander
        {
            get => (SentenceUnderstander)_scenarioContext[nameof(SentenceUnderstander)];
            set => _scenarioContext[nameof(SentenceUnderstander)] = value;
        }

        private static SentenceMeaning ParseMeaning(string meaning) =>
            Either(
                WholeInput(PrologParsers.ProgramParser), 
                WholeInput(PrologParsers.PremisesGroupParser))
                    (new TextInput(meaning, 0))
            .Fold(
                parsingError => throw new InvalidOperationException(parsingError.Text),
                meaning => meaning.Value);

        private static bool MeaningsAreEqual(SentenceMeaning expectedMeaning, SentenceMeaning actualMeaning) =>
            expectedMeaning.Fold(
                expectedRules => actualMeaning.Fold(expectedRules.SequenceEqual, _ => false),
                expectedStatements => actualMeaning.Fold(_ => false, expectedStatements.SequenceEqual));

        private static GenerativePattern MakeGenerativePattern(string generativePatternText) =>
            new (
                "unimportant", 
                generativePatternText, 
                new SentenceMeaning(Array.Empty<Rule>(), default, IsLeft: true));

        private readonly ScenarioContext _scenarioContext;
        private readonly IRussianGrammarParser _grammarParser;
        private readonly IRussianLexicon _russianLexicon;
    }
}