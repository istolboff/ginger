using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Prolog.Engine.Miscellaneous
{
    public static class StructuralEquatableArray
    {
        public static StructuralEquatableArray<T> Empty<T>() => 
            new (Array.Empty<T>());
    }

    public sealed class StructuralEquatableArray<T> : IReadOnlyList<T>, IEquatable<StructuralEquatableArray<T>>
    {
        public StructuralEquatableArray(params T[] values) => _values = values;

        public StructuralEquatableArray(IEnumerable<T> values) => _values = values.ToArray();

        public T this[int index] => _values[index];

        public int Count => _values.Length;

        public IEnumerator<T> GetEnumerator() => (_values as IEnumerable<T>).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();

        public bool Equals(StructuralEquatableArray<T>? other) =>
            StructuralComparisons.StructuralEqualityComparer.Equals(_values, other?._values);

        public override bool Equals(object? obj) => Equals(obj as StructuralEquatableArray<T>);

        public override int GetHashCode() => 
            _values.Length switch
            {
                0 => 0,
                1 => _values[0]?.GetHashCode() ?? 0,
                2 => (_values[0]?.GetHashCode() ?? 0) * -1521134295 + (_values[1]?.GetHashCode() ?? 0),
                _ => ((_values[0]?.GetHashCode() ?? 0) * -1521134295 + (_values[1]?.GetHashCode() ?? 0)) * -1521134295 + (_values[2]?.GetHashCode() ?? 0)
            };

        public override string ToString() =>
            "[" + string.Join(", ", _values.Take(3)) + (_values.Length > 3 ? "..." : string.Empty) + "]";

        public static bool operator ==(StructuralEquatableArray<T>? left, StructuralEquatableArray<T>? right) =>
            ReferenceEquals(left, right) ||
            (!ReferenceEquals(left, null) && left.Equals(right));

        public static bool operator !=(StructuralEquatableArray<T>? left, StructuralEquatableArray<T>? right) =>
            !(left == right);

        private readonly T[] _values;
    }
}