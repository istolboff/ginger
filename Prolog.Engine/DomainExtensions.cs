using System.Linq;

namespace Prolog.Engine
{
    public static class DomainExtensions
    {
        public static UnificationResult InstantiatedTo(this Variable @this, Term term) =>
            new UnificationResult(
                Succeeded: true,
                Instantiations: new StructuralEquatableDictionary<Variable, Term> { [@this] = term });

        public static UnificationResult And(this UnificationResult @this, UnificationResult another)
        {
            if (!(@this?.Succeeded ?? false) || !(another?.Succeeded ?? false))
            {
                return Unification.Failure;
            }

            var (result, success) = @this.Instantiations
                        .Concat(another.Instantiations)
                        .GroupBy(i => i.Key)
                        .AggregateIfAll(
                            new StructuralEquatableDictionary<Variable, Term>(),
                            variableInstantiations => variableInstantiations.All(i => i.Value.Equals(variableInstantiations.First().Value)),
                            (accumulatedInstantiations, variableInstantiations) =>
                            {
                                accumulatedInstantiations.Add(variableInstantiations.First());
                                return accumulatedInstantiations;
                            });

            return success ? new UnificationResult(true, result) : Unification.Failure;
        }
    }
}