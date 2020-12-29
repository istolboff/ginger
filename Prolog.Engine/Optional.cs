using System;

namespace Prolog.Engine
{
    internal static class Optional
    {
        public static Optional<T> Some<T>(T value) => 
            new (value, true);

        public static Optional<T> None<T>() => 
            new (default, false);
    }

    internal sealed record Optional<T>(T? Value, bool HasValue)
    {
        public Optional<TProjected> Map<TProjected>(Func<T, TProjected> f) => 
            new (HasValue ? f(Value!) : default(TProjected), HasValue);

        public T OrElse(T defaultValue) => 
            HasValue ? Value! : defaultValue;

        public T OrElse(Func<T> defaultValue) => 
            HasValue ? Value! : defaultValue();
    }
}