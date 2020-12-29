using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prolog.Engine;

using static Prolog.Engine.Builtin;
using static Prolog.Engine.DomainApi;
using static Prolog.Tests.StockTerms;

using V = System.Collections.Generic.Dictionary<Prolog.Engine.Variable, Prolog.Engine.Term>;

namespace Prolog.Tests
{
    [TestClass]
    public class ListTests : ProofTestsBase
    {
        [TestMethod]
        public void MemberApi()
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
                    Description: "Testing built-in member() functor when nontrivial unification required",
                    Program: Array.Empty<Rule>(),
                    Query: new[] { Member(pair(edge(X, List(one, two, three)), Действие), List(a, pair(edge(List(a, b, c), List(one, two, three)), перевозит(волк, правый, левый)), b)) },
                    ExpectedSolutions: new[] 
                    {
                        new V 
                        {  
                            [X] = List(a, b, c),
                            [Действие] = перевозит(волк, правый, левый)
                        },
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
        public void SubsetApi()
        {
            CheckSituations(new[]
            {
                (
                    Description: "Testing built-in subset() functor, lists contain only atoms",
                    Program: Array.Empty<Rule>(),
                    Query: new[] { Subset(List(fahlquemont,forbach,dudweiler), List(dudweiler,fahlquemont,forbach,freyming,metz,nancy,saarbruecken,stAvold)) },
                    ExpectedSolutions: new[] { new V() }
                ),

                (
                    Description: "Testing built-in subset() functor, lists contain complex terms with variables: result should contain correct instantiations of those variables",
                    Program: Array.Empty<Rule>(),
                    Query: new[] { Subset(List(edge(one, X), edge(two, Y)), List(edge(two, three), edge(one, thirty))) },
                    ExpectedSolutions: new[] 
                    { 
                        new V { [X] = thirty, [Y] = three }
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
                    Program: Array.Empty<Rule>(),
                    Query: new[] { Subtract(List(dudweiler,fahlquemont,forbach,freyming,metz,nancy,saarbruecken,stAvold), List(dudweiler,fahlquemont,forbach), Z) },
                    ExpectedSolutions: new[] 
                    { 
                        new V { [Z] = List(freyming,metz,nancy,saarbruecken,stAvold) }
                    }
                ),

                (
                    Description: "Testing built-in substract() functor, lists contain complex terms with variables: result should contain correct instantiations of those variables",
                    Program: Array.Empty<Rule>(),
                    Query: new[] { Subtract(List(edge(two, three), edge(one, thirty)), List(edge(one, X), edge(two, Y)), Z) },
                    ExpectedSolutions: new[] 
                    { 
                        new V { [X] = thirty, [Y] = three, [Z] = EmptyList }
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
                    Program: Array.Empty<Rule>(),
                    Query: new[] { Append(EmptyList, List(dudweiler,fahlquemont,forbach,freyming,metz,nancy,saarbruecken,stAvold), Z) },
                    ExpectedSolutions: new[] 
                    { 
                        new V { [Z] = List(dudweiler,fahlquemont,forbach,freyming,metz,nancy,saarbruecken,stAvold) }
                    }
                ),

                (
                    Description: "Testing built-in union() functor, second list is empty",
                    Program: Array.Empty<Rule>(),
                    Query: new[] { Append(List(dudweiler,fahlquemont,forbach,freyming,metz,nancy,saarbruecken,stAvold), EmptyList, Z) },
                    ExpectedSolutions: new[] 
                    { 
                        new V { [Z] = List(dudweiler,fahlquemont,forbach,freyming,metz,nancy,saarbruecken,stAvold) }
                    }
                ),

                (
                    Description: "Testing built-in union() functor, both lists are non-empty",
                    Program: Array.Empty<Rule>(),
                    Query: new[] { Append(List(metz,nancy,saarbruecken,stAvold), List(dudweiler,fahlquemont,forbach,freyming), Z) },
                    ExpectedSolutions: new[] 
                    { 
                        new V { [Z] = List(metz,nancy,saarbruecken,stAvold,dudweiler,fahlquemont,forbach,freyming) }
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
                    Program: Array.Empty<Rule>(),
                    Query: new[] { Sort(List(metz,fahlquemont,nancy,freyming,stAvold,dudweiler,saarbruecken,forbach), Z) },
                    ExpectedSolutions: new[] 
                    { 
                        new V { [Z] = List(dudweiler, fahlquemont, forbach, freyming, metz, nancy, saarbruecken, stAvold) }
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
                    Program: Array.Empty<Rule>(),
                    Query: new[] { Flatten(EmptyList, Z) },
                    ExpectedSolutions: new[] 
                    { 
                        new V { [Z] = EmptyList }
                    }
                ),

                (
                    Description: "Testing built-in flatten() functor: list with a single non-list element",
                    Program: Array.Empty<Rule>(),
                    Query: new[] { Flatten(List(atom), Z) },
                    ExpectedSolutions: new[] 
                    { 
                        new V { [Z] = List(atom) }
                    }
                ),

                (
                    Description: "Testing built-in flatten() functor: list with a single list element",
                    Program: Array.Empty<Rule>(),
                    Query: new[] { Flatten(List(List(List(atom))), Z) },
                    ExpectedSolutions: new[] 
                    { 
                        new V { [Z] = List(atom) }
                    }
                ),

                (
                    Description: "Testing built-in flatten() functor: list with several non-list element",
                    Program: Array.Empty<Rule>(),
                    Query: new[] { Flatten(List(atom, edge(one, a), thirty, волк), Z) },
                    ExpectedSolutions: new[] 
                    { 
                        new V { [Z] = List(atom, edge(one, a), thirty, волк) }
                    }
                ),

                (
                    Description: "Testing built-in flatten() functor: complex case",
                    Program: Array.Empty<Rule>(),
                    Query: new[] { Flatten(List(atom, List(edge(one, a), thirty), List(List(волк, left), state(b, c))), Z) },
                    ExpectedSolutions: new[] 
                    { 
                        new V { [Z] = List(atom, edge(one, a), thirty, волк, left, state(b, c)) }
                    }
                ),
            });
        }
    }
}