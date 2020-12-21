using System.Collections;
using System.Globalization;
using System.Linq;
using Prolog.Engine;

namespace Prolog.Tests
{
    internal static class VerboseReporting
    {
        public static string Dump<T>(T @this, string enumSeparator = "; ") =>
            @this switch
            {
                Atom atom => atom.Characters,
                Number number => number.Value.ToString(CultureInfo.InvariantCulture),
                Variable variable => variable.Name,
                ComplexTerm complexTerm => $"{complexTerm.Functor.Name}({string.Join(',', complexTerm.Arguments.Select(a => Dump(a, enumSeparator)))})",
                Rule rule => $"{Dump(rule.Conclusion, enumSeparator)}:-{string.Join(',', rule.Premises.Select(p => Dump(p, enumSeparator)))}",
                UnificationResult unificationResult => string.Join(" & ",unificationResult.Instantiations.Select(i => $"{Dump(i.Key, enumSeparator)} = {Dump(i.Value, enumSeparator)}")),
                string text => text,
                IEnumerable collection => string.Join(enumSeparator, collection.Cast<object>().Select(it => Dump(it, enumSeparator))),
                _ => @this?.ToString() ?? "NULL"
            };
    }
}