using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Prolog.Engine;
using Prolog.Engine.Miscellaneous;
using Prolog.Engine.Parsing;
using Ginger.Runner.Solarix;

namespace Ginger.Runner
{
    using static MayBe;
    using static MakeCompilerHappy;
    using static MonadicParsing;
    using static PrologParser;
    using static TextParsingPrimitives;

    using WordOrQuotation = Either<Word, Quotation>;

    internal sealed class SentenceUnderstander
    {
        public SentenceUnderstander(
            IEnumerable<Func<WordOrQuotation, MayBe<UnderstoodSentence>>> phraseTypeUnderstanders)
        {
            _phraseTypeUnderstanders = phraseTypeUnderstanders.AsImmutable();
        }

        public MayBe<UnderstoodSentence> Understand(WordOrQuotation sentence) =>
            _phraseTypeUnderstanders
                .Select(phraseTypeUnderstander => phraseTypeUnderstander(sentence))
                .TryFirst(result => result.HasValue)
                .OrElse(None);

        public static SentenceUnderstander LoadFromEmbeddedResources(IRussianGrammarParser grammarParser) =>
            new (ParsePatterns(ReadEmbeddedResource("Ginger.Runner.SentenceUnderstandingRules.txt"), grammarParser)
                 .Select(parsedPattern => PatternBuilder.BuildPattern(
                    parsedPattern.PatternId, 
                    parsedPattern.Pattern, 
                    parsedPattern.Meaning)));

        private static TextInput ReadEmbeddedResource(string name)
        {
            var stream = Assembly
                            .GetExecutingAssembly()!
                            .GetManifestResourceStream(name);
            using var reader = new StreamReader(SuppressCa1062(stream));
            return new (reader.ReadToEnd(), 0);
        }

        private static IEnumerable<ParsedPattern> ParsePatterns(
            TextInput rulesText, 
            IRussianGrammarParser grammarParser)
        {
            var tracer = new ParsingTracer(text => Log(text));

            var patternId = tracer.Trace(
                from unused in Lexem("pattern-")
                from id in Repeat(Expect(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '-'))
                from unused1 in Lexem(":").Then(Eol)
                select string.Join(string.Empty, id),
                "patternId");

            var singlePattern = tracer.Trace(
                from id in patternId
                from pattern in ReadTill("::=")
                from meaning in PrologParsers.ProgramParser
                select new ParsedPattern(id, grammarParser.ParseAnnotated(pattern), meaning),
                "singlePattern");

            var patterns = tracer.Trace(Repeat(singlePattern), "patterns");

            return patterns(rulesText).Fold(
                        parsingError => throw ParsingError($"{parsingError.Text} at {parsingError.Location.Position}"),
                        result => result.Value);
        }

        [Conditional("UseLogging")]
        private static void Log(string text) =>
            Trace.WriteLine(text);

        private readonly IReadOnlyCollection<Func<WordOrQuotation, MayBe<UnderstoodSentence>>> _phraseTypeUnderstanders;

        private record ParsedPattern(string PatternId, AnnotatedSentence Pattern, IReadOnlyCollection<Rule> Meaning);
    }
}