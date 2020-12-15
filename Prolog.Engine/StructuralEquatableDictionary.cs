using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Prolog.Engine
{
    public sealed class StructuralEquatableDictionary<TKey, TValue> :
        IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>,
        IEnumerable<KeyValuePair<TKey, TValue>>,
        IEquatable<StructuralEquatableDictionary<TKey, TValue>>
        where TKey : notnull
    {
        public StructuralEquatableDictionary() => 
            _dictionary = new Dictionary<TKey, TValue>();

        public StructuralEquatableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) =>
            _dictionary = new Dictionary<TKey, TValue>(collection);

        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        public ICollection<TKey> Keys => _dictionary.Keys;

        public ICollection<TValue> Values => _dictionary.Values;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _dictionary.Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _dictionary.Values;

        public int Count => _dictionary.Count;

        public bool IsReadOnly => _dictionary.IsReadOnly;

        public void Add(TKey key, TValue value) => _dictionary.Add(key, value);

        public void Add(KeyValuePair<TKey, TValue> item) => _dictionary.Add(item);

        public void Clear() => _dictionary.Clear();

        public bool Contains(KeyValuePair<TKey, TValue> item) => _dictionary.Contains(item);

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
            _dictionary.CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

        public bool Remove(TKey key) => _dictionary.Remove(key);

        public bool Remove(KeyValuePair<TKey, TValue> item) => _dictionary.Remove(item);

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

        private readonly IDictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
    }
}