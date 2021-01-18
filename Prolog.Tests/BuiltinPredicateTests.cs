using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prolog.Engine;

using static Prolog.Engine.Builtin;
using static Prolog.Engine.DomainApi;
using static Prolog.Tests.StockTerms;

using V = System.Collections.Generic.Dictionary<Prolog.Engine.Variable, Prolog.Engine.Term>;

namespace Prolog.Tests
{
    [TestClass]
    public class BuiltinPredicateTests : ProofTestsBase
    {
        [TestMethod]
        public void TestingEqualityPredicate()
        {
            var situations = new[]
            {
                (
                    Description: "Atoms comparisons",
                    LeftPart: a as Term,
                    RightPart: a as Term,
                    ExpectedProofs: new V()
                ),

                (
                    Description: "Simple variable assignment",
                    LeftPart: X as Term,
                    RightPart: a as Term,
                    ExpectedProofs: new V { [X] = a }
                ),

                (
                    Description: "Complex Terms with variables unification",
                    LeftPart: edge(X),
                    RightPart: edge(a),
                    ExpectedProofs: new V { [X] = a }
                ),

                (
                    Description: "Variables are unified with lists",
                    LeftPart: List(pair(edge(X, List(one, two, three)), Action)),
                    RightPart: List(pair(edge(List(a, b, c), List(one, two, three)), перевозит(капуста, левый, правый))),
                    ExpectedProofs: new V { [X] = List(a, b, c), [Action] = перевозит(капуста, левый, правый) }
                )
            };

            CheckSituations(
                from s in situations
                from query in new[] { Equal(s.LeftPart, s.RightPart), Equal(s.RightPart, s.LeftPart) }
                select (s.Description, System.Array.Empty<Rule>(), new[] { query }, new[] { s.ExpectedProofs }));
        }

        [TestMethod]
        public void TestingNonEqualityPredicate()
        {
            var situations = new[]
            {
                (
                    Description: "Atoms comparisons",
                    LeftPart: a as Term,
                    RightPart: b as Term,
                    ExpectedProofs: new V()
                ),
            };

            CheckSituations(
                from s in situations
                from query in new[] { NotEqual(s.LeftPart, s.RightPart), NotEqual(s.RightPart, s.LeftPart) }
                select (s.Description, System.Array.Empty<Rule>(), new[] { query }, new[] { s.ExpectedProofs }));
        }

        [TestMethod]
        public void RulesWithCut()
        {
            CheckSituations(new[]
            {
                (
                    Description: "Program without Cut",
                    Program: new[] 
                        { 
                            Fact(i(one)), 
                            Fact(i(two)), 
                            Fact(j(one)), 
                            Fact(j(two)),
                            Fact(j(three)),
                            Rule(s(X, Y), q(X, Y)), 
                            Fact(s(zero, zero)),
                            Rule(q(X, Y), i(X), j(Y)) 
                        },
                    Query: new[] { s(X, Y) },
                    ExpectedProofs: new[] 
                    { 
                        new V { [X] = one, [Y] = one },
                        new V { [X] = one, [Y] = two },
                        new V { [X] = one, [Y] = three },

                        new V { [X] = two, [Y] = one },
                        new V { [X] = two, [Y] = two },
                        new V { [X] = two, [Y] = three },

                        new V { [X] = zero, [Y] = zero }
                    }
                ),

                (
                    Description: "The same program as before, but this time with Cut",
                    Program: new[] 
                        { 
                            Fact(i(one)), 
                            Fact(i(two)), 
                            Fact(j(one)), 
                            Fact(j(two)),
                            Fact(j(three)),
                            Rule(s(X, Y), q(X, Y)), 
                            Fact(s(zero, zero)),
                            Rule(q(X, Y), i(X), Cut, j(Y)) 
                        },
                    Query: new[] { s(X, Y) },
                    ExpectedProofs: new[] 
                    { 
                        new V { [X] = one, [Y] = one },
                        new V { [X] = one, [Y] = two },
                        new V { [X] = one, [Y] = three },

                        new V { [X] = zero, [Y] = zero }
                    }
                ),

                (
                    Description: "Classical Max",
                    Program: new[]
                    {
                        Rule(max(X, Y, X), GreaterThanOrEqual(X, Y), Cut),
                        Rule(max(X, Y, Y), LessThan(X, Y)),
                        Fact(number(one)),
                        Fact(number(two)),
                        Fact(number(three))
                    },
                    Query: new[] { number(X), number(Y), max(X, Y, X) },
                    ExpectedProofs: new[] 
                    {
                        new V { [X] = one, [Y] = one },

                        new V { [X] = two, [Y] = one },
                        new V { [X] = two, [Y] = two },

                        new V { [X] = three, [Y] = one },
                        new V { [X] = three, [Y] = two },
                        new V { [X] = three, [Y] = three }
                    }
                ),

                (
                    Description: "1st query from the 1st exercise from http://lpn.swi-prolog.org/lpnpage.php?pagetype=html&pageid=lpn-htmlse46",
                    Program: new[] 
                    {
                        Fact(p(one)),
                        Rule(p(two), Cut),
                        Fact(p(three))
                    },
                    Query: new[] { p(X) },
                    ExpectedProofs: new[] 
                    {
                        new V { [X] = one },
                        new V { [X] = two }
                    }
                ),

                (
                    Description: "2nd query from the 1st exercise from http://lpn.swi-prolog.org/lpnpage.php?pagetype=html&pageid=lpn-htmlse46",
                    Program: new[] 
                    {
                        Fact(p(one)),
                        Rule(p(two), Cut),
                        Fact(p(three))
                    },
                    Query: new[] { p(X), p(Y) },
                    ExpectedProofs: new[] 
                    {
                        new V { [X] = one, [Y] = one },
                        new V { [X] = one, [Y] = two },
                        new V { [X] = two, [Y] = one },
                        new V { [X] = two, [Y] = two }
                    }
                ),

                (
                    Description: "3rd query from the 1st exercise from http://lpn.swi-prolog.org/lpnpage.php?pagetype=html&pageid=lpn-htmlse46",
                    Program: new[] 
                    {
                        Fact(p(one)),
                        Rule(p(two), Cut),
                        Fact(p(three))
                    },
                    Query: new[] { p(X), Cut, p(Y) },
                    ExpectedProofs: new[] 
                    {
                        new V { [X] = one, [Y] = one },
                        new V { [X] = one, [Y] = two }
                    }
                ),

                (
                    Description: "Fixed bug test",
                    Program: new[]
                    {
                        Fact(p(two)),
                        Fact(p(three)),
                        Fact(q(one)),
                        Fact(q(two)),
                        Fact(q(three)),
                        Rule(g(X), q(X), p(X), Cut),
                        Rule(s(X), g(X))
                    },
                    Query: new[] { s(X) },
                    ExpectedProofs: new[] 
                    {
                        new V { [X] = two }
                    }
                )
            });
        }

        [TestMethod]
        public void RulesWithNot()
        {
            CheckSituations(new[]
            {
                (
                    Description: "Example from http://lpn.swi-prolog.org/lpnpage.php?pagetype=html&pageid=lpn-htmlse45",
                    Program: new[]
                    {
                        Rule(enjoys(vincent,X), burger(X), Not(big_kahuna_burger(X))),

                        Rule(burger(X), big_mac(X)),
                        Rule(burger(X), big_kahuna_burger(X)),
                        Rule(burger(X), whopper(X)),

                        Fact(big_mac(a)),
                        Fact(big_kahuna_burger(b)),
                        Fact(big_mac(c)),
                        Fact(whopper(d))
                    },
                    Query: new[] { enjoys(vincent,X) },
                    ExpectedProofs: new[] 
                    {
                        new V { [X] = a },
                        new V { [X] = c },
                        new V { [X] = d }
                    }
                )
            });
        }

        [TestMethod]
        public void TestingFindAll()
        {
            var edgeFacts = new[]
                {
                    Fact(edge(a, one)),
                    Fact(edge(a, two)),
                    Fact(edge(a, three)),
                    Fact(edge(b, ten)),
                    Fact(edge(b, twenty)),
                    Fact(edge(b, thirty)),
                };

            CheckSituations(new[]
            {
                (
                    Description: "findall() returns an empty list when there're no solutions to the goal",
                    Program: edgeFacts,
                    Query: new[] { FindAll(X, edge(c, X), A) },
                    ExpectedSolutions: new[] 
                    {
                        new V { [A] = EmptyList }
                    }
                ),

                (
                    Description: "findall() with single plain related variable product",
                    Program: edgeFacts,
                    Query: new[] { FindAll(X, edge(b, X), A) },
                    ExpectedSolutions: new[] 
                    {
                        new V { [A] = List(ten, twenty, thirty) }
                    }
                ),

                (
                    Description: "findall() with complex term of single related variable product",
                    Program: edgeFacts,
                    Query: new[] { FindAll(state(X), edge(a, X), A) },
                    ExpectedSolutions: new[] 
                    {
                        new V { [A] = List(state(one), state(two), state(three)) }
                    }
                ),

                (
                    Description: "findall() with single unrelated plain variable product",
                    Program: edgeFacts,
                    Query: new[] { FindAll(state(Y), edge(a, X), A) },
                    ExpectedSolutions: new[] 
                    {
                        new V { [A] = List(state(_), state(_), state(_)) }
                    }
                ),

                (
                    Description: "findall() with complex term of two variables product",
                    Program: edgeFacts,
                    Query: new[] { FindAll(state(Y, X), edge(X, Y), A) },
                    ExpectedSolutions: new[] 
                    {
                        new V { [A] = List(state(one, a), state(two, a), state(three, a), state(ten, b), state(twenty, b), state(thirty, b)) }
                    }
                )
            });
        }
    }
}