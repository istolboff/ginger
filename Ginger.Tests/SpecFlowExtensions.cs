using System;
using System.Collections.Generic;
using System.Linq;
using Prolog.Engine.Miscellaneous;
using TechTalk.SpecFlow;

namespace Ginger.Tests
{
    internal static class SpecFlowExtensions
    {
        public static IReadOnlyCollection<IReadOnlyDictionary<string, string>> GetMultilineRows(
            this Table @this, 
            bool singleLines = false) =>
            @this.Rows.Any(IsSeparatorRow)
                ? @this.Rows
                    .Split(IsSeparatorRow)
                    .Select(rows => @this.Header.ToDictionary(
                        header => header,
                        header => string.Join(
                            singleLines ? " " : Environment.NewLine, 
                            rows
                                .Select(r => r[header])
                                .Where(v => !string.IsNullOrWhiteSpace(v)))))
                    .AsImmutable()
                : @this.Rows.Select(r => r.AsReadOnlyDictionary()).AsImmutable();

        private static bool IsSeparatorRow(TableRow row) => 
            row.Values.All(value => value.All(ch => ch == '-'));
    }
}