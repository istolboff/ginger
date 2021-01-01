using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prolog.Engine;
using static Prolog.Engine.Builtin;
using static Prolog.Engine.DomainApi;
using static Prolog.Tests.StockTerms;
using static Prolog.Tests.VerboseReporting;

using V = System.Collections.Generic.Dictionary<Prolog.Engine.Variable, Prolog.Engine.Term>;

namespace Prolog.Tests
{
    [TestClass]
    public class ProofTests
    {
        [TestMethod]
        public void ProofBasedOnFactsOnly()
        {
            CheckSituations(new[]
            {
                (
                    Description: "Single unification without variable instantiations",
                    Program: new[] { Fact(f(a)) },
                    Query: new[] { f(a) },
                    ExpectedProofs: new[] { new V() }
                ),

                (
                    Description: "Single unification with single variable instantiation",
                    Program: new[] { Fact(f(a)) },
                    Query: new[] { f(X) },
                    ExpectedProofs: new[] { new V { [X] = a } }
                ),

                (
                    Description: "Several facts unifications with single variable instantiation",
                    Program: new[] { Fact(f(a)), Fact(f(b)) },
                    Query: new[] { f(X) },
                    ExpectedProofs: new[] 
                    { 
                        new V { [X] = a }, 
                        new V { [X] = b }
                    }
                ),

                (
                    Description: "Multiple queries proof",
                    Program: new[] { Fact(f(a)), Fact(f(b)), Fact(g(a)), Fact(g(b)), Fact(h(b)) },
                    Query: new[] { f(X), g(X), h(X) },
                    ExpectedProofs: new[] { new V { [X] = b } }
                ),

                (
                    Description: "Non-trivial calculations based on unification only",
                    Program: new[] { Fact(vertical(line(point(X, Y),point(X, Z)))) },
                    Query: new[] { vertical(line(point(two, three), P)) },
                    ExpectedProofs: new[] { new V { [P] = point(two, _) } }
                ),

                (
                    Description: "Non-trivial calculations based on unification only (checking the case of clashing variable names)",
                    Program: new[] { Fact(horizontal(line(point(_, X),point(_, X)))) },
                    Query: new[] { horizontal(line(point(two, three), X)) },
                    ExpectedProofs: new[] { new V { [X] = point(_, three) } }
                )
            });
        }

        [TestMethod]
        public void ProofBasedOnRulesAndFacts()
        {
            CheckSituations(new[]
            {
                (
                    Description: "Simple recursion",
                    Program: new[] 
                        { 
                            Fact(edge(a, b)), 
                            Fact(edge(b, c)), 
                            Fact(edge(c, d)), 
                            Rule(path(X, Y), edge(X, Y)), 
                            Rule(path(X, Z), edge(X, Y), path(Y, Z)) 
                        },
                    Query: new[] { path(a,X) },
                    ExpectedProofs: new[] 
                    { 
                        new V { [X] = b }, 
                        new V { [X] = c }, 
                        new V { [X] = d } 
                    }
                )
            });
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
        public void ListTests()
        {
            CheckSituations(new[]
            {
                (
                    Description: "Testing built-in member() functor",
                    Program: Array.Empty<Rule>(),
                    Query: new[] { Member(X, List(dudweiler,fahlquemont,forbach,freyming,metz,nancy,saarbruecken,stAvold)) },
                    ExpectedSolutions: new[] 
                    {
                        new V { [X] = dudweiler },
                        new V { [X] = fahlquemont },
                        new V { [X] = forbach },
                        new V { [X] = freyming },
                        new V { [X] = metz },
                        new V { [X] = nancy },
                        new V { [X] = saarbruecken },
                        new V { [X] = stAvold }                    
                    }
                ),

                (
                    Description: "Testing not(member(X, Y)) junction, checking the presence of an atom",
                    Program: Array.Empty<Rule>(),
                    Query: new[] 
                    { 
                        Not(Member(fahlquemont, List(dudweiler,metz,nancy,saarbruecken,stAvold)))
                    },
                    ExpectedSolutions: new[] 
                    {
                        new V()
                    }
                ),

                (
                    Description: "Testing not(member(X, Y)) junction, listing all possible proofs",
                    Program: Array.Empty<Rule>(),
                    Query: new[] 
                    { 
                        Member(X, List(dudweiler,fahlquemont,forbach,freyming,metz,nancy,saarbruecken,stAvold)),
                        Not(Member(X, List(dudweiler,metz,nancy,saarbruecken,stAvold)))
                    },
                    ExpectedSolutions: new[] 
                    {
                        new V { [X] = fahlquemont },
                        new V { [X] = forbach },
                        new V { [X] = freyming }
                    }
                )
            });
        }

        [TestMethod]
        public void RouteBuilding()
        {
            CheckSituations(new[]
            {
                (
                    Description: "Building a route in an acyclic graph",
                    Program: new[]
                    {
                        Fact(directTrain(saarbruecken, dudweiler)),
                        Fact(directTrain(forbach,      saarbruecken)),
                        Fact(directTrain(freyming,     forbach)),
                        Fact(directTrain(stAvold,      freyming)),
                        Fact(directTrain(fahlquemont,  stAvold)),
                        Fact(directTrain(metz,         fahlquemont)),
                        Fact(directTrain(nancy,        metz)),

                        Rule(connected(A, B), directTrain(A, B)),
                        Rule(connected(A, B), directTrain(B, A)),

                        Rule(route(A, B, List(A, B), _), connected(A, B), Cut),
                        Rule(route(A, B, Dot(A, R), Visited),
                            connected(A, C),
                            Not(Member(C, Visited)),
                            route(C, B, R, Dot(A, Visited))),

                        Rule(route(A, B, R), route(A, B, R, EmptyList))
                    },
                    Query: new[] { route(forbach, metz, Route) },
                    ExpectedSolutions: new[] 
                    {
                        new V { [Route] = List(forbach, freyming, stAvold, fahlquemont, metz) }
                    }
                )
            });
        }

        [TestMethod]
        public void SolvingVolfGoatCabbageRiddle()
        {
            var amocwm = atMostOneCreatureWasMoved(List(A, B, C), List(A1, B1, C1), NewFarmerPosition);
            var sao = sidesAreOk(FarmerPosition, WolfPosition, GoatPosition, CabbagePosition);
            CheckSituations(new[] 
            {
                (
                    Description: "Finding a solution for the Volf-Goat-Cabbage crossing the river riddle",
                    Program: new[] 
                    {
                        Fact(riverbank(right)),
                        Fact(riverbank(left)),

                        Rule(
                            canMove(
                                state(FarmerPosition, WolfPosition, GoatPosition, CabbagePosition),
                                state(FarmerPosition1, WolfPosition1, GoatPosition1, CabbagePosition1)),
                            riverbank(FarmerPosition1), 
                            riverbank(WolfPosition1), 
                            riverbank(GoatPosition1), 
                            riverbank(CabbagePosition1),
                            NotEqual(FarmerPosition1, FarmerPosition),
                            atMostOneCreatureWasMoved(List(WolfPosition, GoatPosition, CabbagePosition), List(WolfPosition1, GoatPosition1, CabbagePosition1), FarmerPosition1),
                            sidesAreOk(FarmerPosition1, WolfPosition1, GoatPosition1, CabbagePosition1)),


                        Rule(amocwm, NotEqual(A, A1), Equal(B, B1), Equal(C, C1), Equal(NewFarmerPosition, A1)),
                        Rule(amocwm, Equal(A, A1), NotEqual(B, B1), Equal(C, C1), Equal(NewFarmerPosition, B1)),
                        Rule(amocwm, Equal(A, A1), Equal(B, B1), NotEqual(C, C1), Equal(NewFarmerPosition, C1)),
                        Rule(amocwm, Equal(A, A1), Equal(B, B1), Equal(C, C1)),

                        Rule(sao, NotEqual(WolfPosition, GoatPosition), NotEqual(GoatPosition, CabbagePosition)),
                        Rule(sao, Equal(FarmerPosition, GoatPosition)),

                        Rule(solve(State, Solution), depthfirst(EmptyList, State, Solution)),
                            
                        Fact(depthfirst(_, state(right, right, right, right), List(state(right, right, right, right)))),

                        Rule(
                            depthfirst(Path, State, Dot(State, Solution1)),
                            canMove(State, State1),
                            Not(Member(State1, Path)),
                            depthfirst(Dot(State, Path), State1, Solution1))
                    },
                    Query: new[] { solve(state(left, left, left, left), Solution) },
                    ExpectedSolutions: new[] 
                    { 
                        new V
                        {
                            [Solution] = List(
                                state(left, left, left, left),
                                state(right, left, right, left),
                                state(left, left, right, left),
                                state(right, right, right, left),
                                state(left, right, left, left),
                                state(right, right, left, right),
                                state(left, right, left, right),
                                state(right, right, right, right))
                        }
                    }
                )
            },
            onlyFirstSolution: true);
        }

#if UseLogging
        [TestInitialize]
        public void Setup()
        {
            System.IO.File.Delete(_traceFilePath!);
        }

        [ClassInitialize]
        public static void SetupLogging(TestContext? testContext)
        {
            _traceFilePath = System.IO.Path.Combine(testContext?.TestLogsDir ?? System.IO.Path.GetTempPath(), "Prolog.trace");

            Proof.ProofEvent += (description, nestingLevel, @this) =>
            {
                System.IO.File.AppendAllText(_traceFilePath, new string(' ', nestingLevel * 3));

                if (description != null)
                {
                    System.IO.File.AppendAllText(_traceFilePath, $"{description}: ");
                }

                System.IO.File.AppendAllText(_traceFilePath, Dump(@this));
                System.IO.File.AppendAllLines(_traceFilePath, new[] { string.Empty });
            };
        }
#endif

        private static void CheckSituations(
            IEnumerable<(string Description, Rule[] Program, ComplexTerm[] Query, V[] ExpectedProofs)> situations,
            bool onlyFirstSolution = false)
        {
            var erroneousProofs = 
                (from situation in situations
                let expectedProofs = situation.ExpectedProofs.Select(Unification.Success).ToArray()
                let actualProofs = Proof.Find(situation.Program, situation.Query).Take(onlyFirstSolution ? 1 : int.MaxValue).ToArray()
                where !expectedProofs.SequenceEqual(actualProofs)
                select new 
                { 
                    situation.Description,
                    Prorgam = Dump(situation.Program, Environment.NewLine), 
                    Query = Dump(situation.Query),
                    ExpectedProofs = Dump(expectedProofs), 
                    ActualProofs = Dump(actualProofs) 
                })
                .ToList();

            Assert.IsFalse(erroneousProofs.Any(), Environment.NewLine + string.Join(Environment.NewLine, erroneousProofs));
        }

 #if UseLogging
       private static string? _traceFilePath; 
#endif
   }
}