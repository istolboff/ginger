using System;
using System.Collections.Generic;
using System.Linq;
using static Prolog.Engine.Builtin;

namespace Prolog.Engine
{
    public static class DomainApi
    {
        public delegate ComplexTerm ComplexTermFactory(params Term[] arguments);

        public static Atom Atom(string atomText) => new (atomText);

        public static Number Number(int value) => new (value);

        public static Variable Variable(string name) => new (name);

        public static Functor Functor(string name, int arity = 0) => new (name, arity);

        public static ComplexTerm ComplexTerm(FunctorBase functor, params Term[] arguments) => 
            new (functor, new (arguments));

        public static ComplexTerm ComplexTerm(FunctorBase functor, IEnumerable<Term> arguments) => 
            new (functor, new (arguments));

        public static Rule Fact(ComplexTerm fact) => 
            new (Conclusion: fact, Premises: StructuralEquatableArray.Empty<ComplexTerm>());

        public static Rule Rule(ComplexTerm conclusion, params ComplexTerm[] premises) => 
            new (conclusion, new (premises));

        public static Rule Rule(ComplexTerm conclusion, IEnumerable<ComplexTerm> premises) =>
            new (conclusion, new (premises));

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