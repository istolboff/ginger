using System;
using System.Collections.Generic;
using System.Linq;

using static Prolog.Engine.Optional;

namespace Prolog.Engine
{
    internal static class MoreLinqMethods
    {
        public static IReadOnlyCollection<T> AsImmutable<T>(this IEnumerable<T> @this) =>
            @this as IReadOnlyCollection<T> ?? @this.ToArray();

        public static Optional<T> TryFirst<T>(this IEnumerable<T> @this, Func<T, bool> predicate)
        {
            using var enumerator = @this.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (predicate(enumerator.Current))
                {
                    return Some(enumerator.Current);
                }
            }

            return None<T>();
        }
            
        public static TAccumulate AggregateWhile<TSource, TAccumulate>(
            this IEnumerable<TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func,
            Func<TAccumulate, bool> keepGoing)
        {
            using var sourceEnumerator = source.GetEnumerator();
            var result = seed;
            while (keepGoing(result) && sourceEnumerator.MoveNext())
            {
                result = func(result, sourceEnumerator.Current);
            }

            return result;
        }

        public static (TAccumulate, bool) AggregateIfAll<TSource, TAccumulate>(
            this IEnumerable<TSource> source,
            TAccumulate seed,
            Func<TSource, bool> predicate,
            Func<TAccumulate, TSource, TAccumulate> func)
        {
            using var sourceEnumerator = source.GetEnumerator();
            var result = seed;
            while (sourceEnumerator.MoveNext())
            {
                if (!predicate(sourceEnumerator.Current))
                {
                    return (result, false);
                }

                result = func(result, sourceEnumerator.Current);
            }

            return (result, true);
        }

        public static TValue AddAndReturnValue<TKey, TValue>(
            this IDictionary<TKey, TValue> @this, 
            TKey key, 
            TValue value)
        {
            @this.Add(key, value);
            return value;
        }

        public static IDictionary<TKey, TValue> AddAndReturnSelf<TKey, TValue>(
            this IDictionary<TKey, TValue> @this, 
            KeyValuePair<TKey, TValue> item)
        {
            @this.Add(item);
            return @this;
        }

        public static HashSet<T> AddAndReturnSelf<T>(this HashSet<T> @this, T item)
        {
            @this.Add(item);
            return @this;
        }
    }
}