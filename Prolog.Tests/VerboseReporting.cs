using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Prolog.Engine;
using Prolog.Engine.Miscellaneous;
using static Prolog.Engine.DomainApi;

namespace Prolog.Tests
{
    using SentenceMeaning = Either<IReadOnlyCollection<Rule>, IReadOnlyCollection<ComplexTerm>>;

    internal static class VerboseReporting
    {
        public static string Dump<T>(T @this, string enumSeparator = "; ") =>
            @this switch
            {
                Atom atom => atom.Characters,
                Number number => number.Value.ToString(CultureInfo.InvariantCulture),
                Variable variable => variable.Name,
                ComplexTerm cutOrFail when cutOrFail == Builtin.Cut || cutOrFail == Builtin.Fail => cutOrFail.Functor.Name,
                ComplexTerm list when list.IsList() => "[" + string.Join(", ", IterableList(list, strictMode: false).Select(t => Dump(t))) + "]",
                ComplexTerm complexTerm => $"{complexTerm.Functor.Name}({string.Join(',', complexTerm.Arguments.Select(a => Dump(a, enumSeparator)))})",
                Rule fact when !fact.Premises.Any() => $"{Dump(fact.Conclusion)}.",
                Rule rule => $"{Dump(rule.Conclusion, enumSeparator)}:-{string.Join(',', rule.Premises.Select(p => Dump(p, enumSeparator)))}",
                UnificationResult unificationResult => unificationResult.Succeeded ? "success(" +  string.Join(" & ",unificationResult.Instantiations.Select(i => $"{Dump(i.Key, enumSeparator)} = {Dump(i.Value, enumSeparator)}")) + ")" : "no unification possible",
                IReadOnlyDictionary<Variable, Term> variableInstantiations => string.Join(", ", variableInstantiations.Select(kvp => $"[{Dump(kvp.Key)}] = {Dump(kvp.Value)}")),
                ValueTuple<Term, Term> unification => $"({Dump(unification.Item1)}, {Dump(unification.Item2)})",
                SentenceMeaning sentenceMeaning => sentenceMeaning.Fold(rules => Dump(rules), statements => Dump(statements)),
                string text => text,
                IEnumerable collection => string.Join(enumSeparator, collection.Cast<object>().Select(it => Dump(it, enumSeparator))),
                _ => @this?.ToString() ?? "NULL"
            };
    }
}