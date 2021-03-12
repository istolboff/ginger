using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Prolog.Engine.Miscellaneous;
using Ginger.Runner.Solarix;
using Ginger.Runner;

namespace Ginger.Tests
{
    using static Prolog.Engine.Parsing.PrologParser;
    using static Prolog.Tests.VerboseReporting;
    
    [Binding]
#pragma warning disable CA1812 // Your class is an internal class that is apparently never instantiated on Derived class
    internal sealed class Patterns
#pragma warning restore CA1812
    {
        public Patterns(            
            ScenarioContext scenarioContext,
            IRussianGrammarParser grammarParser)
        {
            _scenarioContext = scenarioContext;
            _grammarParser = grammarParser;
        }

        [Given("the following Patterns")]
        public void DefinePatterns(Table patterns)
        {
            SentenceUnderstander = new SentenceUnderstander(
                patterns.GetMultilineRows().Select(r => PatternBuilder.BuildPattern(
                    r["Id"],
                    _grammarParser.ParseAnnotated(r["Pattern"]),
                     ParseProgram(r["Meaning"]))));
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
                    let expectedMeaning = ParseProgram(situation.ExpectedMeaning)
                    let understoodSentence = SentenceUnderstander.Understand(_grammarParser.ParsePreservingQuotes(situation.Sentence).Single())
                    where !understoodSentence
                            .Map(r => expectedMeaning.SequenceEqual(r.Meaning) && situation.RecognizedWithPattern == r.PatternId)
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
                    from sentence in sentences.Rows.Select(r => r["Sentence"])
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

        private SentenceUnderstander SentenceUnderstander
        {
            get => (SentenceUnderstander)_scenarioContext[nameof(SentenceUnderstander)];
            set => _scenarioContext[nameof(SentenceUnderstander)] = value;
        }

        private readonly ScenarioContext _scenarioContext;
        private readonly IRussianGrammarParser _grammarParser;
   }
}