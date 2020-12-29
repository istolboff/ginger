using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Prolog.Engine
{
    internal sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T? x, T? y) =>
            ReferenceEquals(x, y);

        public int GetHashCode([DisallowNull] T obj) => 
            obj.GetHashCode();

        public static readonly IEqualityComparer<T> Default = new ReferenceEqualityComparer<T>();
    }
}