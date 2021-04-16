using System;
using System.Collections.Generic;
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

    using ConcreteUnderstander = Func<ParsedSentence, MayBe<UnderstoodSentence>>;

    internal sealed class SentenceUnderstander
    {
        private SentenceUnderstander(IReadOnlyCollection<ConcreteUnderstander> phraseTypeUnderstanders) =>
            _phraseTypeUnderstanders = phraseTypeUnderstanders;

        public MayBe<UnderstoodSentence> Understand(ParsedSentence sentence) =>
            Understand(sentence, _phraseTypeUnderstanders);

        public static SentenceUnderstander LoadFromEmbeddedResources(
            IRussianGrammarParser grammarParser,
            IRussianLexicon russianLexicon) 
        =>
            LoadFromPatterns(
                ParsePatterns(ReadEmbeddedResource("Ginger.Runner.SentenceUnderstandingRules.txt")),
                grammarParser,
                russianLexicon);

        public static SentenceUnderstander LoadFromPatterns(
            IEnumerable<GenerativePattern> generativePatterns,
            IRussianGrammarParser grammarParser,
            IRussianLexicon russianLexicon) 
        {
            var concreteUnderstanders = new List<ConcreteUnderstander>();
            var result = new SentenceUnderstander(concreteUnderstanders);

            foreach (var generativePattern in generativePatterns)
            {
                var concretePatterns = generativePattern
                                        .GenerateConcretePatterns(grammarParser, russianLexicon)
                                        .Select(concretePattern => 
                                            new 
                                            { 
                                                concretePattern,
                                                annotatedSentence = grammarParser.ParseAnnotated(
                                                                        DisambiguatedPattern.Create(
                                                                            concretePattern.Pattern, 
                                                                            russianLexicon))
                                            })
                                        .AsImmutable();
                
                var ambiguouslyUnderstoodPatterns = (
                    from it in concretePatterns
                    let understanding = Understand(
                                            new(it.annotatedSentence.Text, it.annotatedSentence.Sentence),
                                            concreteUnderstanders)
                    where understanding.HasValue
                    select new
                    { 
                        it.concretePattern.Pattern, 
                        NewPatternId = generativePattern.PatternId, 
                        ExistingPatternId = understanding.Value!.PatternId 
                    }
                ).AsImmutable();

                if (ambiguouslyUnderstoodPatterns.Any())
                {
                    throw new InvalidOperationException(
                        "The following understanding pattern(s) introduce ambiguity: " + Environment.NewLine +
                        string.Join(
                            Environment.NewLine, 
                            ambiguouslyUnderstoodPatterns.Select(
                                aup => $"   text '{aup.Pattern}' from pattern '{aup.NewPatternId}' " + 
                                       $"was recognized by the pattern '{aup.ExistingPatternId}'")));
                }

                concreteUnderstanders.AddRange(concretePatterns
                    .Select(it => PatternBuilder.BuildPattern(
                                    generativePattern.PatternId,
                                    it.annotatedSentence,
                                    it.concretePattern.Meaning,
                                    grammarParser,
                                    russianLexicon,
                                    result)));
            }

            return result;
        }

        private static MayBe<UnderstoodSentence> Understand(
            ParsedSentence sentence, 
            IEnumerable<ConcreteUnderstander> phraseTypeUnderstanders) 
        =>
            phraseTypeUnderstanders
                .Select(phraseTypeUnderstander => phraseTypeUnderstander(sentence))
                .TryFirst(result => result.HasValue)
                .OrElse(None)
                .Map(understoodSentence => 
                    understoodSentence with 
                    { 
                        Meaning = MeaningMetaModifiers.ApplyTo(understoodSentence.Meaning) 
                    });

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
            var patternId = Tracer.Trace(
                from unused in Lexem("pattern-")
                from id in Repeat(Expect(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '-'))
                from unused1 in Lexem(":").Then(Eol)
                select string.Join(string.Empty, id),
                "patternId");

            var meaning = Tracer.Trace(
                Either(PrologParsers.ProgramParser, PrologParsers.PremisesGroupParser),
                "meaning");

            var singlePattern = Tracer.Trace(
                from id in patternId
                from generativePattern in ReadTill("::=")
                from generativeMeaning in meaning
                select new GenerativePattern(id, generativePattern, generativeMeaning),
                "singlePattern");

            var patterns = Tracer.Trace(Repeat(singlePattern), "patterns");

            return patterns(rulesText).Fold(
                        parsingError => throw ParsingError($"{parsingError.Text} at {parsingError.Location.Position}"),
                        result => result.Value);
        }

        private readonly IReadOnlyCollection<ConcreteUnderstander> _phraseTypeUnderstanders;
    }
}