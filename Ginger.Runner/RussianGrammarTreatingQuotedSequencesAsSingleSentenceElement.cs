using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ginger.Runner.Solarix;
using Prolog.Engine;

using static Prolog.Engine.Either;

namespace Ginger.Runner
{
    using WordOrQuotation = Prolog.Engine.Either<Word, Quotation>;

    internal sealed record Quotation(string Content, IReadOnlyList<WordOrQuotation> Children);

    internal sealed record Word(string Content, IReadOnlyCollection<LemmaVersion> LemmaVersions, IReadOnlyList<WordOrQuotation> Children, LinkType? LeafLinkType);
    
    internal static class RussianGrammarTreatingQuotedSequencesAsSingleSentenceElement
    {
        public static IReadOnlyCollection<WordOrQuotation> ParsePreservingQuotes(
            this IRussianGrammarParser @this, 
            string text)
        {
            var quotedSequences = LocateQuotedSequences(text);
            var quotationSubstitutions = MakeSubstitutionsOfQuotedSequencesWithSingleWords(quotedSequences);
            var textWithSubstitutedQuotes = ApplySubstitutions(quotationSubstitutions, text);
            var parsedText = @this.Parse(textWithSubstitutedQuotes);
            return RestoreQuotations(quotationSubstitutions, parsedText);
        }

        private static IEnumerable<string> LocateQuotedSequences(string text) =>
            QuotationRecognizer.Matches(text).Select(m => m.Groups[1].Value).AsImmutable();

        private static IReadOnlyDictionary<string, string> MakeSubstitutionsOfQuotedSequencesWithSingleWords(
            IEnumerable<string> quotedSequences) =>
            quotedSequences
                .Select((quotation, index) => (quotation, index))
                .ToDictionary(
                    it => it.index < StockWordsForQuotationSubstitution.Length 
                        ? StockWordsForQuotationSubstitution[it.index]
                        : throw new NotSupportedException($"Sentences with more than {StockWordsForQuotationSubstitution.Length} quotations are not supported."),
                    it => it.quotation);

        private static string ApplySubstitutions(
            IReadOnlyDictionary<string, string> quotationSubstitutions,
            string text)
        {
            var result = text;
            foreach (var (toWhat, fromWhat) in quotationSubstitutions)
            {
                result = result.Replace($"'{fromWhat}'", toWhat);
            }

            return result;
        }

        private static IReadOnlyList<WordOrQuotation> RestoreQuotations(
            IReadOnlyDictionary<string, string> quotationSubstitutions, 
            IEnumerable<SentenceElement> parsedText) 
        =>
            (from sentenceElement in parsedText
            let children = RestoreQuotations(quotationSubstitutions, sentenceElement.Children)
            select quotationSubstitutions
                    .TryFind(sentenceElement.Content)
                    .Fold(
                        substitution => Right<Word, Quotation>(new Quotation(substitution, children)),
                        () => Left<Word, Quotation>(
                            new Word(
                                sentenceElement.Content, 
                                sentenceElement.LemmaVersions, 
                                children, 
                                sentenceElement.LeafLinkType)))
            ).ToList();

        private static readonly Regex QuotationRecognizer = new Regex(@"'([^']+)'", RegexOptions.Compiled);

        private static readonly string[] StockWordsForQuotationSubstitution = new[] 
        {
            "Амбивалентность", "Апперцепция", "Благо", "Герменевтика", "Детерминизм",
            "Индивидуализм", "Императив", "Либерализм", "Мистицизм", "Нигилизм", "Обскурантизм",
            "Патристика", "Плюрализм", "Схоластика", "Телеология", "Фатализм"
        };
    }
}