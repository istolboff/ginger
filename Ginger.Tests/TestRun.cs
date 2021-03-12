using System;
using System.Diagnostics;
using BoDi;
using Ginger.Runner;
using Ginger.Runner.Solarix;
using TechTalk.SpecFlow;

namespace Ginger.Tests
{
    [Binding]
#pragma warning disable CA1812 // Your class is an internal class that is apparently never instantiated on Derived class    
    internal sealed class TestRun
#pragma warning restore CA1812
    {
        public TestRun(IObjectContainer diContainer)
        {
            _diContainer = diContainer;
        }

        [BeforeTestRun]
        public static void SetupTestRun()
        {
            _russianGrammarParser = new SolarixParserMemoizer(new SolarixRussianGrammarEngine());
            _sentenceUnderstander = SentenceUnderstander.LoadFromEmbeddedResources(_russianGrammarParser);
            PatternBuilder.PatternRecognitionEvent += (log, success) => LogPatternRecognitionEvent(log, success);
        }

        [AfterTestRun]
        public static void TeardownTestRun()
        {
            _russianGrammarParser?.Dispose();
        }

        [BeforeScenario]
        public void SetupDiContainer()
        {
            _diContainer.RegisterInstanceAs(_russianGrammarParser);
            _diContainer.RegisterInstanceAs(_sentenceUnderstander);
        }

        [Conditional("UseLogging")]
        private static void LogPatternRecognitionEvent(string log, bool checkSucceeded)
        {
            Console.WriteLine($"Check for {log} {(checkSucceeded ? "succeeded" : "failed")}");
        }

        private readonly IObjectContainer _diContainer;

        private static IRussianGrammarParser? _russianGrammarParser;
        private static SentenceUnderstander? _sentenceUnderstander; 
   }
}