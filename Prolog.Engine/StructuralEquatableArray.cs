using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Prolog.Engine
{
    public sealed class StructuralEquatableArray<T> : IReadOnlyList<T>, IEquatable<StructuralEquatableArray<T>>
    {
        public StructuralEquatableArray(params T[] values) => _values = values;

        public StructuralEquatableArray(IEnumerable<T> values) => _values = values.ToArray();

        public T this[int index] => _values[index];

        public int Count => _values.Count;

        public IEnumerator<T> GetEnumerator() => _values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();

        public bool Equals(StructuralEquatableArray<T>? other) =>
            StructuralComparisons.StructuralEqualityComparer.Equals(_values, other?._values);

        public override bool Equals(object? obj) => Equals(obj as StructuralEquatableArray<T>);

        public override int GetHashCode() => _values.GetHashCode();

        public override string ToString() =>
            "[" + string.Join(", ", _values.Take(3)) + (_values.Count > 3 ? "..." : string.Empty) + "]";

        public static bool operator ==(StructuralEquatableArray<T>? left, StructuralEquatableArray<T>? right) =>
            ReferenceEquals(left, right) ||
            (!ReferenceEquals(left, null) && left.Equals(right));

        public static bool operator !=(StructuralEquatableArray<T>? left, StructuralEquatableArray<T>? right) =>
            !(left == right);

        public static readonly StructuralEquatableArray<ComplexTerm> Empty = 
            new StructuralEquatableArray<ComplexTerm>(Array.Empty<ComplexTerm>());

        private readonly IReadOnlyList<T> _values;
    }
}