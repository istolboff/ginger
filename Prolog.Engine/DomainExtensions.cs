using System.Collections.Generic;
using System.Linq;

namespace Prolog.Engine
{
    public static class DomainExtensions
    {
        public static UnificationResult And(this UnificationResult @this, UnificationResult another)
        {
#pragma warning disable CA1062 // R# knows better
            if (!@this.Succeeded || !another.Succeeded)
#pragma warning restore CA1062
            {
                return Unification.Failure;
            }

            var (result, success) = @this.Instantiations
                        .Concat(another.Instantiations)
                        .GroupBy(i => i.Key)
                        .AggregateIfAll(
                            new Dictionary<Variable, Term>() as IDictionary<Variable, Term>,
                            variableInstantiations => variableInstantiations.All(i => i.Value.Equals(variableInstantiations.First().Value)),
                            (accumulatedInstantiations, variableInstantiations) => 
                                accumulatedInstantiations.AddAndReturnSelf(variableInstantiations.First()));

            return success ? Unification.Success(result) : Unification.Failure;
        }
    }
}