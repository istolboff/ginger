using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Prolog.Engine.Miscellaneous;

namespace Prolog.Engine.Parsing
{
    using static Either;
    using static MonadicParsing;
    
    internal static class TextParsingPrimitives
    {
        public static Parser<string> SkipWhitespaces =>
            Repeat(Expect(char.IsWhiteSpace)).Select(chars => string.Join(string.Empty, chars));

        public static Parser<bool> Eof =>
            input => (input.Position == input.Text.Length) switch 
                { 
                    true => Right(Result(true, input)),
                    false => Left(new ParsingError("expected to be at the end of input", input))
                };

        public static Parser<bool> Eol =>
            Repeat(Expect(ch => char.IsWhiteSpace(ch) && !Environment.NewLine.Contains(ch)))
                .Then<IReadOnlyCollection<char>, bool>(
                    input => (input.Position < input.Text.Length && 
                            string.Compare(
                                input.Text, 
                                input.Position, 
                                Environment.NewLine, 
                                0, 
                                Environment.NewLine.Length, 
                                false, 
                                CultureInfo.InvariantCulture) == 0)
                        switch 
                        { 
                            true => Right(Result(true, input.Skip(Environment.NewLine.Length))),
                            false => Left(new ParsingError("expected to be at the end of line", input))
                        });


        public static Parser<char> Expect(Func<char, bool> checkChar, bool ignoreCommentCharacter = false) =>
            from ch in ignoreCommentCharacter ? new Parser<char>(Read) : ReadSkippingComment
            where checkChar(ch)
            select ch;

        public static Parser<char> Expect(char ch) =>
            Expect(c => c == ch);

        public static Parser<string> Lexem(Func<char, bool> firstChar, Func<char, bool> allTheOtherChars) =>
            from firstOne in SkipWhitespaces.Then(Expect(firstChar))
            from theOtherOnes in Repeat(Expect(allTheOtherChars))
            select firstOne + string.Join(string.Empty, theOtherOnes);

        public static Parser<string> Lexem(params string[] lexems) =>
            SkipWhitespaces.Then(
                input =>
                {
                    var matchingCharacters = string.Empty;
                    while (true)
                    {
                        var nextChar = Read(input);
                        if (nextChar.IsLeft)
                        {
                            return MakeResult(matchingCharacters);
                        }

                        var newMatchingCharacters = matchingCharacters + nextChar.Right!.Value.ToString();
                        if (lexems.All(l => !l.StartsWith(newMatchingCharacters, StringComparison.Ordinal)))
                        {
                            return MakeResult(matchingCharacters);
                        }

                        matchingCharacters = newMatchingCharacters;
                        input = nextChar.Right!.Rest;

                        Either<ParsingError, Result<string>> MakeResult(string s) =>
                            lexems.Any(l => string.Equals(l, s, StringComparison.Ordinal))
                                ? Right<ParsingError, Result<string>>(Result(s, input))
                                : Left(new ParsingError($"Expected one of {string.Join(",", lexems)}", input));
                    }
                });

        public static Parser<string> ReadTill(string lexem) =>
            SkipWhitespaces.Then<string, string>(
                input => input.Text.IndexOf(lexem, input.Position, StringComparison.Ordinal) switch
                {
                    var foundOffset when foundOffset >= 0 => 
                        Right(
                            Result(
                                input.Text.Substring(input.Position, foundOffset - input.Position), 
                                input.MoveTo(foundOffset + lexem.Length))),
                    _ => Left(
                            new ParsingError(
                                $"Could not locate any further occurencies of '{lexem}'", 
                                input))
                });

        public static InvalidOperationException ParsingError(string message) => new (message);

        private static Either<ParsingError, Result<char>> Read(TextInput input) =>
            (input.Position < input.Text.Length) switch
            {
                true => Right(Result(input.Text[input.Position], input.Skip(1))),
                false => Left(new ParsingError($"Attempt to read past the end of the input '{input.Text}'.", input))
            };

        private static Either<ParsingError, Result<char>> ReadSkippingComment(TextInput input) =>
            (input.Position >= input.Text.Length) switch
            {
                true => Left(new ParsingError($"Attempt to read past the end of the input '{input.Text}'.", input)),
                false when input.Text[input.Position] != '%' => Right(Result(input.Text[input.Position], input.Skip(1))),
                _ => Right(Result(' ', input.SkipToEndOfLine()))
            };
    }
}