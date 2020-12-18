using System;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prolog.Engine;
using static Prolog.Engine.Proof;
using static Prolog.Engine.Builtin;
using static Prolog.Tests.PrologApi;
using static Prolog.Tests.StockTerms;
using static Prolog.Tests.VerboseReporting;

using V = Prolog.Engine.StructuralEquatableDictionary<Prolog.Engine.Variable, Prolog.Engine.Term>;
using System.Globalization;
using System.Collections;

namespace Prolog.Tests
{
    [TestClass]
    public class ProofTests
    {
        [TestMethod]
        public void ProofBasedOnFactsOnly()
        {
            var situations = new[]
            {
                new
                {
                    Description = "Single unification without variable instantiations",
                    Facts = new[] { f(a) },
                    Query = new[] { f(a) },
                    ExpectedProofs = new[] { Success }
                },

                new
                {
                    Description = "Single unification with single variable instantiation",
                    Facts = new[] { f(a) },
                    Query = new[] { f(X) },
                    ExpectedProofs = new[] { X.InstantiatedTo(a) }
                },

                new
                {
                    Description = "Several facts unifications with single variable instantiation",
                    Facts = new[] { f(a), f(b) },
                    Query = new[] { f(X) },
                    ExpectedProofs = new[] { X.InstantiatedTo(a), X.InstantiatedTo(b) }
                },

                new 
                {
                    Description = "Multiple queries proof",
                    Facts = new[] { f(a), f(b), g(a), g(b), h(b) },
                    Query = new[] { f(X), g(X), h(X) },
                    ExpectedProofs = new[] { X.InstantiatedTo(b) }
                },

                new
                {
                    Description = "Non-trivial calculations based on unification only",
                    Facts = new[] { vertical(line(point(X, Y),point(X, Z))) },
                    Query = new[] { vertical(line(point(two, three), P)) },
                    ExpectedProofs = new[] { P.InstantiatedTo(point(two, _)) }
                },

                new
                {
                    Description = "Non-trivial calculations based on unification only (checking the case of clashing variable names)",
                    Facts = new[] { horizontal(line(point(_, X),point(_, X))) },
                    Query = new[] { horizontal(line(point(two, three), X)) },
                    ExpectedProofs = new[] { X.InstantiatedTo(point(_, three)) }
                }
            };

            var erroneousProofs = 
                (from situation in situations
                let actualProofs = Proof.Find(situation.Facts.Select(f => Fact(f)).ToArray(), situation.Query).ToArray()
                where !situation.ExpectedProofs.SequenceEqual(actualProofs)
                select new 
                { 
                    Description = situation.Description,
                    Facts = Dumpable(situation.Facts), 
                    Query = Dumpable(situation.Query), 
                    ExpectedProofs = Dumpable(situation.ExpectedProofs), 
                    ActualProofs = Dumpable(actualProofs) 
                })
                .ToList();

            Assert.IsFalse(erroneousProofs.Any(), Environment.NewLine + string.Join(Environment.NewLine, erroneousProofs));
        }

        [TestMethod]
        public void ProofBasedOnRulesAndFacts()
        {
            var situations = new[]
            {
                new
                {
                    Description = "Simple recursion",
                    Program = new[] 
                        { 
                            Fact(edge(a, b)), 
                            Fact(edge(b, c)), 
                            Fact(edge(c, d)), 
                            Rule(path(X, Y), edge(X, Y)), 
                            Rule(path(X, Z), edge(X, Y), path(Y, Z)) 
                        },
                    Query = path(a,X),
                    ExpectedProofs = new[] { X.InstantiatedTo(b), X.InstantiatedTo(c), X.InstantiatedTo(d) }
                }
            };

            var erroneousProofs = 
                (from situation in situations
                let actualProofs = Proof.Find(situation.Program, situation.Query).ToArray()
                where !situation.ExpectedProofs.SequenceEqual(actualProofs)
                select new 
                { 
                    Prorgam = Dumpable(situation.Program), 
                    Query = situation.Query, 
                    ExpectedProofs = Dumpable(situation.ExpectedProofs), 
                    ActualProofs = Dumpable(actualProofs) 
                })
                .ToList();

            Assert.IsFalse(erroneousProofs.Any(), Environment.NewLine + string.Join(Environment.NewLine, erroneousProofs));
        }

        [TestMethod]
        public void RulesWithCut()
        {
            var situations = new[]
            {
                new
                {
                    Description = "Program without Cut",
                    Program = new[] 
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
                    Query = new[] { s(X, Y) },
                    ExpectedSolutions = new[] 
                    { 
                        new V { [X] = one, [Y] = one },
                        new V { [X] = one, [Y] = two },
                        new V { [X] = one, [Y] = three },

                        new V { [X] = two, [Y] = one },
                        new V { [X] = two, [Y] = two },
                        new V { [X] = two, [Y] = three },

                        new V { [X] = zero, [Y] = zero }
                    }
                },

                new
                {
                    Description = "The same program as before, but this time with Cut",
                    Program = new[] 
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
                    Query = new[] { s(X, Y) },
                    ExpectedSolutions = new[] 
                    { 
                        new V { [X] = one, [Y] = one },
                        new V { [X] = one, [Y] = two },
                        new V { [X] = one, [Y] = three },

                        new V { [X] = zero, [Y] = zero }
                    }
                },

                new
                {
                    Description = "Classical Max",
                    Program = new[]
                    {
                        Rule(max(X, Y, X), GreaterThanOrEqual(X, Y), Cut),
                        Rule(max(X, Y, Y), LessThan(X, Y)),
                        Fact(number(one)),
                        Fact(number(two)),
                        Fact(number(three))
                    },
                    Query = new[] { number(X), number(Y), max(X, Y, X) },
                    ExpectedSolutions = new[] 
                    {
                        new V { [X] = one, [Y] = one },

                        new V { [X] = two, [Y] = one },
                        new V { [X] = two, [Y] = two },

                        new V { [X] = three, [Y] = one },
                        new V { [X] = three, [Y] = two },
                        new V { [X] = three, [Y] = three }
                    }
                }
            };

            var erroneousProofs = 
                (from situation in situations
                let expectedProofs = situation.ExpectedSolutions.Select(vi => new UnificationResult(true, vi)).ToArray()
                let actualProofs = Proof.Find(situation.Program, situation.Query).ToArray()
                where !expectedProofs.SequenceEqual(actualProofs)
                select new 
                { 
                    Prorgam = Dumpable(situation.Program), 
                    Query = situation.Query, 
                    ExpectedProofs = Dumpable(expectedProofs), 
                    ActualProofs = Dumpable(actualProofs) 
                })
                .ToList();

            Assert.IsFalse(erroneousProofs.Any(), Environment.NewLine + string.Join(Environment.NewLine, erroneousProofs));
        }
 
        [ClassInitialize]
        public static void SetupLogging(TestContext? testContext)
        {
            TraceFilePath = Path.Combine(testContext?.TestLogsDir ?? Path.GetTempPath(), "Prolog.trace");

            Proof.ProofEvent += (description, @this) =>
            {
                if (description != null)
                {
                    File.AppendAllText(TraceFilePath, $"{description}: ");
                }

                File.AppendAllText(TraceFilePath, Dump(@this));
                File.AppendAllLines(TraceFilePath, new[] { string.Empty });

                return;

                static string Dump<Q>(Q @this) =>
                    @this switch
                    {
                        Atom atom => atom.Characters,
                        Number number => number.Value.ToString(CultureInfo.InvariantCulture),
                        Variable variable => variable.Name,
                        ComplexTerm complexTerm => $"{complexTerm.Functor.Name}({string.Join(',', complexTerm.Arguments.Select(Dump))})",
                        Rule rule => $"{Dump(rule.Conclusion)}:-{string.Join(',', rule.Premises.Select(Dump))}",
                        UnificationResult unificationResult => string.Join(" & ",unificationResult.Instantiations.Select(i => $"{Dump(i.Key)} = {Dump(i.Value)}")),
                        string text => text,
                        IEnumerable collection => string.Join("; ", collection.Cast<object>().Select(Dump)),
                        _ => @this?.ToString() ?? "NULL"
                    };
            };
        }

        [TestInitialize]
        public void Setup()
        {
            File.Delete(TraceFilePath!);
        }

        private static string? TraceFilePath; 
   }
}