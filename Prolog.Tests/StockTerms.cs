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

        public static readonly Variable X = new ("X");
        public static readonly Variable X1 = new ("X1");
        public static readonly Variable Y = new ("Y");
        public static readonly Variable Z = new ("Z");
        public static readonly Variable P = new ("P");
        public static readonly Variable A = new ("A");
        public static readonly Variable B = new ("B");
        public static readonly Variable C = new ("C");
        public static readonly Variable R = new ("R");
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

        public static readonly Atom atom = new ("atom");
        public static readonly Atom a = new ("a");
        public static readonly Atom b = new ("b");
        public static readonly Atom c = new ("c");
        public static readonly Atom d = new ("d");
        public static readonly Atom right = new ("right");
        public static readonly Atom left = new ("left");

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

        private static ComplexTermFactory MakeComplexTerm(string functorName) => 
            arguments => ComplexTerm(Functor(functorName, arguments.Length), arguments);
        // ReSharper enable InconsistentNaming
    }
}