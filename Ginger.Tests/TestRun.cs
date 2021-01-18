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
            RussianGrammarParser = new SolarixParserMemoizer(new SolarixRussianGrammarEngine());
            SentenceUnderstander = SentenceUnderstander.LoadFromEmbeddedResources(RussianGrammarParser);
            PatternBuilder.PatternRecognitionEvent += (log, success) => LogPatternRecognitionEvent(log, success);
        }

        [AfterTestRun]
        public static void TeardownTestRun()
        {
            RussianGrammarParser?.Dispose();
        }

        [BeforeScenario]
        public void SetupDiContainer()
        {
            _diContainer.RegisterInstanceAs(RussianGrammarParser);
            _diContainer.RegisterInstanceAs(SentenceUnderstander);
        }

        [Conditional("UseLogging")]
        private static void LogPatternRecognitionEvent(string log, bool checkSucceeded)
        {
            Console.WriteLine($"Check for {log} {(checkSucceeded ? "succeeded" : "failed")}");
        }

        private readonly IObjectContainer _diContainer;

        private static IRussianGrammarParser? RussianGrammarParser;
        private static SentenceUnderstander? SentenceUnderstander; 
   }
}