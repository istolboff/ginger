using System.Collections.Generic;
using System.Linq;

namespace Prolog.Engine
{
    public static class Unification
    {
        public static UnificationResult CarryOut(Term leftTerm, Term rightTerm)
        {
            if (leftTerm is Atom leftAtom && rightTerm is Atom rightAtom)
            {
                return Result(leftAtom.Equals(rightAtom));
            }

            if (leftTerm is Number leftNumber && rightTerm is Number rightNumber)
            {
                return Result(leftNumber.Equals(rightNumber));
            }

            if (leftTerm is Variable leftVariable)
            {
                return Success(leftVariable, rightTerm);
            }

            if (rightTerm is Variable rightVariable)
            {
                return Success(rightVariable, leftTerm);
            }

            if (leftTerm is ComplexTerm leftComplexTerm && rightTerm is ComplexTerm rightComplexTerm)
            {
                return !leftComplexTerm.Functor.Equals(rightComplexTerm.Functor)
                    ? Failure
                    : leftComplexTerm.Arguments
                        .Zip(rightComplexTerm.Arguments)
                        .AggregateWhile(
                            Success(),
                            (result, correspondingArguments) => 
                                result.And(CarryOut(correspondingArguments.Item1, correspondingArguments.Item2)),
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

        public static UnificationResult Result(bool succeeded) => 
            new (Succeeded: succeeded, Instantiations: new ());

        public static UnificationResult Success() => 
            Result(true);

        public static UnificationResult Success(Variable variable, Term value) =>
            Success(Enumerable.Repeat(KeyValuePair.Create(variable, value), 1));

        public static UnificationResult Success(IEnumerable<KeyValuePair<Variable, Term>> variableInstantiations) => 
            new (Succeeded: true, Instantiations: new(variableInstantiations));

        public static readonly UnificationResult Failure = Result(false);
    }
}