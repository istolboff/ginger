using System;
using System.Collections.Generic;
using System.Linq;
using Prolog.Engine;
using TechTalk.SpecFlow;

namespace Ginger.Tests
{
    internal static class SpecFlowExtensions
    {
        public static IReadOnlyCollection<IDictionary<string, string>> GetMultilineRows(this Table @this) =>
            @this.Rows.Any(IsSeparatorRow)
                ? @this.Rows
                    .Split(IsSeparatorRow)
                    .Select(rows => @this.Header.ToDictionary(
                        header => header,
                        header => string.Join(
                            Environment.NewLine, 
                            rows
                                .Select(r => r[header])
                                .Where(v => !string.IsNullOrWhiteSpace(v)))))
                    .AsImmutable()
                : @this.Rows.AsImmutable();

        private static bool IsSeparatorRow(TableRow row) => 
            row.Values.All(value => value.All(ch => ch == '-'));
    }
}