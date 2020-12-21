using System;
using System.Collections.Generic;

namespace Prolog.Engine
{
    public static class Builtin
    {
#pragma warning disable CA1707
        public static readonly Variable _ = new Variable("_");
#pragma warning restore CA1707

        public static readonly ComplexTerm Cut = new ComplexTerm(new Functor("!", 0), new StructuralEquatableArray<Term>());

        public static readonly ComplexTerm Fail = new ComplexTerm(new Functor("fail", 0), new StructuralEquatableArray<Term>());

        public static readonly Functor Call = new Functor("call", 1);

        public static readonly string Not = "not";

        public static readonly IReadOnlyCollection<Rule> Rules = new[]
        {
            new Rule(
                new ComplexTerm(new Functor(Not, 1), new StructuralEquatableArray<Term>(new Variable("X"))),
                new StructuralEquatableArray<ComplexTerm>(
                    new ComplexTerm(Call, new StructuralEquatableArray<Term>(new Variable("X"))),
                    Cut,
                    Fail)),
            new Rule(
                new ComplexTerm(new Functor(Not, 1), new StructuralEquatableArray<Term>(_)),
                new StructuralEquatableArray<ComplexTerm>())
        };

        public static readonly BuiltinFunctor GreaterThanOrEqual = 
            new BuiltinFunctor(
                ">=", 
                2, 
                arguments => arguments[0] switch 
                {
                    Number n1 when arguments[1] is Number n2 => n1.Value >= n2.Value,
                    Atom a1 when arguments[1] is Atom a2 => string.Compare(a1.Characters, a2.Characters, StringComparison.OrdinalIgnoreCase) >= 0,
                    _ => false
                });

        public static readonly BuiltinFunctor LessThan = 
            new BuiltinFunctor(
                "<", 
                2, 
                arguments => arguments[0] switch 
                {
                    Number n1 when arguments[1] is Number n2 => n1.Value < n2.Value,
                    Atom a1 when arguments[1] is Atom a2 => string.Compare(a1.Characters, a2.Characters, StringComparison.OrdinalIgnoreCase) < 0,
                    _ => false
                });
    }
}