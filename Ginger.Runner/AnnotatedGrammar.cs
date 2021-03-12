using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ginger.Runner.Solarix;
using Prolog.Engine.Miscellaneous;

namespace Ginger.Runner
{
    using WordOrQuotation = Either<Word, Quotation>;

    internal sealed record AnnotatedSentence(
        WordOrQuotation Sentence,
        IReadOnlyCollection<string> AnnotatedWords,
        IReadOnlyCollection<LemmaVersion> AnnotatedWordLemmas)
    {
        public bool WordIsAnnotated(string word, LemmaVersion lemmaVersion) =>
            AnnotatedWords.Contains(word) ||
            AnnotatedWordLemmas.Any(lm => lm.Lemma == lemmaVersion.Lemma && 
                                lm.PartOfSpeech == lemmaVersion.PartOfSpeech &&
                                lm.Characteristics.CompatibleTo(lemmaVersion.Characteristics));
    }

    internal static class AnnotatedGrammar
    {
        public static AnnotatedSentence ParseAnnotated(this IRussianGrammarParser @this, string text)
        {
            var sentence = @this.ParsePreservingQuotes(RemoveAnnotations(text)).Single();
            var annotatedWords = GetAnnotatedWords(text);
            var andAllTheirLemmas = GetAllWordLemmas(sentence, annotatedWords).AsImmutable();
            return new (sentence, annotatedWords, andAllTheirLemmas.ToHashSet());
        }

        private static IReadOnlyCollection<string> GetAnnotatedWords(string text) =>
            AnnotatedWordsMatcher
                .Matches(text)
                .SelectMany(m => m.Groups[1].Value.Split(' ', ',', ';', '-', ':'))
                .AsImmutable();

        private static string RemoveAnnotations(string text) =>
            AnnotatedWordsMatcher.Replace(text, m => m.Groups[1].Value);

        private static IEnumerable<LemmaVersion> GetAllWordLemmas(
            WordOrQuotation wordOrQuotation, 
            IReadOnlyCollection<string> annotatedWords) 
        =>
            wordOrQuotation.Fold(
                word => 
                    annotatedWords.Contains(word.Content) 
                        ? word.LemmaVersions
                            .Concat(word.Children.SelectMany(child => GetAllWordLemmas(child, annotatedWords)))
                        : word.Children.SelectMany(child => GetAllWordLemmas(child, annotatedWords)),
                _ => Enumerable.Empty<LemmaVersion>());

        private static readonly Regex AnnotatedWordsMatcher = new (@"~([^~]+)~", RegexOptions.Compiled);
    }
}