using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Prolog.Engine;
using Ginger.Runner;
using Ginger.Runner.Solarix;

namespace Ginger.Tests.StepDefinitions
{
    using static Prolog.Engine.Parsing.PrologParser;
    using static BuisnessRuleChecker;

    [Binding]
#pragma warning disable CA1812 // Your class is an internal class that is apparently never instantiated on Derived class    
    internal sealed class Sut
#pragma warning restore CA1812
    {
        public Sut(
            ScenarioContext scenarioContext,
            IRussianGrammarParser grammarParser,
            SentenceUnderstander sentenceUnderstander)
        {
            _scenarioContext = scenarioContext;
            _grammarParser = grammarParser;
            _sentenceUnderstander = sentenceUnderstander;
        }

        public SutSpecification SutSpecification
        {
            get => (SutSpecification)_scenarioContext[nameof(SutSpecification)];
            set => _scenarioContext[nameof(SutSpecification)] = value;
        }

        [Given("SUT is described as follows")]
        public void SutIsDescribedAs(Table description)
        {
            SutSpecification = ParseTextDescripton(description);
        }

        [Then("the following scenarios should be generated")]
        public void FollowingScenariosShouldBeGenerated(Table expectedScenarios)
        {
            var es = 
                from row in expectedScenarios.GetMultilineRows()
                from scenario in ParseTerm(row["Scenario Steps"])
                                    .CastToList<ComplexTerm>()
                                    .Select(route => new TestScenario(row["Expected Outcome"], new (route)))
                group scenario by scenario.ExpectedOutcome into g
                select g;
            var generatedScenarios = GenerateTestScenarios(SutSpecification);
            Assert.IsTrue(
                es.OrderBy(s => s.Key).Select(g => g)
                    .SequenceEqual(generatedScenarios.OrderBy(s => s.Key).Select(g => g)));
        }

        private SutSpecification ParseTextDescripton(Table description)
        {
            using var sutDescriptionBuilder = new SutDescriptionBuilder(_grammarParser, _sentenceUnderstander);
            foreach (var row in description.GetMultilineRows())
            {
                switch (row["Type"])
                {
                    case "Сущность": sutDescriptionBuilder.DefineEntity(row["Phrasing"]); break;
                    case "Воздействие": sutDescriptionBuilder.DefineEffect(row["Phrasing"]); break;
                    case "Граничное условие": sutDescriptionBuilder.DefineBoundaryCondition(row["Phrasing"]); break;
                    case "Правило": sutDescriptionBuilder.DefineBusinessRule(row["Phrasing"]); break;
                    default: throw new InvalidOperationException($"Unsupported type '{row["Type"]}' of SUT description fragment");
                }
            }

            return sutDescriptionBuilder.BuildDescription();
        }

        private readonly ScenarioContext _scenarioContext;
        private readonly IRussianGrammarParser _grammarParser;
        private readonly SentenceUnderstander _sentenceUnderstander;
    }
}