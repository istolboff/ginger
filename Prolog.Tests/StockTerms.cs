using Prolog.Engine;
using static Prolog.Tests.PrologApi;

namespace Prolog.Tests
{
    internal static class StockTerms
    {
        public delegate ComplexTerm ComplexTermFactory(params Term[] arguments);

        public static readonly ComplexTermFactory GreaterThanOrEqual = MakeBuiltinOperator(Builtin.GreaterThanOrEqual);
        public static readonly ComplexTermFactory LessThan = MakeBuiltinOperator(Builtin.LessThan);

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
        public static readonly ComplexTermFactory not = MakeComplexTerm(Builtin.Not);

        public static readonly Variable X = Variable("X");
        public static readonly Variable X1 = Variable("X1");
        public static readonly Variable Y = Variable("Y");
        public static readonly Variable Z = Variable("Z");
        public static readonly Variable P = Variable("P");

        public static readonly Atom atom = Atom("atom");
        public static readonly Atom a = Atom("a");
        public static readonly Atom b = Atom("b");
        public static readonly Atom c = Atom("c");
        public static readonly Atom d = Atom("d");
        public static readonly Atom vincent = Atom("vincent");

        public static readonly Number zero = Number(0);
        public static readonly Number one = Number(1);
        public static readonly Number two = Number(2);
        public static readonly Number three = Number(3);

        public static readonly Atom Something = Atom("AnUnimportantVariable");
        public static readonly Atom SomethingElse = Atom("AnotherUnimportantVariable");

        private static ComplexTermFactory MakeComplexTerm(string functorName) => 
            arguments => ComplexTerm(Functor(functorName, arguments.Length), arguments);

        private static ComplexTermFactory MakeBuiltinOperator(BuiltinFunctor builtinFunctor) =>
            arguments => ComplexTerm(builtinFunctor, arguments);
    }
}