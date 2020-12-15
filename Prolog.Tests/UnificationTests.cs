using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prolog.Engine;
using static Prolog.Tests.PrologApi;
using static Prolog.Tests.StockTerms;

namespace Prolog.Tests
{
    [TestClass]
    public class UnificationTests
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
                                new[] { line(X, Something), line(atom, Something) },
                                new[] { line(Something, X), line(Something, atom) },
                                new[] { line(Something, X, SomethingElse), line(Something, atom, SomethingElse) }
                            }
                    },

                    new 
                    {
                        ExpectedUnification = X.InstantiatedTo(atom).And(Y.InstantiatedTo(one)),
                        TermPairs = new[]
                            {
                                new[] { line(X, one), line(atom, Y) },
                                new[] { line(Something, X, one), line(Something, atom, Y) },
                                new[] { line(X, Something, one), line(atom, Something, Y) },
                                new[] { line(X, one, Something), line(atom, Y, Something) },
                                new[] { line(Something, X, one, SomethingElse), line(Something, atom, Y, SomethingElse) },
                            }
                    },

                    new
                    {
                        ExpectedUnification = X.InstantiatedTo(atom),
                        TermPairs = new[]
                            {
                                new[] { line(point(X)), line(point(atom)) },
                                new[] { line(point(X), Something), line(point(atom), Something) },
                                new[] { line(Something, point(X)), line(Something, point(atom)) },
                                new[] { line(Something, point(X), SomethingElse), line(Something, point(atom), SomethingElse) },

                                new[] { line(point(X, Something)), line(point(atom, Something)) },
                                new[] { line(point(Something, X)), line(point(Something, atom)) },
                                new[] { line(point(Something, X, SomethingElse)), line(point(Something, atom, SomethingElse)) }
                            }
                    },

                    new
                    {
                        ExpectedUnification = X.InstantiatedTo(point(atom)),
                        TermPairs = new[]
                            {
                                new[] { line(X), line(point(atom)) },
                                new[] { line(X, Something), line(point(atom), Something) },
                                new[] { line(Something, X), line(Something, point(atom)) },
                                new[] { line(Something, X, SomethingElse), line(Something, point(atom), SomethingElse) }
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
                    new[] { line(X, Something), point(atom, Something) },
                    new[] { line(Something, X), point(Something, atom) },
                    new[] { line(Something, X, SomethingElse), point(Something, atom, SomethingElse) },

                    // incompatible arity
                    new[] { line(X), line(atom, Something) },
                    new[] { line(X, Something), line(atom) },
                    new[] { line(Something, X), line(atom) },
                    new[] { line(Something, X, SomethingElse), line(Something, atom) },

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
    }
}
