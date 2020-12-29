using System;

namespace Prolog.Engine
{
    internal static class MakeCompilerHappy
    {
        public static T SuppressCa1062<T>(T? reference) where T : class => 
            reference ?? throw new ArgumentNullException(nameof(reference));
    }
}