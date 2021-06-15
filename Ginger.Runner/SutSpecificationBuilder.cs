using System;
using System.Collections.Generic;
using System.Linq;
using Ginger.Runner.Solarix;
using Prolog.Engine;
using Prolog.Engine.Miscellaneous;

namespace Ginger.Runner
{
    using SentenceMeaning = Either<IReadOnlyCollection<Rule>, IReadOnlyCollection<ComplexTerm>>;
    
    internal sealed class SutSpecificationBuilder
    {
        public SutSpecificationBuilder(IRussianGrammarParser grammarParser, SentenceUnderstander sentenceUnderstander)
        {
            _grammarParser = grammarParser;
            _sentenceUnderstander = sentenceUnderstander;
        }

        public void DefineEntity(string phrasing)
        {
            var entityDefinitions = Understand(phrasing)
                    .Fold(
                        rules => rules,
                        _ => throw new InvalidOperationException(
                            "When defining entities, only rule-defining sentences are alowed. " +
                            $"You're trying to use the sentence '{phrasing}' which is undesrstood as a set of statements."));
            _entityDefinitions.AddRange(entityDefinitions);
        }

        public void DefineEffect(string phrasing)
        {
            var effectAsRules = Understand(phrasing)
                    .Fold(
                        rules => rules,
                        _ => throw new InvalidOperationException(
                            "When defining effects, only rule-defining sentences are alowed. " +
                            $"You're trying to use the sentence '{phrasing}' which is undesrstood as a set of statements."));

            _effects.AddRange(effectAsRules.Select(MoveEntityDefinitionsFromConclusionToPremises));

            Rule MoveEntityDefinitionsFromConclusionToPremises(Rule rule)
            {

            }
        }

        public void DefineBoundaryCondition(string phrasing)
        {
            throw new NotImplementedException();
        }

        public void DefineBusinessRule(string phrasing)
        {
            throw new NotImplementedException();
        }

        public SutSpecification BuildDescription() =>
            new (
                _effects.Concat(_effects).AsImmutable(),
                Array.Empty<BusinessRule>(),
                _effects,
                new ());

        private SentenceMeaning Understand(string phrasing) =>
            _sentenceUnderstander
                .Understand(_grammarParser.ParsePreservingQuotes(phrasing))
                .OrElse(() => throw new InvalidOperationException($"Could not understand the phrase {phrasing}"))
                .Meaning;

        private readonly IRussianGrammarParser _grammarParser;
        private readonly SentenceUnderstander _sentenceUnderstander;
        private readonly List<Rule> _entityDefinitions = new ();
        private readonly List<Rule> _effects = new ();
    }
}