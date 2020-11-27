using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prolog.Engine;
using static Prolog.Tests.PrologApi;

namespace Prolog.Tests
{
    [TestClass]
    public class GivenPrologInterpreter
    {
        // If term1 and term2 are constants, then term1 and term2 unify if and only if 
        // they are the same atom, or the same number.
        [TestMethod]
        public void ConstantsUnification()
        {
            Assert.IsTrue(Unification.CarryOut(Atom("some-atom"), Atom("some-atom")).Succeeded);
            Assert.IsFalse(Unification.CarryOut(Atom("some-atom"), Atom("some-other-atom")).Succeeded);
            Assert.IsTrue(Unification.CarryOut(Number(42), Number(42)).Succeeded);
            Assert.IsFalse(Unification.CarryOut(Number(42), Number(21)).Succeeded);
            Assert.IsFalse(Unification.CarryOut(Atom("2"), Number(2)).Succeeded);
        }

        // If term1 is a variable and term2 is any type of term, then term1 and term2 unify, 
        // and term1 is instantiated to term2 . Similarly, if term2 is a variable and term1 
        // is any type of term, then term1 and term2 unify, and term2 is instantiated to term1 
        [TestMethod]
        public void UnificationOfVariableWithAnyTypeOfTermButVariable()
        {
            var wrongUnifications = 
                (from term in new Term[] 
                    {
                        Atom("atom"),
                        Number(42),
                        // List(Atom("a"), Atom("b"), Number(42)),
                        ComplexTerm(Functor("hide", 3), Atom("X"), Number(42), ComplexTerm(Functor("father", 1), Atom("butch")))
                    }
                let variable = Variable("X")
                from arguments in new (Term, Term)[] { (variable, term), (term, variable) }
                let unification = Unification.CarryOut(arguments.Item1, arguments.Item2)
                where !unification.Succeeded || !unification.Instantiations[variable].Equals(term)
                select new { arguments.Item1, arguments.Item2, Unification = unification })
                .ToList();

            Assert.IsFalse(wrongUnifications.Any(), Environment.NewLine + string.Join(Environment.NewLine, wrongUnifications));
        }

        // (So if they are both variables, they’re both instantiated to each other, and we say that they share values.)
        [TestMethod]
        public void UnificationOfVariableWithAnotherVariable()
        {
            Assert.IsTrue(X.InstantiatedTo(Y).Equals(Unification.CarryOut(Variable("X"), Variable("Y"))));
        }

        // If term1 and term2 are complex terms, then they unify if and only if:
        //       They have the same functor and arity, and
        //       all their corresponding arguments unify, and
        //       the variable instantiations are compatible. 
        //       (For example, it is not possible to instantiate variable X to mia when 
        //        unifying one pair of arguments, and to instantiate X to vincent when
        //        unifying another pair of arguments .)
        [TestMethod]
        public void SuccessfulUnificationOfFunctor()
        {
            var wrongUnifications =
                (from test in new[]
                {
                    new 
                    {
                        ExpectedUnification = X.InstantiatedTo(atom),
                        TermPairs = new[]
                            {
                                new[] { line(X), line(atom) },
                                new[] { line(X, _), line(atom, _) },
                                new[] { line(_, X), line(_, atom) },
                                new[] { line(_, X, __), line(_, atom, __) }
                            }
                    },

                    new 
                    {
                        ExpectedUnification = X.InstantiatedTo(atom).And(Y.InstantiatedTo(one)),
                        TermPairs = new[]
                            {
                                new[] { line(X, one), line(atom, Y) },
                                new[] { line(_, X, one), line(_, atom, Y) },
                                new[] { line(X, _, one), line(atom, _, Y) },
                                new[] { line(X, one, _), line(atom, Y, _) },
                                new[] { line(_, X, one, __), line(_, atom, Y, __) },
                            }
                    },

                    new
                    {
                        ExpectedUnification = X.InstantiatedTo(atom),
                        TermPairs = new[]
                            {
                                new[] { line(point(X)), line(point(atom)) },
                                new[] { line(point(X), _), line(point(atom), _) },
                                new[] { line(_, point(X)), line(_, point(atom)) },
                                new[] { line(_, point(X), __), line(_, point(atom), __) },

                                new[] { line(point(X, _)), line(point(atom, _)) },
                                new[] { line(point(_, X)), line(point(_, atom)) },
                                new[] { line(point(_, X, __)), line(point(_, atom, __)) }
                            }
                    },

                    new
                    {
                        ExpectedUnification = X.InstantiatedTo(point(atom)),
                        TermPairs = new[]
                            {
                                new[] { line(X), line(point(atom)) },
                                new[] { line(X, _), line(point(atom), _) },
                                new[] { line(_, X), line(_, point(atom)) },
                                new[] { line(_, X, __), line(_, point(atom), __) }
                            }
                    }
                }
                from termPair in test.TermPairs
                from arguments in new (ComplexTerm, ComplexTerm)[] { (termPair[0], termPair[1]), (termPair[1], termPair[0]) }
                let unification = Unification.CarryOut(arguments.Item1, arguments.Item2)
                where !unification.Succeeded || !unification.Equals(test.ExpectedUnification)
                select new { arguments.Item1, arguments.Item2, test.ExpectedUnification, ActualUnification = unification })
                .ToList();

            Assert.IsFalse(wrongUnifications.Any(), Environment.NewLine + string.Join(Environment.NewLine, wrongUnifications));
        }

        [TestMethod]
        public void UnsuccessfulUnificationOfFunctor()
        {
            var wrongUnifications =
                (from termPair in new[]
                {
                    // incompatible functors
                    new[] { line(X), point(atom) },
                    new[] { line(X, _), point(atom, _) },
                    new[] { line(_, X), point(_, atom) },
                    new[] { line(_, X, __), point(_, atom, __) },

                    // incompatible arity
                    new[] { line(X), line(atom, _) },
                    new[] { line(X, _), line(atom) },
                    new[] { line(_, X), line(atom) },
                    new[] { line(_, X, __), line(_, atom) },

                    //  variable instantiations are incompatible
                    new[] { line(X, X), line(atom, one) },
                    new[] { line(point(X), X), line(point(atom), one) },
                    new[] { line(X, point(X)), line(one, point(atom)) }
                }
                from arguments in new (ComplexTerm, ComplexTerm)[] { (termPair[0], termPair[1]), (termPair[1], termPair[0]) }
                let unification = Unification.CarryOut(arguments.Item1, arguments.Item2)
                where unification.Succeeded
                select new { Arguments = arguments, Unification = unification })
                .ToList();

            Assert.IsFalse(wrongUnifications.Any(), string.Join(Environment.NewLine, wrongUnifications));
        }

        [TestMethod]
        public void UnificationWhenVariablesGetSubstitutedOnBothSides()
        {
            var wrongUnifications = 
                (from test in new[] 
                {
                    new 
                    { 
                        ExpectedUnification = X.InstantiatedTo(X1).And(Y.InstantiatedTo(atom)).And(Z.InstantiatedTo(one)),
                        TermPair = (date(X, Y, one), date(X1, atom, Z))
                    },
                    new
                    {
                        ExpectedUnification = Unification.Failure,
                        TermPair = (point(one, X), point(X, two))
                    }
                }
                let unification = Unification.CarryOut(test.TermPair.Item1, test.TermPair.Item2)
                where !unification.Equals(test.ExpectedUnification)
                select new { test.TermPair.Item1, test.TermPair.Item2, test.ExpectedUnification, ActualUnification = unification })
                .ToList();

            Assert.IsFalse(wrongUnifications.Any(), string.Join(Environment.NewLine, wrongUnifications));
        }

        private static ComplexTerm date(params Term[] arguments) => ComplexTerm(Functor("date", arguments.Length), arguments);

        private static ComplexTerm line(params Term[] arguments) => ComplexTerm(Functor("line", arguments.Length), arguments);

        private static ComplexTerm point(params Term[] arguments) => ComplexTerm(Functor("point", arguments.Length), arguments);

        private static ComplexTerm vertical(params Term[] arguments) => ComplexTerm(Functor("vertical", arguments.Length), arguments);

        private static readonly Variable X = Variable("X");
        private static readonly Variable X1 = Variable("X1");
        private static readonly Variable Y = Variable("Y");
        private static readonly Variable Z = Variable("Z");
        private static readonly Variable P = Variable("P");
        private static readonly Atom atom = Atom("atom");
        private static readonly Number one = Number(1);
        private static readonly Number two = Number(2);
        private static readonly Term _ = Number(3457);
        private static readonly Term __ = Atom("something unimportant");
    }
}
