using Prolog.Engine;

namespace Prolog.Tests
{
    public static class PrologApi
    {
        public static Atom Atom(string atomText) => new Atom(atomText);

        public static Number Number(int value) => new Number(value);

        public static Variable Variable(string name) => new Variable(name);

        public static Functor Functor(string name, int arity) => new Functor(name, arity);

        public static ComplexTerm ComplexTerm(Functor functor, params Term[] arguments) => 
            new ComplexTerm(functor, new StructuralEquatableArray<Term>(arguments));

        public static Variable UnboundVariable(int id) => Variable("_6532");
   }
}