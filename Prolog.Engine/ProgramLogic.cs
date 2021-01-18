using System;

namespace Prolog.Engine
{
    internal static class ProgramLogic
    {
        public static InvalidOperationException Error(string message) =>
            new InvalidOperationException("Program Logic Error: " + message);
    }
}