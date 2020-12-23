using System;
using System.Collections.Generic;
using static Prolog.Engine.DomainApi;

namespace Prolog.Engine
{
    public static class Builtin
    {
#pragma warning disable CA1707
        public static readonly Variable _ = Variable("_");
#pragma warning restore CA1707

        public static readonly ComplexTerm True = ComplexTerm(Functor("True"));

        public static readonly ComplexTerm Cut = ComplexTerm(Functor("!"));

        public static readonly ComplexTerm Fail = ComplexTerm(Functor("fail"));

        public static readonly ComplexTerm EmptyList = ComplexTerm(Functor("[]"));

        public static readonly Functor CallFunctor = Functor("call", 1);

        public static readonly ComplexTermFactory GreaterThanOrEqual = StandardOperator(
            ">=", 
            (left, right) => 
                left switch 
                {
                    Number n1 when right is Number n2 => n1.Value >= n2.Value,
                    Atom a1 when right is Atom a2 => string.Compare(a1.Characters, a2.Characters, StringComparison.OrdinalIgnoreCase) >= 0,
                    _ => false
                });

        public static readonly ComplexTermFactory LessThan = StandardOperator(
            "<", 
            (left, right) => 
                left switch 
                {
                    Number n1 when right is Number n2 => n1.Value < n2.Value,
                    Atom a1 when right is Atom a2 => string.Compare(a1.Characters, a2.Characters, StringComparison.OrdinalIgnoreCase) < 0,
                    _ => false
                });

        public static ComplexTerm Dot(Term head, Term tail) =>
            ComplexTerm(Functor(".", 2), head, tail);

        public static ComplexTerm Member(Term element, Term list) =>
            ComplexTerm(Functor("member", 2), element, list);

        public static ComplexTerm Not(Term term) => 
            ComplexTerm(Functor("not", 1), term);

        public static ComplexTerm Call(Term callee) =>
            ComplexTerm(CallFunctor, callee);

        public static readonly IReadOnlyCollection<Rule> Rules = new[]
        {
            // not()
            Rule(Not(X), Call(X), Cut, Fail),
            Fact(Not(_)),

            // member()
            Fact(Member(X, Dot(X, _))),
            Rule(Member(X, Dot(_, T)), Member(X, T)),
        };

        // stock terms, usable for formulating built-in rules
        private static Variable X => Variable("X");
        private static Variable T => Variable("T");
    }
}