using System;
using System.Collections.Generic;
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
            var entityDefinitions = _sentenceUnderstander
                    .Understand(_grammarParser.ParsePreservingQuotes(phrasing))
                    .Map(understanding => understanding.Meaning.Fold(
                                rules => rules,
                                _ => throw new InvalidOperationException(
                                    "When defining entities, only rule-defining sentences are alowed. " +
                                    "You're trying to use the sentence '{phrasing}' which is undesrstood as a set of statements.")))
                    .OrElse(() => throw new InvalidOperationException($"Could not understand the phrase {phrasing}"));
            _entityDefinitions.AddRange(entityDefinitions);
            _program.AddRange(entityDefinitions);
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

        public SutSpecification BuildDescription()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _grammarParser.Dispose();
        }

        private readonly IRussianGrammarParser _grammarParser;
        private readonly SentenceUnderstander _sentenceUnderstander;
        private readonly List<Rule> _entityDefinitions = new ();
        private readonly List<Rule> _program = new ();
    }
}
