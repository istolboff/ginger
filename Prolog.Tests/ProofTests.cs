using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prolog.Engine;
using static Prolog.Tests.PrologApi;
using static Prolog.Tests.StockTerms;
using static Prolog.Tests.VerboseReporting;
using static Prolog.Engine.Proof;

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
                let actualProofs = Proof.Find(situation.Facts.Select(f => Fact(f)).ToArray(), situation.Query)
                where !situation.ExpectedProofs.SequenceEqual(actualProofs)
                select new 
                { 
                    Description = situation.Description,
                    Facts = Dumpable(situation.Facts), 
                    Query = Dumpable(situation.Query), 
                    ExpectedProofs = Dumpable(situation.ExpectedProofs), 
                    ActualProofs = Dumpable(actualProofs.ToArray()) 
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
                let actualProofs = Proof.Find(situation.Program, situation.Query)
                where !situation.ExpectedProofs.SequenceEqual(actualProofs)
                select new 
                { 
                    Prorgam = Dumpable(situation.Program), 
                    Query = situation.Query, 
                    ExpectedProofs = Dumpable(situation.ExpectedProofs), 
                    ActualProofs = Dumpable(actualProofs.ToList()) 
                })
                .ToList();

            Assert.IsFalse(erroneousProofs.Any(), Environment.NewLine + string.Join(Environment.NewLine, erroneousProofs));
        }
    }
}