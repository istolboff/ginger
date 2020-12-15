using Prolog.Engine;
using static Prolog.Tests.PrologApi;

namespace Prolog.Tests
{
    internal static class StockTerms
    {
        public static ComplexTerm f(params Term[] arguments) => ComplexTerm(Functor("f", arguments.Length), arguments);

        public static ComplexTerm g(params Term[] arguments) => ComplexTerm(Functor("g", arguments.Length), arguments);

        public static ComplexTerm h(params Term[] arguments) => ComplexTerm(Functor("h", arguments.Length), arguments);

        public static ComplexTerm edge(params Term[] arguments) => ComplexTerm(Functor("edge", arguments.Length), arguments);

        public static ComplexTerm path(params Term[] arguments) => ComplexTerm(Functor("path", arguments.Length), arguments);

        public static ComplexTerm date(params Term[] arguments) => ComplexTerm(Functor("date", arguments.Length), arguments);

        public static ComplexTerm line(params Term[] arguments) => ComplexTerm(Functor("line", arguments.Length), arguments);

        public static ComplexTerm point(params Term[] arguments) => ComplexTerm(Functor("point", arguments.Length), arguments);

        public static ComplexTerm vertical(params Term[] arguments) => ComplexTerm(Functor("vertical", arguments.Length), arguments);

        public static ComplexTerm horizontal(params Term[] arguments) => ComplexTerm(Functor("horizontal", arguments.Length), arguments);

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

        public static readonly Number one = Number(1);
        public static readonly Number two = Number(2);
        public static readonly Number three = Number(3);

        public static readonly Atom Something = Atom("AnUnimportantVariable");
        public static readonly Atom SomethingElse = Atom("AnotherUnimportantVariable");
    }
}