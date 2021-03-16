using System;
using System.Collections.Generic;
using System.Linq;
using Prolog.Engine.Miscellaneous;

namespace Prolog.Engine.Parsing
{
    using static Either;
    using static MayBe;

    internal sealed record TextInput(string Text, int Position)
    {
        public TextInput Skip(int n) => this with { Position = this.Position + n };

        public TextInput MoveTo(int n) => this with { Position = n };

        public TextInput SkipToEndOfLine() =>
            MoveTo(Text
                    .IndexOfAny(new[] { '\r', '\n' }, Position)
                    .Apply(i => i >= 0 ? i : Text.Length));
    }

    internal sealed record ParsingError(string Text, TextInput Location);

    internal sealed record Result<TValue>(TValue Value, TextInput Rest);

    internal delegate Either<ParsingError, Result<TValue>> Parser<TValue>(TextInput input);

#pragma warning disable CA1801 // Review unused parameters
    internal sealed record ParsingTracer(Action<string> TraceParsingEvent, int MaxNestingLevel = 64)
#pragma warning restore CA1801
    {
        public Parser<T> Trace<T>(Parser<T> parser, string parserName) =>
            input =>
            {
                if (_nestingLevel >= MaxNestingLevel)
                {
                    return parser(input);
                }

                var linePrefix = new string(' ', _nestingLevel * 3);
                TraceParsingEvent($"{linePrefix}+{parserName} <== {Dump(input)}");
                ++_nestingLevel;
                var result = parser(input);
                --_nestingLevel;
                TraceParsingEvent($"{linePrefix}-{parserName}: {result.Fold(e => $"failed; {e.Text} at {Dump(e.Location)}", r => r.Value?.ToString())}");
                return result;

                string Dump(TextInput i) => i.Text.Insert(i.Position, "▲");
            };

        private int _nestingLevel;
    }

    internal static class MonadicParsing
    {
        public static Parser<TValue2> Select<TValue, TValue2>(
            this Parser<TValue> parser,
            Func<TValue, TValue2> selector) =>
            input => parser(input).Map(r => Result(selector(r.Value), r.Rest));

        public static Parser<TValue2> SelectMany<TValue, TIntermediate, TValue2>(
            this Parser<TValue> parser,
            Func<TValue, Parser<TIntermediate>> selector,
            Func<TValue, TIntermediate, TValue2> projector) =>
            input => parser(input)
                .Apply(it =>
                    it
                        .Combine(it
                            .Map(r => selector(r.Value)(r.Rest))
                            .Flatten())
                        .Map(r => Result(projector(r.First.Value, r.Second.Value), r.Second.Rest)));

        public static Parser<TValue> Where<TValue>(this Parser<TValue> parser, Func<TValue, bool> predicate) =>
            input => parser(input).Apply(r => r.IsLeft || predicate(r.Right!.Value)
                    ? r
                    : Left(new ParsingError($"Unexpected value {r.Right!.Value}", input)));

        public static Parser<TValue2> Then<TValue1, TValue2>(this Parser<TValue1> parser1, Parser<TValue2> parser2) =>
            input => parser1(input).Map(r => parser2(r.Rest)).Flatten();

        public static Parser<MayBe<TValue>> Optional<TValue>(Parser<TValue> parser) =>
            input => Right(parser(input).Fold(_ => Result(MakeNone<TValue>(), input), r => Result(Some(r.Value), r.Rest)));

        public static Parser<TValue> Or<TValue>(Parser<TValue> parser1, Parser<TValue> parser2) =>
            input => parser1(input).Fold(_ => parser2(input), Right<ParsingError, Result<TValue>>);

        public static Parser<TValue> Or<TValue>(
            Parser<TValue> parser1,
            Parser<TValue> parser2,
            Parser<TValue> parser3,
            params Parser<TValue>[] parsers) =>
            parsers.Aggregate(Or(Or(parser1, parser2), parser3), Or);

        public static Parser<IReadOnlyCollection<TValue>> Repeat<TValue>(
            Parser<TValue> parser,
            bool atLeastOnce = false) =>
        input => 
        {
            var result = new List<TValue>();
            while (true)
            {
                var nextElement = parser(input);
                if (nextElement.IsLeft)
                {
                    return (!atLeastOnce || result.Any()) switch
                    {
                        true => Right(Result(result.AsImmutable(), input)),
                        false => Left(nextElement.Left!)
                    };
                }

                result.Add(nextElement.Right!.Value);
                input = nextElement.Right!.Rest;
            }
        };

        public static Parser<IReadOnlyCollection<TValue>> Repeat<TValue, TSeparator>(
            Parser<TValue> parser,
            Parser<TSeparator> separatorParser,
            bool atLeastOnce = false) =>
            atLeastOnce
                ? from firstElement in parser
                  from theOtherElements in Repeat(separatorParser.Then(parser))
                  select new[] { firstElement }.Concat(theOtherElements).AsImmutable()
                : from elements in Optional(Repeat(parser, separatorParser, atLeastOnce: true))
                  select elements.OrElse(Array.Empty<TValue>());

        public static Parser<bool> ForwardDeclaration<T>(Parser<T>? unsued) =>
            input => unsued == null 
                ? Right(Result(true, input)) 
                : throw ProgramLogic.Error("call to ForwardDeclaration() seems to be unnecessary in this spot.");

        public static Result<TValue> Result<TValue>(TValue value, TextInput rest) =>
            new (value, rest);
    }
}