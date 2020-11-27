using System.Linq;

namespace Prolog.Engine
{
    public static class Unification
    {
        public sealed record Result(bool Succeeded, StructuralEquatableDictionary<Variable, Term> Instantiations);

        public static Result CarryOut(Term leftTerm, Term rightTerm)
        {
            if (leftTerm is Atom leftAtom && rightTerm is Atom rightAtom)
            {
                return new Result(Succeeded: leftAtom.Equals(rightAtom), Instantiations: NoInstantiations);
            }

            if (leftTerm is Number leftNumber && rightTerm is Number rightNumber)
            {
                return new Result(Succeeded: leftNumber.Equals(rightNumber), Instantiations: NoInstantiations);
            }

            if (leftTerm is Variable leftVariable)
            {
                return leftVariable.InstantiatedTo(rightTerm);
            }

            if (rightTerm is Variable rightVariable)
            {
                return rightVariable.InstantiatedTo(leftTerm);
            }

            if (leftTerm is ComplexTerm leftComplexTerm && rightTerm is ComplexTerm rightComplexTerm)
            {
                return !leftComplexTerm.Functor.Equals(rightComplexTerm.Functor)
                    ? Failure
                    : leftComplexTerm.Arguments
                        .Zip(rightComplexTerm.Arguments)
                        .AggregateWhile(
                            new Result(Succeeded: true, NoInstantiations),
                            (result, correspondingArguments) => result.And(CarryOut(correspondingArguments.Item1, correspondingArguments.Item2)),
                            result => result.Succeeded);
            }

            return Failure;
        }

        public static readonly Result Failure = new Result(Succeeded: false, Instantiations: new StructuralEquatableDictionary<Variable, Term>());

        private static readonly StructuralEquatableDictionary<Variable, Term> NoInstantiations = new();
    }
}