using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Prolog.Engine
{
    public sealed class StructuralEquatableDictionary<TKey, TValue> :
        IReadOnlyDictionary<TKey, TValue>,
        IEquatable<StructuralEquatableDictionary<TKey, TValue>>
        where TKey : notnull
    {
        public StructuralEquatableDictionary() => 
            _dictionary = new ();

        public StructuralEquatableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) =>
            _dictionary = new (collection);

        public TValue this[TKey key] => _dictionary[key];

        public IEnumerable<TKey> Keys => _dictionary.Keys;

        public IEnumerable<TValue> Values => _dictionary.Values;

        public int Count => _dictionary.Count;

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) =>
            _dictionary.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

        public bool Equals(StructuralEquatableDictionary<TKey, TValue>? other) =>
            other != null &&
            Count == other.Count &&
            other.All(kvp => TryGetValue(kvp.Key, out var value) &&
                             EqualityComparer<TValue>.Default.Equals(kvp.Value, value));

        public override bool Equals(object? obj) => Equals(obj as StructuralEquatableDictionary<TKey, TValue>);

        public override int GetHashCode() => _dictionary.GetHashCode();

        public override string ToString() =>
            "{" +
                string.Join(
                    ", ",
                    _dictionary.Take(3).Select(kvp => $"[{kvp.Key}] = {kvp.Value}")) +
                (Count > 3 ? "..." : string.Empty) +
            "}";

        public static bool operator ==(StructuralEquatableDictionary<TKey, TValue>? left, StructuralEquatableDictionary<TKey, TValue>? right) =>
            ReferenceEquals(left, right) ||
            (!ReferenceEquals(left, null) && left.Equals(right));

        public static bool operator !=(StructuralEquatableDictionary<TKey, TValue>? left, StructuralEquatableDictionary<TKey, TValue>? right) =>
            !(left == right);

        private readonly Dictionary<TKey, TValue> _dictionary;
    }
}