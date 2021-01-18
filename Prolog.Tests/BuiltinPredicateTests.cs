using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using V = System.Collections.Generic.Dictionary<string, string>;

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
                    LeftPart: "a",
                    RightPart: "a",
                    ExpectedProofs: new V()
                ),

                (
                    Description: "Simple variable assignment",
                    LeftPart: "X",
                    RightPart: "a",
                    ExpectedProofs: new V { ["X"] = "a" }
                ),

                (
                    Description: "Complex Terms with variables unification",
                    LeftPart: "edge(X)",
                    RightPart: "edge(a)",
                    ExpectedProofs: new V { ["X"] = "a" }
                ),

                (
                    Description: "Variables are unified with lists",
                    LeftPart:  "[pair(edge(X,[1, 2, 3]), Action)]",
                    RightPart: "[pair(edge([a, b, c], [1, 2, 3]), перевозит(капуста, левый, правый))]",
                    ExpectedProofs: new V { ["X"] = "[a, b, c]", ["Action"] = "перевозит(капуста, левый, правый)" }
                )
            };

            CheckSituations(
                from s in situations
                from query in new[] { $"{s.LeftPart} = {s.RightPart}", $"{s.RightPart} = {s.LeftPart}" }
                select (s.Description, string.Empty, query, new[] { s.ExpectedProofs }));
        }

        [TestMethod]
        public void TestingNonEqualityPredicate()
        {
            var situations = new[]
            {
                (
                    Description: "Atoms comparisons",
                    LeftPart: "a",
                    RightPart: "b",
                    ExpectedProofs: new V()
                ),
            };

            CheckSituations(
                from s in situations
                from query in new[] { $"{s.LeftPart} \\= {s.RightPart}", $"{s.RightPart} \\= {s.LeftPart}" }
                select (s.Description, string.Empty, query, new[] { s.ExpectedProofs }));
        }

        [TestMethod]
        public void RulesWithCut()
        {
            CheckSituations(new[]
            {
                (
                    Description: "Program without Cut",
                    Program: @"
                            i(one).
                            i(two).
                            j(one).
                            j(two).
                            j(three).
                            s(X, Y) :- q(X, Y).
                            s(zero, zero).
                            q(X, Y) :- i(X), j(Y).",
                    Query: "s(X, Y)",
                    ExpectedProofs: new[] 
                    { 
                        new V { ["X"] = "one", ["Y"] = "one" },
                        new V { ["X"] = "one", ["Y"] = "two" },
                        new V { ["X"] = "one", ["Y"] = "three" },

                        new V { ["X"] = "two", ["Y"] = "one" },
                        new V { ["X"] = "two", ["Y"] = "two" },
                        new V { ["X"] = "two", ["Y"] = "three" },

                        new V { ["X"] = "zero", ["Y"] = "zero" }
                    }
                ),

                (
                    Description: "The same program as before, but this time with Cut",
                    Program: @"
                            i(one). 
                            i(two).
                            j(one).
                            j(two).
                            j(three).
                            s(X, Y) :- q(X, Y).
                            s(zero, zero).
                            q(X, Y) :- i(X), !, j(Y).",
                    Query: "s(X, Y)",
                    ExpectedProofs: new[] 
                    { 
                        new V { ["X"] = "one", ["Y"] = "one" },
                        new V { ["X"] = "one", ["Y"] = "two" },
                        new V { ["X"] = "one", ["Y"] = "three" },

                        new V { ["X"] = "zero", ["Y"] = "zero" }
                    }
                ),

                (
                    Description: "Classical Max",
                    Program: @"
                        max(X, Y, X) :- X>=Y,!.
                        max(X, Y, Y) :- X<Y.
                        number(1).
                        number(2).
                        number(3).",
                    Query: "number(X), number(Y), max(X, Y, X)",
                    ExpectedProofs: new[] 
                    {
                        new V { ["X"] = "1", ["Y"] = "1" },

                        new V { ["X"] = "2", ["Y"] = "1" },
                        new V { ["X"] = "2", ["Y"] = "2" },

                        new V { ["X"] = "3", ["Y"] = "1" },
                        new V { ["X"] = "3", ["Y"] = "2" },
                        new V { ["X"] = "3", ["Y"] = "3" }
                    }
                ),

                (
                    Description: "1st query from the 1st exercise from http://lpn.swi-prolog.org/lpnpage.php?pagetype=html&pageid=lpn-htmlse46",
                    Program: @"
                        p(one).
                        p(two) :- !.
                        p(three).",
                    Query: "p(X)",
                    ExpectedProofs: new[] 
                    {
                        new V { ["X"] = "one" },
                        new V { ["X"] = "two" }
                    }
                ),

                (
                    Description: "2nd query from the 1st exercise from http://lpn.swi-prolog.org/lpnpage.php?pagetype=html&pageid=lpn-htmlse46",
                    Program: @"
                        p(one).
                        p(two) :- !.
                        p(three).",
                    Query: "p(X), p(Y)",
                    ExpectedProofs: new[] 
                    {
                        new V { ["X"] = "one", ["Y"] = "one" },
                        new V { ["X"] = "one", ["Y"] = "two" },
                        new V { ["X"] = "two", ["Y"] = "one" },
                        new V { ["X"] = "two", ["Y"] = "two" }
                    }
                ),

                (
                    Description: "3rd query from the 1st exercise from http://lpn.swi-prolog.org/lpnpage.php?pagetype=html&pageid=lpn-htmlse46",
                    Program: @"
                        p(one).
                        p(two) :- !.
                        p(three).",
                    Query: "p(X), !, p(Y)",
                    ExpectedProofs: new[] 
                    {
                        new V { ["X"] = "one", ["Y"] = "one" },
                        new V { ["X"] = "one", ["Y"] = "two" }
                    }
                ),

                (
                    Description: "Fixed bug test",
                    Program: @"
                        p(two).
                        p(three).
                        q(one).
                        q(two).
                        q(three).
                        g(X) :- q(X), p(X), !.
                        s(X) :- g(X).",
                    Query: "s(X)",
                    ExpectedProofs: new[] 
                    {
                        new V { ["X"] = "two" }
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
                    Program: @"
                        enjoys(vincent,X) :- burger(X), not(big_kahuna_burger(X)).

                        burger(X) :- big_mac(X).
                        burger(X) :- big_kahuna_burger(X).
                        burger(X) :- whopper(X).

                        big_mac(a).
                        big_kahuna_burger(b).
                        big_mac(c).
                        whopper(d).",
                    Query: "enjoys(vincent,X)",
                    ExpectedProofs: new[] 
                    {
                        new V { ["X"] = "a" },
                        new V { ["X"] = "c" },
                        new V { ["X"] = "d" }
                    }
                )
            });
        }

        [TestMethod]
        public void TestingFindAll()
        {
            var edgeFacts = @"
                    edge(a, one).
                    edge(a, two).
                    edge(a, three).
                    edge(b, ten).
                    edge(b, twenty).
                    edge(b, thirty).";

            CheckSituations(new[]
            {
                (
                    Description: "findall() returns an empty list when there're no solutions to the goal",
                    Program: edgeFacts,
                    Query: "findall(X, edge(c, X), A)",
                    ExpectedSolutions: new[] 
                    {
                        new V { ["A"] = "[]" }
                    }
                ),

                (
                    Description: "findall() with single plain related variable product",
                    Program: edgeFacts,
                    Query: "findall(X, edge(b, X), A)",
                    ExpectedSolutions: new[] 
                    {
                        new V { ["A"] = "[ten, twenty, thirty]" }
                    }
                ),

                (
                    Description: "findall() with complex term of single related variable product",
                    Program: edgeFacts,
                    Query: "findall(state(X), edge(a, X), A)",
                    ExpectedSolutions: new[] 
                    {
                        new V { ["A"] = "[state(one), state(two), state(three)]" }
                    }
                ),

                (
                    Description: "findall() with single unrelated plain variable product",
                    Program: edgeFacts,
                    Query: "findall(state(Y), edge(a, X), A)",
                    ExpectedSolutions: new[] 
                    {
                        new V { ["A"] = "[state(_), state(_), state(_)]" }
                    }
                ),

                (
                    Description: "findall() with complex term of two variables product",
                    Program: edgeFacts,
                    Query: "findall(state(Y, X), edge(X, Y), A)",
                    ExpectedSolutions: new[] 
                    {
                        new V { ["A"] = "[state(one, a), state(two, a), state(three, a), state(ten, b), state(twenty, b), state(thirty, b)]" }
                    }
                )
            });
        }

        [ClassInitialize] public static void TestClassInitialize(TestContext? testContext) => SetupLogging(testContext);
    }
}