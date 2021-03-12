using System;
using System.Collections.Generic;
using System.Linq;
using Ginger.Runner.Solarix;
using Prolog.Engine;

namespace Ginger.Runner
{
    internal sealed class SutDescriptionBuilder : IDisposable
    {
        public SutDescriptionBuilder(IRussianGrammarParser grammarParser, SentenceUnderstander sentenceUnderstander)
        {
            _grammarParser = grammarParser;
            _sentenceUnderstander = sentenceUnderstander;
        }

        public void DefineEntity(string phrasing)
        {
            _program.AddRange(
                _sentenceUnderstander
                    .Understand(_grammarParser.ParsePreservingQuotes(phrasing).Single())
                    .Map(understanding => understanding.Meaning)
                    .OrElse(() => throw new InvalidOperationException($"Could not understand the phrase {phrasing}")));
        }

        public void DefineEffect(string phrasing)
        {
            throw new NotImplementedException();
        }

        public void DefineBoundaryCondition(string phrasing)
        {
            throw new NotImplementedException();
        }

        public void DefineBusinessRule(string phrasing)
        {
            throw new NotImplementedException();
        }

        public SutDescription BuildDescription()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _grammarParser.Dispose();
        }

        private readonly IRussianGrammarParser _grammarParser;
        private readonly SentenceUnderstander _sentenceUnderstander;
        private readonly List<Rule> _program = new ();
    }
}
