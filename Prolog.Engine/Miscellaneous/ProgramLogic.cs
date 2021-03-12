using System;

namespace Prolog.Engine.Miscellaneous
{
    internal static class ProgramLogic
    {
        public static InvalidOperationException Error(string message) =>
            new ("Program Logic Error: " + message);
    }
}