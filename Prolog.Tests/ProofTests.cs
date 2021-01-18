using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using V = System.Collections.Generic.Dictionary<string, string>;

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
                    Program: string.Empty,
                    Query: "FinalStateName = 'миссия заканчивается успехом'",
                    ExpectedProofs: new[] 
                    { 
                        new V { ["FinalStateName"] = "'миссия заканчивается успехом'" }
                    }
                )
            });

            CheckSituations(new[]
            {
                (
                    Description: "Single unification without variable instantiations",
                    Program: "f(a).",
                    Query: "f(a)",
                    ExpectedProofs: new[] { new V() }
                ),

                (
                    Description: "Single unification with single variable instantiation",
                    Program: "f(a).",
                    Query: "f(X)",
                    ExpectedProofs: new[] { new V { ["X"] = "a" } }
                ),

                (
                    Description: "Several facts unifications with single variable instantiation",
                    Program: "f(a). f(b).",
                    Query: "f(X)",
                    ExpectedProofs: new[] 
                    { 
                        new V { ["X"] = "a" }, 
                        new V { ["X"] = "b" }
                    }
                ),

                (
                    Description: "Multiple queries proof",
                    Program: "f(a). f(b). g(a). g(b). h(b).",
                    Query: "f(X), g(X), h(X)",
                    ExpectedProofs: new[] { new V { ["X"] = "b" } }
                ),

                (
                    Description: "Non-trivial calculations based on unification only",
                    Program: "vertical(line(point(X, Y), point(X, Z))).",
                    Query: "vertical(line(point(two, three), P))",
                    ExpectedProofs: new[] { new V { ["P"] = "point(two, _)" } }
                ),

                (
                    Description: "Non-trivial calculations based on unification only (checking the case of clashing variable names)",
                    Program: "horizontal(line(point(_, X), point(_, X))).",
                    Query: "horizontal(line(point(two, three), X))",
                    ExpectedProofs: new[] { new V { ["X"] = "point(_, three)" } }
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
                    Program: @"
                            edge(a, b). 
                            edge(b, c). 
                            edge(c, d). 
                            path(X, Y) :- edge(X, Y).
                            path(X, Z) :- edge(X, Y), path(Y, Z).",
                    Query: "path(a,X)",
                    ExpectedProofs: new[] 
                    { 
                        new V { ["X"] = "b" }, 
                        new V { ["X"] = "c" }, 
                        new V { ["X"] = "d" } 
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
                    Program: @"
                        directTrain(saarbruecken, dudweiler).
                        directTrain(forbach,      saarbruecken).
                        directTrain(freyming,     forbach).
                        directTrain(stAvold,      freyming).
                        directTrain(fahlquemont,  stAvold).
                        directTrain(metz,         fahlquemont).
                        directTrain(nancy,        metz).

                        connected(A, B):-directTrain(A, B).
                        connected(A, B):-directTrain(B, A).

                        route(A, B, [A, B], _) :- connected(A, B), !.
                        route(A, B, [A | R], Visited) :-
                            connected(A, C),
                            not(member(C, Visited)),
                            route(C, B, R, [A | Visited]).

                        route(A, B, R):- route(A, B, R, []).",
                    Query: "route(forbach, metz, Route)",
                    ExpectedSolutions: new[] 
                    {
                        new V { ["Route"] = "[forbach, freyming, stAvold, fahlquemont, metz]" }
                    }
                )
            });
        }

        [TestMethod]
        public void SolvingVolfGoatCabbageRiddleUsingDepthFirstApproach()
        {
            CheckSituations(new[] 
            {
                (
                    Description: "Finding a solution for the Volf-Goat-Cabbage crossing the river riddle",
                    Program: @"
                        % state(FarmerPosition, WolfPosition, GoatPosition, CabbagePosition)
                        % Each position can be either right or left
                        
                        riverbank(right).
                        riverbank(left).

                        canMove(
                        state(FarmerPosition, WolfPosition, GoatPosition, CabbagePosition),
                        state(FarmerPosition1, WolfPosition1, GoatPosition1, CabbagePosition1)) :-
                            riverbank(FarmerPosition1), 
                            riverbank(WolfPosition1), 
                            riverbank(GoatPosition1), 
                            riverbank(CabbagePosition1),
                            FarmerPosition1 \= FarmerPosition,
                            atMostOneCreatureWasMoved([WolfPosition, GoatPosition, CabbagePosition], [WolfPosition1, GoatPosition1, CabbagePosition1], FarmerPosition1),
                            sidesAreOk(FarmerPosition1, WolfPosition1, GoatPosition1, CabbagePosition1).

                        atMostOneCreatureWasMoved([A, B, C], [A1, B1, C1], NewFarmerPosition) :- 
                            A \= A1, B = B1, C = C1, NewFarmerPosition = A1
                            ;   
                            A = A1, B \= B1, C = C1, NewFarmerPosition = B1
                            ;   
                            A = A1, B = B1, C \= C1, NewFarmerPosition = C1
                            ;
                            A = A1, B = B1, C = C1.

                        sidesAreOk(FarmerPosition, WolfPosition, GoatPosition, CabbagePosition) :-
                            WolfPosition \= GoatPosition, GoatPosition \= CabbagePosition
                            ;   
                            FarmerPosition = GoatPosition.

                        solve(State, Solution) :-
                            depthfirst([], State, Solution).
                            
                        depthfirst(_, state(right, right, right, right), [state(right, right, right, right)]).

                        depthfirst(Path, State, [State | Solution1]) :-
                            canMove(State, State1),
                            not(member(State1, Path)),
                            depthfirst([State | Path], State1, Solution1).",
                    Query: "solve(state(left, left, left, left), Solution)",
                    ExpectedSolutions: new[] 
                    { 
                        new V
                        {
                            ["Solution"] = @"[
                                state(left, left, left, left),
                                state(right, left, right, left),
                                state(left, left, right, left),
                                state(right, right, right, left),
                                state(left, right, left, left),
                                state(right, right, left, right),
                                state(left, right, left, right),
                                state(right, right, right, right)]"
                        }
                    }
                )
            },
            onlyFirstSolution: true);
        }
 
        [TestMethod]
        public void SolvingVolfGoatCabbageRiddleUsingBreadthFirstApproach()
        {
            var program = @"
                % Сущности
                % ========

                % река имеет левый и правый берега
                    берег(река, левый).
                    берег(река, правый).

                % перевозимые существа это волк, коза и капуста 
                    перевозимоеCущество(волк).
                    перевозимоеCущество(коза).
                    перевозимоеCущество(капуста).


                % Воздействия
                % ===========

                % Действие 'фермер перевозит перевозимое существо на другой берег' переводит из состояния 
                % 'фермер и перевозимое существо находятся на одном берегу' в состояние 
                % 'фермер и перевозимое существо находятся на другом берегу'.

                действие(
                        перевозит(фермер, Х, ОдинБерег, ДругойБерег), 
                        фрагментСостояния([находится(фермер, ОдинБерег), находится(Х, ОдинБерег)]),
                        фрагментСостояния([находится(фермер, ДругойБерег), находится(Х, ДругойБерег)])) :-
                    перевозимоеCущество(Х),
                    берег(река, ОдинБерег),
                    берег(река, ДругойБерег),
                    ОдинБерег \= ДругойБерег.


                % Действие 'фермер переправляется на другой берег' переводит из состояния 
                % 'фермер находится на одном берегу' в состояние 
                % 'фермер находится на другом берегу'.

                действие(
                        переправляется(фермер, ОдинБерег, ДругойБерег),
                        фрагментСостояния([находится(фермер, ОдинБерег)]),
                        фрагментСостояния([находится(фермер, ДругойБерег)])) :-
                    берег(река, ОдинБерег),
                    берег(река, ДругойБерег),
                    ОдинБерег \= ДругойБерег.


                % Граничные условия
                % =================
                
                % в момент начала миссии фермер, волк, коза и капуста находятся на левом берегу реки

                начальноеСостояние(
                        [находится(волк, левый),
                        находится(коза, левый),
                        находится(капуста, левый),
                        находится(фермер, левый)]).


                % Правила
                % =======

                % если волк и коза находятся на одном берегу реки, а фермер находится на другом берегу реки,
                % то миссия заканчивается неудачей с формулировкой 'волк съел козу'

                конечноеСостояние(
                    'волк съел козу',                          
                    [находится(волк, ОдинБерег),
                    находится(коза, ОдинБерег),
                    находится(фермер, ДругойБерег)]) :-
                    берег(река, ОдинБерег),
                    берег(река, ДругойБерег),
                    ОдинБерег \= ДругойБерег.    
                    


                % если коза и капуста находятся на одном берегу реки, а фермер находится на другом берегу реки,
                % то миссия заканчивается неудачей с формулировкой 'коза съела капусту'

                конечноеСостояние(
                    'коза съела капусту',
                    [находится(коза, ОдинБерег),
                    находится(капуста, ОдинБерег),
                    находится(фермер, ДругойБерег)]) :-
                    берег(река, ОдинБерег),
                    берег(река, ДругойБерег),
                    ОдинБерег \= ДругойБерег.

                % если волк, коза и капуста находятся на правом берегу реки, то миссия заканчивается успехом

                конечноеСостояние(
                    'миссия заканчивается успехом',
                    [находится(волк, правый),
                    находится(коза, правый),
                    находится(капуста, правый),
                    находится(фермер, правый)]).


                % Стандартные предикаты для поиска путей в пространстве состояний.

                выполнимо(ТекущееПолноеСостояние, Действие, ФрагментСостоянияДо, ФрагментСостоянияПосле, ПолноеСостояниеПосле) :-
                    действие(Действие, фрагментСостояния(ФрагментСостоянияДо), фрагментСостояния(ФрагментСостоянияПосле)),
                    subset(ФрагментСостоянияДо, ТекущееПолноеСостояние),
                    subtract(ТекущееПолноеСостояние, ФрагментСостоянияДо, Y),
                    append(Y, ФрагментСостоянияПосле, Z),
                    sort(Z, ПолноеСостояниеПосле).

                unwantedFinalState(ExpectedFinalStateName, CurrentState) :-
                    конечноеСостояние(X, ConcreteFinalState),
                    X \= ExpectedFinalStateName,
                    subset(ConcreteFinalState, CurrentState).

                solve(InitialState, FinalStateName, FinalState, Solution) :-
                    sort(InitialState, SortedInitialState),
                    sort(FinalState, SortedFinalState),
                    breadthFirst([SortedInitialState], [], SortedFinalState, FinalStateName, Result), 
                    flatten(Result, FlattenedResult),
                    listFullFinalStates(FinalState, FlattenedResult, MatchingFullFinalStates),
                    buildPath(SortedInitialState, MatchingFullFinalStates, FlattenedResult, ReversedPath),
                    reverse(ReversedPath, Solution).

                breadthFirst([], _, _, _, _) :- 
                    fail.

                breadthFirst([Finish | _], _, FinishSubset, _, []) :- 
                    subset(FinishSubset, Finish).

                breadthFirst([H | QueueAdded], QueueProcessed, Finish, FinalStateName, [ConnectingEdges, Res]) :- 
                    adjacentNodes(H, FinalStateName, Adjacents),
                    dictionaryRemoveKeys(Adjacents, QueueAdded, Temp1),
                    dictionaryRemoveKeys(Temp1, QueueProcessed, Temp2),
                    buildConnectingEdges(H, Temp2, ConnectingEdges),
                    dictionaryKeys(Temp2, StatesReachableIn1Step),
                    append(QueueAdded, StatesReachableIn1Step, ExtendedQueueAdded),
                    breadthFirst(ExtendedQueueAdded, [H | QueueProcessed], Finish, FinalStateName, Res).

                adjacentNodes(H, FinalStateName, Adjacents) :-
                    findall(pair(NewState, Action), canMoveToNewState(H, FinalStateName, NewState, Action), Adjacents).

                canMoveToNewState(State, FinalStateName, State1, Действие) :-
                    выполнимо(State, Действие, _, _, State1),
                    not(unwantedFinalState(FinalStateName, State1)).

                buildConnectingEdges(_, [], []).
                buildConnectingEdges(From, [pair(To, Action) | Tail], [pair(edge(From, To), Action) | Result]) :- 
                    buildConnectingEdges(From, Tail, Result).

                buildPath(Start, FullFinalStates, Edges, [pair(edge(Start, Finish), Action)]) :-
                    member(Finish, FullFinalStates),
                    member(pair(edge(Start, Finish), Action), Edges).
                buildPath(Start, FullFinalStates, Edges, [pair(edge(X, Finish), Action) | Path]) :-
                    member(Finish, FullFinalStates),
                    member(pair(edge(X, Finish), Action), Edges),
                    buildPath1(Start, X, Edges, Path).

                buildPath1(Start, Finish, Edges, [pair(edge(Start, Finish), Action)]) :-
                    member(pair(edge(Start, Finish), Action), Edges).
                buildPath1(Start, Finish, Edges, [pair(edge(X, Finish), Action) | Path]) :-
                    member(pair(edge(X, Finish), Action), Edges),
                    buildPath1(Start, X, Edges, Path).

                listFullFinalStates(FinalState, [pair(edge(_, Finish), _) | Edges], [Finish | Result]) :-
                    subset(FinalState, Finish),
                    listFullFinalStates(FinalState, Edges, Result),
                    !.
                listFullFinalStates(FinalState, [_ | Edges], Result) :-
                    listFullFinalStates(FinalState, Edges, Result).
                listFullFinalStates(_, [], []).


                dictionaryKeys([pair(Key, _) | Dictionary], [Key | Keys]) :- 
                    dictionaryKeys(Dictionary, Keys).
                dictionaryKeys([], []).

                dictionaryValues([pair(_, Value) | Dictionary], [Value | Values]) :- 
                    dictionaryValues(Dictionary, Values).
                dictionaryValues([], []).

                dictionaryRemoveKeys([pair(Key, _) | Dictionary], Keys, Dictionary1) :- 
                    member(Key, Keys), 
                    dictionaryRemoveKeys(Dictionary, Keys, Dictionary1),
                    !.
                dictionaryRemoveKeys([X | Dictionary], Keys, [X | Dictionary1]) :- 
                    dictionaryRemoveKeys(Dictionary, Keys, Dictionary1).
                dictionaryRemoveKeys([], _, []).";

            var situations = new[]
            {
                    (
                        Description: "Finding a solution for the Volf-Goat-Cabbage crossing the river riddle using breadth-first approach",
                        DesiredFinalState: "миссия заканчивается успехом", 
                        ExpectedSolutions: new[] 
                        {
                            @"[
                                перевозит(фермер, коза, левый, правый), 
                                переправляется(фермер, правый, левый), 
                                перевозит(фермер, волк, левый, правый), 
                                перевозит(фермер, коза, правый, левый), 
                                перевозит(фермер, капуста, левый, правый), 
                                переправляется(фермер, правый, левый), 
                                перевозит(фермер, коза, левый, правый)
                              ]"
                        }
                    ),

                    (
                        Description: "Make Volf-Goat-Cabbage crossing the river riddle end up in the 'волк съел козу' state",
                        DesiredFinalState: "волк съел козу",
                        ExpectedSolutions: new[] 
                        {
                            "[перевозит(фермер, капуста, левый, правый)]",
                            @"[
                                перевозит(фермер, коза, левый, правый), 
                                переправляется(фермер, правый, левый), 
                                перевозит(фермер, волк, левый, правый), 
                                переправляется(фермер, правый, левый)
                              ]"
                        }
                    ),

                    (
                        Description: "Make Volf-Goat-Cabbage crossing the river riddle end up in the 'коза съела капусту' state",
                        DesiredFinalState: "коза съела капусту",
                        ExpectedSolutions: new[] 
                        {
                            "[перевозит(фермер, волк, левый, правый)]",
                            @"[
                                перевозит(фермер, коза, левый, правый), 
                                переправляется(фермер, правый, левый), 
                                перевозит(фермер, капуста, левый, правый), 
                                переправляется(фермер, правый, левый)
                            ]"
                        }
                    )
            };

            CheckSituations(situations.Select(s => 
                    (
                        s.Description,
                        program,
                        @"начальноеСостояние(InitialState),
                            FinalStateName = '" + s.DesiredFinalState + @"',
                            конечноеСостояние(FinalStateName, FinalState),
                            solve(InitialState, FinalStateName, FinalState, Solution),
                            dictionaryValues(Solution, Route)",
                        System.Array.ConvertAll(s.ExpectedSolutions, es => new V { ["Route"] = es })
                    )),
                ignoreUnexpectedActualInstantiations: true);
        }

        [ClassInitialize] public static void TestClassInitialize(TestContext? testContext) => SetupLogging(testContext);
    }
}