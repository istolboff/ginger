using System;
using System.Collections.Generic;
using System.Linq;

using static Prolog.Engine.MayBe;

namespace Prolog.Engine
{
    internal static class MoreLinqMethods
    {
        public static IReadOnlyCollection<T> AsImmutable<T>(this IEnumerable<T> @this) =>
            @this as IReadOnlyCollection<T> ?? @this.ToArray();

        public static MayBe<T> TryFirst<T>(this IEnumerable<T> @this, Func<T, bool> predicate)
        {
            foreach (var element in @this)
            {
                if (predicate(element))
                {
                    return Some(element);
                }
            }

            return None;
        }
            
        public static MayBe<T> TryFirst<T>(this IEnumerable<T> @this) =>
            @this.TryFirst(_ => true);

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

        public static IEnumerable<TPartitioned> Partition<T, TPartitioned>(
            this IEnumerable<T> @this, 
            Func<T, T, TPartitioned> makePartition)
        {
            var first = default(T);
            var setFirstElement = true;

            foreach (var item in @this)
            {
                if (setFirstElement)
                {
                    first = item;
                    setFirstElement = false;
                }
                else
                {
                    yield return makePartition(first!, item);
                    setFirstElement = true;
                }
            }
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> @this, Func<T, bool> isSplitter)
        {
            var nextValue = new List<T>();
            foreach (var item in @this)
            {
                if (isSplitter(item))
                {
                    if (nextValue.Any())
                    {
                        yield return nextValue;
                        nextValue = new List<T>();
                    }
                }
                else
                {
                    nextValue.Add(item);
                }
            }

            if (nextValue.Any())
            {
                yield return nextValue;
            }
        }
    }
}