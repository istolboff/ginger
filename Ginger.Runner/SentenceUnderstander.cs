using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
    using ConcreteUnderstander = Func<Either<Word, Quotation>, MayBe<UnderstoodSentence>>;

    internal sealed class SentenceUnderstander
    {
        public SentenceUnderstander(
            IEnumerable<ConcreteUnderstander> phraseTypeUnderstanders)
        {
            _phraseTypeUnderstanders = phraseTypeUnderstanders.AsImmutable();
        }

        public MayBe<UnderstoodSentence> Understand(WordOrQuotation sentence) =>
            _phraseTypeUnderstanders
                .Select(phraseTypeUnderstander => phraseTypeUnderstander(sentence))
                .TryFirst(result => result.HasValue)
                .OrElse(None);

        public static SentenceUnderstander LoadFromEmbeddedResources(
            IRussianGrammarParser grammarParser,
            IRussianLexicon russianLexicon) 
        =>
            new (from generativePattern in ParsePatterns(ReadEmbeddedResource("Ginger.Runner.SentenceUnderstandingRules.txt"))
                 from concreteUnderstander in generativePattern.GenerateConcreteUnderstanders(grammarParser, russianLexicon)
                 select concreteUnderstander);

        private static TextInput ReadEmbeddedResource(string name)
        {
            var stream = Assembly
                            .GetExecutingAssembly()!
                            .GetManifestResourceStream(name);
            using var reader = new StreamReader(SuppressCa1062(stream));
            return new (reader.ReadToEnd(), 0);
        }

        private static IEnumerable<GenerativePattern> ParsePatterns(TextInput rulesText)
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
                from generativePattern in ReadTill("::=")
                from generativeMeaning in PrologParsers.ProgramParser
                select new GenerativePattern(id, generativePattern, generativeMeaning),
                "singlePattern");

            var patterns = tracer.Trace(Repeat(singlePattern), "patterns");

            return patterns(rulesText).Fold(
                        parsingError => throw ParsingError($"{parsingError.Text} at {parsingError.Location.Position}"),
                        result => result.Value);
        }

        [Conditional("UseLogging")]
        private static void Log(string text) =>
            Trace.WriteLine(text);

        private readonly IReadOnlyCollection<ConcreteUnderstander> _phraseTypeUnderstanders;
    }
}