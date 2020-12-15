using System.Collections.Generic;
using System.Linq;

namespace Prolog.Tests
{
    internal static class VerboseReporting
    {
        public static VerboseArray<T> Dumpable<T>(IReadOnlyCollection<T> elements) => new VerboseArray<T>(elements);
    }

    internal sealed record VerboseArray<T>(IReadOnlyCollection<T> elements)
    {
        public override string ToString() => 
            "[" + string.Join(", ", elements.Take(3)) + (elements.Count > 3 ? "..." : string.Empty) + "]";
    }
}