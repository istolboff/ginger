using System;

namespace Prolog.Engine
{
    internal static class GeneralExtensions
    {
        public static TResult Apply<T, TResult>(this T @this, Func<T, TResult> f) =>
            f(@this);
    }
}
