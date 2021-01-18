using System.Collections.Generic;
using System.Linq;
using Prolog.Engine;

using static Prolog.Engine.DomainApi;
using static Prolog.Engine.PrologParser;

namespace Ginger.Runner
{
    public static class BuisnessRuleChecker
    {
        public static IReadOnlyCollection<IGrouping<string, TestScenario>> GenerateTestScenarios(
            SutDescription sutDescription) =>
           (from businessRule in sutDescription.BusinessRules
            let query = ParseQuery($@"
                    начальноеСостояние(InitialState)
                    , FinalStateName = '{businessRule.FinalStateName}'
                    , конечноеСостояние(FinalStateName, FinalState)
                    , solve(InitialState, FinalStateName, FinalState, Solution)
                    , dictionaryValues(Solution, Route)")
            from proof in Proof.Find(sutDescription.Program, query)
            let scenario = new TestScenario(
                businessRule.FinalStateName, 
                new (proof.Instantiations[Variable("Route")].CastToList<ComplexTerm>()))
            group scenario by scenario.ExpectedOutcome into g
            select g)
            .AsImmutable();
    }
}