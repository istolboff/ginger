using System.Collections.Generic;
using Prolog.Engine;
using Prolog.Engine.Miscellaneous;

namespace Ginger.Runner
{
    using SentenceMeaning = Either<IReadOnlyCollection<Rule>, IReadOnlyCollection<ComplexTerm>>;

    internal sealed record UnderstoodSentence(string PatternId, SentenceMeaning Meaning);

    public sealed record BusinessRule(string FinalStateName);

    public sealed record SutDescription(
        IReadOnlyCollection<Rule> Program,
        IReadOnlyCollection<BusinessRule> BusinessRules,
        IReadOnlyCollection<object> Effects, 
        object InitialState);

    public sealed record TestScenario(
        string ExpectedOutcome, 
        StructuralEquatableArray<ComplexTerm> Steps);
}