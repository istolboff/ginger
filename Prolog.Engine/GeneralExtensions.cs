using System;
using System.Collections.Generic;
using System.Linq;

using static Prolog.Engine.MayBe;

namespace Prolog.Engine
{
    internal static class GeneralExtensions
    {
        public static TResult Apply<T, TResult>(this T @this, Func<T, TResult> f) =>
            f(@this);

        public static MayBe<TValue> TryFind<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> @this, TKey key) =>
            @this.TryGetValue(key, out var result) ? Some(result) : None;

        public static bool IsOneOf<T>(this T value, T alternative1, T alternative2)
            => EqualityComparer<T>.Default.Equals(value, alternative1) || 
               EqualityComparer<T>.Default.Equals(value, alternative2);

        public static bool IsOneOf<T>(this T value, T alternative1, T alternative2, T alternative3)
            => EqualityComparer<T>.Default.Equals(value, alternative1) || 
               EqualityComparer<T>.Default.Equals(value, alternative2) ||
               EqualityComparer<T>.Default.Equals(value, alternative3);
    }
}
