using System.Linq;

namespace Prolog.Engine
{
    public static class Unification
    {
        public static UnificationResult CarryOut(Term leftTerm, Term rightTerm)
        {
            if (leftTerm is Atom leftAtom && rightTerm is Atom rightAtom)
            {
                return new UnificationResult(Succeeded: leftAtom.Equals(rightAtom), Instantiations: NoInstantiations);
            }

            if (leftTerm is Number leftNumber && rightTerm is Number rightNumber)
            {
                return new UnificationResult(Succeeded: leftNumber.Equals(rightNumber), Instantiations: NoInstantiations);
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
                            new UnificationResult(Succeeded: true, NoInstantiations),
                            (result, correspondingArguments) => result.And(CarryOut(correspondingArguments.Item1, correspondingArguments.Item2)),
                            result => result.Succeeded);
            }

            return Failure;
        }

        public static bool IsPossible(Term leftTerm, Term rightTerm)
        {
            if (leftTerm is Atom leftAtom && rightTerm is Atom rightAtom)
            {
                return leftAtom.Equals(rightAtom);
            }

            if (leftTerm is Number leftNumber && rightTerm is Number rightNumber)
            {
                return leftNumber.Equals(rightNumber);
            }

            if (leftTerm is Variable || rightTerm is Variable)
            {
                return true;
            }

            if (leftTerm is ComplexTerm leftComplexTerm && rightTerm is ComplexTerm rightComplexTerm)
            {
                return leftComplexTerm.Functor.Equals(rightComplexTerm.Functor) &&
                       leftComplexTerm.Arguments
                        .Zip(rightComplexTerm.Arguments)
                        .All(it => IsPossible(it.First, it.Second));
            }

            return false;
       }

        public static readonly UnificationResult Success = new UnificationResult(Succeeded: true, Instantiations: new StructuralEquatableDictionary<Variable, Term>());

        public static readonly UnificationResult Failure = new UnificationResult(Succeeded: false, Instantiations: new StructuralEquatableDictionary<Variable, Term>());

        private static readonly StructuralEquatableDictionary<Variable, Term> NoInstantiations = new ();
    }
}