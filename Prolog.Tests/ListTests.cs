using Microsoft.VisualStudio.TestTools.UnitTesting;
using V = System.Collections.Generic.Dictionary<string, string>;

namespace Prolog.Tests
{
    [TestClass]
    public class ListTests : ProofTestsBase
    {
        [TestMethod]
        public void InfixOperatorsInLists()
        {
            CheckSituations(new[]
            {
                (
                    Description: "Testing parsing lists with members in the form of infix operators",
                    Program: "situation([located(man, on, X), located(woman, on, X), surface(cube, X), X = external]).",
                    Query: "situation(X)",
                    ExpectedSolutions: new[] 
                    {
                        new V { ["X"] = "[located(man, on, _), located(woman, on, _), surface(cube, _), _ = external]" }
                    }
                )
            });
        }

        [TestMethod]
        public void MemberApi()
        {
            CheckSituations(new[]
            {
                (
                    Description: "Testing built-in member() functor",
                    Program: string.Empty,
                    Query: "member(X, [dudweiler,fahlquemont,forbach,freyming,metz,nancy,saarbruecken,stAvold])",
                    ExpectedSolutions: new[] 
                    {
                        new V { ["X"] = "dudweiler" },
                        new V { ["X"] = "fahlquemont" },
                        new V { ["X"] = "forbach" },
                        new V { ["X"] = "freyming" },
                        new V { ["X"] = "metz" },
                        new V { ["X"] = "nancy" },
                        new V { ["X"] = "saarbruecken" },
                        new V { ["X"] = "stAvold" }                    
                    }
                ),

                (
                    Description: "Testing built-in member() functor when nontrivial unification required",
                    Program: string.Empty,
                    Query: "member(pair(edge(X, [one, two, three]), Действие), [a, pair(edge( [a, b, c], [one, two, three]), перевозит(волк, правый, левый)), b])",
                    ExpectedSolutions: new[] 
                    {
                        new V 
                        {  
                            ["X"] = "[a, b, c]",
                            ["Действие"] = "перевозит(волк, правый, левый)"
                        },
                    }
                ),

                (
                    Description: "Testing not(member(X, Y)) junction, checking the presence of an atom",
                    Program: string.Empty,
                    Query: "not(member(fahlquemont, [dudweiler,metz,nancy,saarbruecken,stAvold]))",
                    ExpectedSolutions: new[] 
                    {
                        new V()
                    }
                ),

                (
                    Description: "Testing not(member(X, Y)) junction, listing all possible proofs",
                    Program: string.Empty,
                    Query: @"
                        member(X, [dudweiler,fahlquemont,forbach,freyming,metz,nancy,saarbruecken,stAvold]),
                        not(member(X, [dudweiler,metz,nancy,saarbruecken,stAvold]))",
                    ExpectedSolutions: new[] 
                    {
                        new V { ["X"] = "fahlquemont" },
                        new V { ["X"] = "forbach" },
                        new V { ["X"] = "freyming" }
                    }
                )
            });
        }

        [TestMethod]
        public void SubsetApi()
        {
            CheckSituations(new[]
            {
                (
                    Description: "Testing built-in subset() functor, lists contain only atoms",
                    Program: string.Empty,
                    Query: "subset([fahlquemont,forbach,dudweiler], [dudweiler,fahlquemont,forbach,freyming,metz,nancy,saarbruecken,stAvold])",
                    ExpectedSolutions: new[] { new V() }
                ),

                (
                    Description: "Testing built-in subset() functor, lists contain complex terms with variables: result should contain correct instantiations of those variables",
                    Program: string.Empty,
                    Query: "subset([edge(one, X), edge(two, Y)], [edge(two, three), edge(one, thirty)])",
                    ExpectedSolutions: new[] 
                    { 
                        new V { ["X"] = "thirty", ["Y"] = "three" }
                    }
                ),
            });
        }

        [TestMethod]
        public void SubstractApi()
        {
            CheckSituations(new[]
            {
                (
                    Description: "Testing built-in substract() functor, lists contain only atoms",
                    Program: string.Empty,
                    Query: "subtract([dudweiler,fahlquemont,forbach,freyming,metz,nancy,saarbruecken,stAvold], [dudweiler,fahlquemont,forbach], Z)",
                    ExpectedSolutions: new[] 
                    { 
                        new V { ["Z"] = "[freyming,metz,nancy,saarbruecken,stAvold]" }
                    }
                ),

                (
                    Description: "Testing built-in substract() functor, lists contain complex terms with variables: result should contain correct instantiations of those variables",
                    Program: string.Empty,
                    Query: "subtract([edge(2, 3), edge(1, 30)], [edge(1, X), edge(2, Y)], Z)",
                    ExpectedSolutions: new[] 
                    { 
                        new V { ["X"] = "30", ["Y"] = "3", ["Z"] = "[]" }
                    }
                )
            });
        }

        [TestMethod]
        public void AppendApi()
        {
            CheckSituations(new[]
            {
                (
                    Description: "Testing built-in union() functor, first list is empty",
                    Program: string.Empty,
                    Query: "append([], [dudweiler,fahlquemont,forbach,freyming,metz,nancy,saarbruecken,stAvold], Z)",
                    ExpectedSolutions: new[] 
                    { 
                        new V { ["Z"] = "[dudweiler,fahlquemont,forbach,freyming,metz,nancy,saarbruecken,stAvold]" }
                    }
                ),

                (
                    Description: "Testing built-in union() functor, second list is empty",
                    Program: string.Empty,
                    Query: "append([dudweiler,fahlquemont,forbach,freyming,metz,nancy,saarbruecken,stAvold], [], Z)",
                    ExpectedSolutions: new[] 
                    { 
                        new V { ["Z"] = "[dudweiler,fahlquemont,forbach,freyming,metz,nancy,saarbruecken,stAvold]" }
                    }
                ),

                (
                    Description: "Testing built-in union() functor, both lists are non-empty",
                    Program: string.Empty,
                    Query: "append([metz,nancy,saarbruecken,stAvold], [dudweiler,fahlquemont,forbach,freyming], Z)",
                    ExpectedSolutions: new[] 
                    { 
                        new V { ["Z"] = "[metz,nancy,saarbruecken,stAvold,dudweiler,fahlquemont,forbach,freyming]" }
                    }
                )
            });
        }

        [TestMethod]
        public void SortApi()
        {
            CheckSituations(new[]
            {
                (
                    Description: "Testing built-in sort() functor",
                    Program: string.Empty,
                    Query: "sort([metz,fahlquemont,nancy,freyming,stAvold,dudweiler,saarbruecken,forbach], Z)",
                    ExpectedSolutions: new[] 
                    { 
                        new V { ["Z"] = "[dudweiler, fahlquemont, forbach, freyming, metz, nancy, saarbruecken, stAvold]" }
                    }
                )                
            });
        }

        [TestMethod]
        public void FlattenApi()
        {
            CheckSituations(new[]
            {
                (
                    Description: "Testing built-in flatten() functor: empty list",
                    Program: string.Empty,
                    Query: "flatten([], Z)",
                    ExpectedSolutions: new[] 
                    { 
                        new V { ["Z"] = "[]" }
                    }
                ),

                (
                    Description: "Testing built-in flatten() functor: list with a single non-list element",
                    Program: string.Empty,
                    Query: "flatten([atom], Z)",
                    ExpectedSolutions: new[] 
                    { 
                        new V { ["Z"] = "[atom]" }
                    }
                ),

                (
                    Description: "Testing built-in flatten() functor: list with a single list element",
                    Program: string.Empty,
                    Query: "flatten([[atom]], Z)",
                    ExpectedSolutions: new[] 
                    { 
                        new V { ["Z"] = "[atom]" }
                    }
                ),

                (
                    Description: "Testing built-in flatten() functor: list with several non-list element",
                    Program: string.Empty,
                    Query: "flatten([atom, edge(one, a), thirty, волк], Z)",
                    ExpectedSolutions: new[] 
                    { 
                        new V { ["Z"] = "[atom, edge(one, a), thirty, волк]" }
                    }
                ),

                (
                    Description: "Testing built-in flatten() functor: complex case",
                    Program: string.Empty,
                    Query: "flatten([atom, [edge(one, a), thirty], [[волк, left], state(b, c)]], Z)",
                    ExpectedSolutions: new[] 
                    { 
                        new V { ["Z"] = "[atom, edge(one, a), thirty, волк, left, state(b, c)]" }
                    }
                ),
            });
        }

        [ClassInitialize] public static void TestClassInitialize(TestContext? testContext) => SetupLogging(testContext);
    }
}