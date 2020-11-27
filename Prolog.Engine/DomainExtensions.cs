using System.Linq;

namespace Prolog.Engine
{
    public static class DomainExtensions
    {
        public static Unification.Result InstantiatedTo(this Variable @this, Term term) => 
            new Unification.Result(
                Succeeded: true, 
                Instantiations: new StructuralEquatableDictionary<Variable, Term> { [@this] = term });

        public static Unification.Result And(this Unification.Result @this, Unification.Result another) 
        {
            if (!@this.Succeeded || !another.Succeeded)
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
                            }
                        );

            return success ? new Unification.Result(true, result) : Unification.Failure;
        }
    }
}