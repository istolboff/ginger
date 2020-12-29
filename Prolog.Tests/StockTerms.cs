using Prolog.Engine;
using static Prolog.Engine.DomainApi;

namespace Prolog.Tests
{
    internal static class StockTerms
    {
        // ReSharper disable InconsistentNaming
        public static readonly ComplexTermFactory f = MakeComplexTerm("f");
        public static readonly ComplexTermFactory g = MakeComplexTerm("g");
        public static readonly ComplexTermFactory h = MakeComplexTerm("h");
        public static readonly ComplexTermFactory s = MakeComplexTerm("s");
        public static readonly ComplexTermFactory p = MakeComplexTerm("p");
        public static readonly ComplexTermFactory q = MakeComplexTerm("q");
        public static readonly ComplexTermFactory i = MakeComplexTerm("i");
        public static readonly ComplexTermFactory j = MakeComplexTerm("j");
        public static readonly ComplexTermFactory edge = MakeComplexTerm("edge");
        public static readonly ComplexTermFactory path = MakeComplexTerm("path");
        public static readonly ComplexTermFactory date = MakeComplexTerm("date");
        public static readonly ComplexTermFactory line = MakeComplexTerm("line");
        public static readonly ComplexTermFactory point = MakeComplexTerm("point");
        public static readonly ComplexTermFactory vertical = MakeComplexTerm("vertical");
        public static readonly ComplexTermFactory horizontal = MakeComplexTerm("horizontal");
        public static readonly ComplexTermFactory max = MakeComplexTerm("max");
        public static readonly ComplexTermFactory number = MakeComplexTerm("number");
        public static readonly ComplexTermFactory enjoys = MakeComplexTerm("enjoys");
        public static readonly ComplexTermFactory burger = MakeComplexTerm("burger");
        public static readonly ComplexTermFactory big_kahuna_burger = MakeComplexTerm("big_kahuna_burger");
        public static readonly ComplexTermFactory big_mac = MakeComplexTerm("big_mac");
        public static readonly ComplexTermFactory whopper = MakeComplexTerm("whopper");
        public static readonly ComplexTermFactory directTrain = MakeComplexTerm("directTrain");
        public static readonly ComplexTermFactory connected = MakeComplexTerm("connected");
        public static readonly ComplexTermFactory route = MakeComplexTerm("route");
        public static readonly ComplexTermFactory riverbank = MakeComplexTerm("riverbank");
        public static readonly ComplexTermFactory canMove = MakeComplexTerm("canMove");
        public static readonly ComplexTermFactory state = MakeComplexTerm("state");
        public static readonly ComplexTermFactory atMostOneCreatureWasMoved = MakeComplexTerm("atMostOneCreatureWasMoved");
        public static readonly ComplexTermFactory sidesAreOk = MakeComplexTerm("sidesAreOk");
        public static readonly ComplexTermFactory solve = MakeComplexTerm("solve");
        public static readonly ComplexTermFactory depthfirst = MakeComplexTerm("depthfirst");
        public static readonly ComplexTermFactory берег = MakeComplexTerm("берег");
        public static readonly ComplexTermFactory перевозимоеCущество = MakeComplexTerm("перевозимоеCущество");
        public static readonly ComplexTermFactory действие = MakeComplexTerm("действие");
        public static readonly ComplexTermFactory перевозит = MakeComplexTerm("перевозит");
        public static readonly ComplexTermFactory фрагментСостояния = MakeComplexTerm("фрагментСостояния");
        public static readonly ComplexTermFactory находится = MakeComplexTerm("находится");
        public static readonly ComplexTermFactory переправляется = MakeComplexTerm("переправляется");
        public static readonly ComplexTermFactory начальноеСостояние = MakeComplexTerm("начальноеСостояние");
        public static readonly ComplexTermFactory конечноеСостояние = MakeComplexTerm("конечноеСостояние");
        public static readonly ComplexTermFactory выполнимо = MakeComplexTerm("выполнимо");
        public static readonly ComplexTermFactory unwantedFinalState = MakeComplexTerm("unwantedFinalState");
        public static readonly ComplexTermFactory breadthFirst = MakeComplexTerm("breadthFirst");
        public static readonly ComplexTermFactory listFullFinalStates = MakeComplexTerm("listFullFinalStates");
        public static readonly ComplexTermFactory buildPath = MakeComplexTerm("buildPath");
        public static readonly ComplexTermFactory adjacentNodes = MakeComplexTerm("adjacentNodes");
        public static readonly ComplexTermFactory dictionaryRemoveKeys = MakeComplexTerm("dictionaryRemoveKeys");
        public static readonly ComplexTermFactory buildConnectingEdges = MakeComplexTerm("buildConnectingEdges");
        public static readonly ComplexTermFactory dictionaryKeys = MakeComplexTerm("dictionaryKeys");
        public static readonly ComplexTermFactory dictionaryValues = MakeComplexTerm("dictionaryValues");
        public static readonly ComplexTermFactory pair = MakeComplexTerm("pair");
        public static readonly ComplexTermFactory canMoveToNewState = MakeComplexTerm("canMoveToNewState");
        public static readonly ComplexTermFactory buildPath1 = MakeComplexTerm("buildPath1");

        public static readonly Variable X = new ("X");
        public static readonly Variable X1 = new ("X1");
        public static readonly Variable Y = new ("Y");
        public static readonly Variable Z = new ("Z");
        public static readonly Variable P = new ("P");
        public static readonly Variable A = new ("A");
        public static readonly Variable B = new ("B");
        public static readonly Variable C = new ("C");
        public static readonly Variable R = new ("R");
        public static readonly Variable H = new ("H");
        public static readonly Variable Route = new ("Route");
        public static readonly Variable Visited = new ("Visited");
        public static readonly Variable FarmerPosition = new ("FarmerPosition");
        public static readonly Variable WolfPosition = new ("WolfPosition");
        public static readonly Variable GoatPosition = new ("GoatPosition");
        public static readonly Variable CabbagePosition = new ("CabbagePosition");
        public static readonly Variable NewFarmerPosition = new ("NewFarmerPosition");
        public static readonly Variable FarmerPosition1 = new ("FarmerPosition1");
        public static readonly Variable WolfPosition1 = new ("WolfPosition1");
        public static readonly Variable GoatPosition1 = new ("GoatPosition1");
        public static readonly Variable CabbagePosition1 = new ("CabbagePosition1");
        public static readonly Variable A1 = new ("A1");
        public static readonly Variable B1 = new ("B1");
        public static readonly Variable C1 = new ("C1");
        public static readonly Variable State = new ("State");
        public static readonly Variable Solution = new ("Solution");
        public static readonly Variable State1 = new ("State1");
        public static readonly Variable Solution1 = new ("Solution1");
        public static readonly Variable Path = new ("Path");
        public static readonly Variable ОдинБерег = new ("ОдинБерег");
        public static readonly Variable ДругойБерег = new ("ДругойБерег");
        public static readonly Variable ТекущееПолноеСостояние = new ("ТекущееПолноеСостояние");
        public static readonly Variable Действие = new ("Действие");
        public static readonly Variable ФрагментСостоянияДо = new ("ФрагментСостоянияДо");
        public static readonly Variable ФрагментСостоянияПосле = new ("ФрагментСостоянияПосле");
        public static readonly Variable ПолноеСостояниеПосле = new ("ПолноеСостояниеПосле");
        public static readonly Variable ExpectedFinalStateName = new ("ExpectedFinalStateName");
        public static readonly Variable CurrentState = new ("CurrentState");
        public static readonly Variable ConcreteFinalState = new ("ConcreteFinalState");
        public static readonly Variable InitialState = new ("InitialState");
        public static readonly Variable FinalStateName = new ("FinalStateName");
        public static readonly Variable FinalState = new ("FinalState");
        public static readonly Variable SortedInitialState = new ("SortedInitialState");
        public static readonly Variable SortedFinalState = new ("SortedFinalState");
        public static readonly Variable Result = new ("Result");
        public static readonly Variable FlattenedResult = new ("FlattenedResult");
        public static readonly Variable MatchingFullFinalStates = new ("MatchingFullFinalStates");
        public static readonly Variable ReversedPath = new ("ReversedPath");
        public static readonly Variable Finish = new ("Finish");
        public static readonly Variable FinishSubset = new ("FinishSubset");
        public static readonly Variable QueueAdded = new ("QueueAdded");
        public static readonly Variable QueueProcessed = new ("QueueProcessed");
        public static readonly Variable ConnectingEdges = new ("ConnectingEdges");
        public static readonly Variable Res = new ("Res");
        public static readonly Variable Adjacents = new ("Adjacents");
        public static readonly Variable Temp1 = new ("Temp1");
        public static readonly Variable Temp2 = new ("Temp2");
        public static readonly Variable StatesReachableIn1Step = new ("StatesReachableIn1Step");
        public static readonly Variable ExtendedQueueAdded = new ("ExtendedQueueAdded");
        public static readonly Variable NewState = new ("NewState");
        public static readonly Variable Action = new ("Action");
        public static readonly Variable From = new ("From");
        public static readonly Variable To = new ("To");
        public static readonly Variable Tail = new ("Tail");
        public static readonly Variable Start = new ("Start");
        public static readonly Variable FullFinalStates = new ("FullFinalStates");
        public static readonly Variable Edges = new ("Edges");
        public static readonly Variable Key = new ("Key");
        public static readonly Variable Keys = new ("Keys");
        public static readonly Variable Value = new ("Value");
        public static readonly Variable Values = new ("Values");
        public static readonly Variable Dictionary = new ("Dictionary");
        public static readonly Variable Dictionary1 = new ("Dictionary1");

        public static readonly Atom atom = new ("atom");
        public static readonly Atom a = new ("a");
        public static readonly Atom b = new ("b");
        public static readonly Atom c = new ("c");
        public static readonly Atom d = new ("d");
        public static readonly Atom right = new ("right");
        public static readonly Atom left = new ("left");
        public static readonly Atom река = new ("река");
        public static readonly Atom левый = new ("левый");
        public static readonly Atom правый = new ("правый");
        public static readonly Atom волк = new ("волк");
        public static readonly Atom коза = new ("коза");
        public static readonly Atom капуста = new ("капуста");
        public static readonly Atom фермер = new ("фермер");

        public static readonly Atom vincent = new ("vincent");

        public static readonly Atom dudweiler = new ("dudweiler");
        public static readonly Atom fahlquemont = new ("fahlquemont");
        public static readonly Atom forbach = new ("forbach");
        public static readonly Atom freyming = new ("freyming");
        public static readonly Atom metz = new ("metz");
        public static readonly Atom nancy = new ("nancy");
        public static readonly Atom saarbruecken = new ("saarbruecken");
        public static readonly Atom stAvold = new ("stAvold");
        
        public static readonly Atom Something = new ("AnUnimportantVariable");
        public static readonly Atom SomethingElse = new ("AnotherUnimportantVariable");

        public static readonly Number zero = new (0);
        public static readonly Number one = new (1);
        public static readonly Number two = new (2);
        public static readonly Number three = new (3);
        public static readonly Number ten = new (10);
        public static readonly Number twenty = new (20);
        public static readonly Number thirty = new (30);

        private static ComplexTermFactory MakeComplexTerm(string functorName) => 
            arguments => ComplexTerm(Functor(functorName, arguments.Length), arguments);
        // ReSharper enable InconsistentNaming
    }
}