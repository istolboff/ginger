using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Prolog.Engine.Miscellaneous;
using Ginger.Runner.Solarix;


namespace Ginger.Tests
{
    using static MayBe;
    using static Impl;

    internal sealed class SolarixParserMemoizer : IRussianGrammarParser
    {
        public SolarixParserMemoizer(IRussianGrammarParser wrappedParser)
        {
            _wrappedParser = wrappedParser;
            EnsureDataFileExists();
            _knownSentences = LoadKnownSentences();
        }

        public IReadOnlyCollection<SentenceElement> Parse(string text)
        {
            var normalizedText = Normalize(text);
            return TryToRecallResult(normalizedText)
                .OrElse(() => 
                {
                    var result = _wrappedParser.Parse(normalizedText);
                    Memoize(normalizedText, result);
                    return result;
                });
        }

        public void Dispose()
        {
            _wrappedParser.Dispose();
        }

        private MayBe<IReadOnlyCollection<SentenceElement>> TryToRecallResult(string text)
        {
            return _knownSentences.TryGetValue(text, out var sentenceElements) 
                        ? Some(sentenceElements.Value) 
                        : None;
        }

        private void Memoize(string text, IReadOnlyCollection<SentenceElement> result)
        {
            _knownSentences.Add(text, new Lazy<IReadOnlyCollection<SentenceElement>>(() => result));
            File.AppendAllLines(DataFilePath, new[] { text, Serialize(result) }, Encoding.UTF8);
        }

        private static string Normalize(string text) =>
            SpaceRemover.Replace(text.Trim().Replace(Environment.NewLine, " "), " ");

        private static string DataFilePath => 
            Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, 
                "SolarixRussianGrammarParser.memoized");

        private static void EnsureDataFileExists()
        {
            if (!File.Exists(DataFilePath))
            {
                File.Create(DataFilePath).Dispose();
            }
        }

        private static IDictionary<string, Lazy<IReadOnlyCollection<SentenceElement>>> LoadKnownSentences()
        {
            return File
                .ReadLines(DataFilePath, Encoding.UTF8)
                .Partition((text, serializedData) => new { text, serializedData })
                .ToDictionary(
                    item => item.text, 
                    item => new Lazy<IReadOnlyCollection<SentenceElement>>(() => Deserialize(item.serializedData)),
                    RussianIgnoreCase);
        }

        private static string Serialize(IReadOnlyCollection<SentenceElement> sentenceElements)
        {
            return JsonConvert.SerializeObject(
                sentenceElements, 
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        }

        private static IReadOnlyCollection<SentenceElement> Deserialize(string serializedData)
        {
            return JsonConvert.DeserializeObject<IReadOnlyCollection<SentenceElement>>(
                serializedData,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All })!;
        }

        private readonly IRussianGrammarParser _wrappedParser;
        private readonly IDictionary<string, Lazy<IReadOnlyCollection<SentenceElement>>> _knownSentences;

        private static readonly Regex SpaceRemover = new (@"\s+", RegexOptions.Compiled);
    }
}