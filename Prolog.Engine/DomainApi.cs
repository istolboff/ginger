using System;
using System.Linq;
using static Prolog.Engine.Builtin;

namespace Prolog.Engine
{
    public static class DomainApi
    {
        public delegate ComplexTerm ComplexTermFactory(params Term[] arguments);

        public static Atom Atom(string atomText) => new Atom(atomText);

        public static Number Number(int value) => new Number(value);

        public static Variable Variable(string name) => new Variable(name);

        public static Functor Functor(string name, int arity = 0) => new Functor(name, arity);

        public static ComplexTerm ComplexTerm(FunctorBase functor, params Term[] arguments) => 
            new ComplexTerm(functor, new StructuralEquatableArray<Term>(arguments));

        public static Rule Fact(ComplexTerm fact) => 
            new Rule(Conclusion: fact, Premises: StructuralEquatableArray<ComplexTerm>.Empty);

        public static Rule Rule(ComplexTerm conclusion, params ComplexTerm[] premises) => 
            new Rule(conclusion, new StructuralEquatableArray<ComplexTerm>(premises));

        public static ComplexTerm List(params Term[] elements) => 
            elements.Reverse().Aggregate(EmptyList, (list, element) => Dot(element, list));

        internal static ComplexTermFactory StandardOperator(string operatorName, Func<Term, Term, bool> invoke) =>
            formalArguments => ComplexTerm(
                new BuiltinFunctor(
                    operatorName, 
                    2, 
                    arguments => invoke(arguments[0], arguments[1])),
                formalArguments);

   }
}