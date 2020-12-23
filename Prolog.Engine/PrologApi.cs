using System.Collections.Generic;
using System.Linq;
using Prolog.Engine;

namespace Prolog.Tests
{
    public static class PrologApi
    {
        public static Atom Atom(string atomText) => new Atom(atomText);

        public static Number Number(int value) => new Number(value);

        public static Variable Variable(string name) => new Variable(name);

        public static Functor Functor(string name, int arity) => new Functor(name, arity);

        public static ComplexTerm ComplexTerm(FunctorBase functor, params Term[] arguments) => 
            new ComplexTerm(functor, new StructuralEquatableArray<Term>(arguments));

        public static Variable UnboundVariable(int id) => Variable($"_{id}");

        public static Rule Fact(ComplexTerm fact) => 
            new Rule(Conclusion: fact, Premises: StructuralEquatableArray<ComplexTerm>.Empty);

        public static Rule Rule(ComplexTerm conclusion, params ComplexTerm[] premises) => 
            new Rule(conclusion, new StructuralEquatableArray<ComplexTerm>(premises));

        public static readonly UnificationResult Success = 
            new UnificationResult(Succeeded: true, Instantiations: new StructuralEquatableDictionary<Variable, Term>());
   }
}