using System.Collections.Generic;
using System.Linq;

using static Prolog.Engine.MakeCompilerHappy;

namespace Prolog.Engine
{
    public static class DomainExtensions
    {
        public static UnificationResult And(this UnificationResult @this, UnificationResult another)
        {
            if (!SuppressCa1062(@this).Succeeded || !SuppressCa1062(another).Succeeded)
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

        public static bool IsList(this ComplexTerm @this) => 
            SuppressCa1062(@this) == Builtin.EmptyList || SuppressCa1062(@this).Functor.Equals(Builtin.DotFunctor);
    }
}