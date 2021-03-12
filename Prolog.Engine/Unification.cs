using System.Collections.Generic;
using System.Linq;
using Prolog.Engine.Miscellaneous;

namespace Prolog.Engine
{
    public static class Unification
    {
        public static UnificationResult CarryOut(Term leftTerm, Term rightTerm) =>
            (leftTerm, rightTerm) switch 
            {
                (Atom leftAtom, Atom rightAtom) => Result(leftAtom.Equals(rightAtom)),
                (Number leftNumber, Number rightNumber) => Result(leftNumber.Equals(rightNumber)),
                (Variable leftVariable, Variable rightVariable) when leftVariable.Equals(rightVariable) => Success(),
                (Variable leftVariable, _) => Success(leftVariable, rightTerm),
                (_, Variable rightVariable) => Success(rightVariable, leftTerm),
                (ComplexTerm leftComplexTerm, ComplexTerm rightComplexTerm) =>
                    !leftComplexTerm.Functor.Equals(rightComplexTerm.Functor)
                        ? Failure
                        : leftComplexTerm.Arguments
                            .Zip(rightComplexTerm.Arguments)
                            .AggregateWhile(
                                Success(),
                                (result, correspondingArguments) => 
                                    result.And(CarryOut(correspondingArguments.Item1, correspondingArguments.Item2)),
                                result => result.Succeeded),
                _ => Failure
            };

        public static bool IsPossible(Term leftTerm, Term rightTerm) =>
            (leftTerm, rightTerm) switch 
            {
                (Atom leftAtom, Atom rightAtom) => leftAtom.Equals(rightAtom),
                (Number leftNumber, Number rightNumber) => leftNumber.Equals(rightNumber),
                (Variable, _) => true,
                (_, Variable) => true,
                (ComplexTerm leftComplexTerm, ComplexTerm rightComplexTerm) =>
                    leftComplexTerm.Functor.Equals(rightComplexTerm.Functor) &&
                        leftComplexTerm.Arguments
                            .Zip(rightComplexTerm.Arguments)
                            .All(it => IsPossible(it.First, it.Second)),
                _ => false
            };

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