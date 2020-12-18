using System;

namespace Prolog.Engine
{
    public static class Builtin
    {
        public static readonly ComplexTerm Cut = new ComplexTerm(new Functor("!", 0), new StructuralEquatableArray<Term>());

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