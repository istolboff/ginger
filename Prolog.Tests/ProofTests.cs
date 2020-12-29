using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prolog.Engine;
using static Prolog.Engine.Builtin;
using static Prolog.Engine.DomainApi;
using static Prolog.Tests.StockTerms;

using V = System.Collections.Generic.Dictionary<Prolog.Engine.Variable, Prolog.Engine.Term>;

namespace Prolog.Tests
{
    [TestClass]
    public class ProofTests : ProofTestsBase
    {
        [TestMethod]
        public void ProofBasedOnFactsOnly()
        {
            CheckSituations(new[]
            {
                (
                    Description: "Adhoc test",
                    Program: System.Array.Empty<Rule>(),
                    Query: new[] { Equal(FinalStateName, Atom("миссия заканчивается успехом")) },
                    ExpectedProofs: new[] 
                    { 
                        new V { [FinalStateName] = Atom("миссия заканчивается успехом") }
                    }
                )
            });

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
        public void SolvingVolfGoatCabbageRiddleUsingDepthFirstApproach()
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
 
        [TestMethod]
        public void SolvingVolfGoatCabbageRiddleUsingBreadthFirstApproach()
        {
            // while (!System.Diagnostics.Debugger.IsAttached) { System.Threading.Thread.Sleep(System.TimeSpan.FromSeconds(1)); }

            var program = new[] 
                    {
                        // % Сущности
                        // % ========

                        // % река имеет левый и правый берега
                            Fact(берег(река, левый)),
                            Fact(берег(река, правый)),

                        // % перевозимые существа это волк, коза и капуста 
                            Fact(перевозимоеCущество(волк)),
                            Fact(перевозимоеCущество(коза)),
                            Fact(перевозимоеCущество(капуста)),


                        // % Воздействия
                        // % ===========

                        // % Действие 'фермер перевозит перевозимое существо на другой берег' переводит из состояния 
                        // % 'фермер и перевозимое существо находятся на одном берегу' в состояние 
                        // % 'фермер и перевозимое существо находятся на другом берегу'.

                        Rule(действие(
                                перевозит(фермер, X, ОдинБерег, ДругойБерег), 
                                фрагментСостояния(List(находится(фермер, ОдинБерег), находится(X, ОдинБерег))),
                                фрагментСостояния(List(находится(фермер, ДругойБерег), находится(X, ДругойБерег)))),
                            перевозимоеCущество(X),
                            берег(река, ОдинБерег),
                            берег(река, ДругойБерег),
                            NotEqual(ОдинБерег, ДругойБерег)),


                        // % Действие 'фермер переправляется на другой берег' переводит из состояния 
                        // % 'фермер находится на одном берегу' в состояние 
                        // % 'фермер находится на другом берегу'.

                        Rule(действие(
                                переправляется(фермер, ОдинБерег, ДругойБерег),
                                фрагментСостояния(List(находится(фермер, ОдинБерег))),
                                фрагментСостояния(List(находится(фермер, ДругойБерег)))),
                            берег(река, ОдинБерег),
                            берег(река, ДругойБерег),
                            NotEqual(ОдинБерег, ДругойБерег)),


                        // % Граничные условия
                        // % =================
                        
                        // % в момент начала миссии фермер, волк, коза и капуста находятся на левом берегу реки

                        Fact(начальноеСостояние(
                            List(находится(волк, левый),
                                находится(коза, левый),
                                находится(капуста, левый),
                                находится(фермер, левый)))),


                        // % Правила
                        // % =======

                        // % если волк и коза находятся на одном берегу реки, а фермер находится на другом берегу реки,
                        // % то миссия заканчивается неудачей с формулировкой 'волк съел козу'

                        Rule(конечноеСостояние(
                            Atom("волк съел козу"),                          
                            List(
                                находится(волк, ОдинБерег),
                                находится(коза, ОдинБерег),
                                находится(фермер, ДругойБерег))),
                            берег(река, ОдинБерег),
                            берег(река, ДругойБерег),
                            NotEqual(ОдинБерег, ДругойБерег)),

                        // % если коза и капуста находятся на одном берегу реки, а фермер находится на другом берегу реки,
                        // % то миссия заканчивается неудачей с формулировкой 'коза съела капусту'

                        Rule(конечноеСостояние(
                            Atom("коза съела капусту"),
                            List(
                                находится(коза, ОдинБерег),
                                находится(капуста, ОдинБерег),
                                находится(фермер, ДругойБерег))),
                            берег(река, ОдинБерег),
                            берег(река, ДругойБерег),
                            NotEqual(ОдинБерег, ДругойБерег)),

                        // % если волк, коза и капуста находятся на правом берегу реки, то миссия заканчивается успехом

                        Fact(конечноеСостояние(
                            Atom("миссия заканчивается успехом"),
                            List(
                                находится(волк, правый),
                                находится(коза, правый),
                                находится(капуста, правый),
                                находится(фермер, правый)))),


                        // % Стандартные предикаты для поиска путей в пространстве состояний.

                        Rule(выполнимо(ТекущееПолноеСостояние, Действие, ФрагментСостоянияДо, ФрагментСостоянияПосле, ПолноеСостояниеПосле),
                            действие(Действие, фрагментСостояния(ФрагментСостоянияДо), фрагментСостояния(ФрагментСостоянияПосле)),
                            Subset(ФрагментСостоянияДо, ТекущееПолноеСостояние),
                            Subtract(ТекущееПолноеСостояние, ФрагментСостоянияДо, Y),
                            Append(Y, ФрагментСостоянияПосле, Z),
                            Sort(Z, ПолноеСостояниеПосле)),

                        Rule(unwantedFinalState(ExpectedFinalStateName, CurrentState),
                            конечноеСостояние(X, ConcreteFinalState),
                            NotEqual(X, ExpectedFinalStateName),
                            Subset(ConcreteFinalState, CurrentState)),

                        Rule(solve(InitialState, FinalStateName, FinalState, Solution),
                            Sort(InitialState, SortedInitialState),
                            Sort(FinalState, SortedFinalState),
                            breadthFirst(List(SortedInitialState), EmptyList, SortedFinalState, FinalStateName, Result), 
                            Flatten(Result, FlattenedResult),
                            listFullFinalStates(FinalState, FlattenedResult, MatchingFullFinalStates),
                            buildPath(SortedInitialState, MatchingFullFinalStates, FlattenedResult, ReversedPath),
                            Reverse(ReversedPath, Solution)),

                        Rule(breadthFirst(EmptyList, _, _, _, _), 
                            Fail),

                        Rule(breadthFirst(Dot(Finish, _), _, FinishSubset, _, EmptyList),
                            Subset(FinishSubset, Finish)),

                        Rule(breadthFirst(Dot(H, QueueAdded), QueueProcessed, Finish, FinalStateName, List(ConnectingEdges, Res)), 
                            adjacentNodes(H, FinalStateName, Adjacents),
                            dictionaryRemoveKeys(Adjacents, QueueAdded, Temp1),
                            dictionaryRemoveKeys(Temp1, QueueProcessed, Temp2),
                            buildConnectingEdges(H, Temp2, ConnectingEdges),
                            dictionaryKeys(Temp2, StatesReachableIn1Step),
                            Append(QueueAdded, StatesReachableIn1Step, ExtendedQueueAdded),
                            breadthFirst(ExtendedQueueAdded, Dot(H, QueueProcessed), Finish, FinalStateName, Res)),

                        Rule(adjacentNodes(H, FinalStateName, Adjacents),
                            FindAll(pair(NewState, Action), canMoveToNewState(H, FinalStateName, NewState, Action), Adjacents)),

                        Rule(canMoveToNewState(State, FinalStateName, State1, Действие),
                            выполнимо(State, Действие, _, _, State1),
                            Not(unwantedFinalState(FinalStateName, State1))),

                        Fact(buildConnectingEdges(_, EmptyList, EmptyList)),
                        Rule(buildConnectingEdges(From, Dot(pair(To, Action), Tail), Dot(pair(edge(From, To), Action), Result)),
                            buildConnectingEdges(From, Tail, Result)),

                        Rule(buildPath(Start, FullFinalStates, Edges, List(pair(edge(Start, Finish), Action))),
                            Member(Finish, FullFinalStates),
                            Member(pair(edge(Start, Finish), Action), Edges)),
                        Rule(buildPath(Start, FullFinalStates, Edges, Dot(pair(edge(X, Finish), Action), Path)),
                            Member(Finish, FullFinalStates),
                            Member(pair(edge(X, Finish), Action), Edges),
                            buildPath1(Start, X, Edges, Path)),

                        Rule(buildPath1(Start, Finish, Edges, List(pair(edge(Start, Finish), Action))),
                            Member(pair(edge(Start, Finish), Action), Edges)),
                        Rule(buildPath1(Start, Finish, Edges, Dot(pair(edge(X, Finish), Action), Path)),
                            Member(pair(edge(X, Finish), Action), Edges),
                            buildPath1(Start, X, Edges, Path)),

                        Rule(listFullFinalStates(FinalState, Dot(pair(edge(_, Finish), _), Edges), Dot(Finish, Result)),
                            Subset(FinalState, Finish),
                            listFullFinalStates(FinalState, Edges, Result),
                            Cut),
                        Rule(listFullFinalStates(FinalState, Dot(_, Edges), Result),
                            listFullFinalStates(FinalState, Edges, Result)),
                        Fact(listFullFinalStates(_, EmptyList, EmptyList)),


                        Rule(dictionaryKeys(Dot(pair(Key, _), Dictionary), Dot(Key, Keys)),
                            dictionaryKeys(Dictionary, Keys)),
                        Fact(dictionaryKeys(EmptyList, EmptyList)),

                        Rule(dictionaryValues(Dot(pair(_, Value), Dictionary), Dot(Value, Values)),
                            dictionaryValues(Dictionary, Values)),
                        Fact(dictionaryValues(EmptyList, EmptyList)),

                        Rule(dictionaryRemoveKeys(Dot(pair(Key, _), Dictionary), Keys, Dictionary1),
                            Member(Key, Keys), 
                            dictionaryRemoveKeys(Dictionary, Keys, Dictionary1),
                            Cut),
                        Rule(dictionaryRemoveKeys(Dot(X, Dictionary), Keys, Dot(X, Dictionary1)),
                            dictionaryRemoveKeys(Dictionary, Keys, Dictionary1)),
                        Fact(dictionaryRemoveKeys(EmptyList, _, EmptyList))

                        // % начальноеСостояние(InitialState)
                        // % , FinalStateName = 'волк съел козу'
                        // % , конечноеСостояние(FinalStateName, FinalState)
                        // % , solve(InitialState, FinalStateName, FinalState, Solution)
                        // % , dictionaryValues(Solution, Route)

                        // % начальноеСостояние(InitialState)
                        // % , FinalStateName = 'коза съела капусту'
                        // % , конечноеСостояние(FinalStateName, FinalState)
                        // % , solve(InitialState, FinalStateName, FinalState, Solution)
                        // % , dictionaryValues(Solution, Route)                        
                    };

            CheckSituations(new[] 
                {
                    (
                        Description: "Finding a solution for the Volf-Goat-Cabbage crossing the river riddle using breadth-first approach",
                        Program: program,
                        Query: new[] 
                        { 
                            начальноеСостояние(InitialState), 
                            Equal(FinalStateName, Atom("миссия заканчивается успехом")), 
                            конечноеСостояние(FinalStateName, FinalState), 
                            solve(InitialState, FinalStateName, FinalState, Solution), 
                            dictionaryValues(Solution, Route) 
                        },
                        ExpectedSolutions: new[] 
                        {
                            new V 
                            { 
                                [Route] = List(
                                    перевозит(фермер, коза, левый, правый), 
                                    переправляется(фермер, правый, левый), 
                                    перевозит(фермер, волк, левый, правый), 
                                    перевозит(фермер, коза, правый, левый), 
                                    перевозит(фермер, капуста, левый, правый), 
                                    переправляется(фермер, правый, левый), 
                                    перевозит(фермер, коза, левый, правый)) 
                            }
                        }
                    ),
                },
                ignoreUnlistedActualInstantiations: true);
        }

        [ClassInitialize] public static void TestClassInitialize(TestContext? testContext) => SetupLogging(testContext);
    }
}