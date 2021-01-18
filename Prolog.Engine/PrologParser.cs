using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;

using static Prolog.Engine.Builtin;
using static Prolog.Engine.DomainApi;
using static Prolog.Engine.Either;
using static Prolog.Engine.MayBe;
using static Prolog.Engine.TextParsingPrimitives;
using static Prolog.Engine.MonadicParsing;

namespace Prolog.Engine
{
    public static class PrologParser
    {
        public static IReadOnlyCollection<Rule> ParseProgram(string input) =>
            TryParse(PrologParsers.ProgramParser, input, "Failed to parse Prolog program");

        public static IReadOnlyCollection<IReadOnlyCollection<ComplexTerm>> ParseQuery(string input) =>
            TryParse(PrologParsers.QueryParser, input, "Failed to parse Prolog queries");

        public static Term ParseTerm(string input) =>
            TryParse(PrologParsers.TermParser, input, "Failed to parse Prolog term");

        internal static MayBe<Term> TryParseTerm(string input) =>
            TryParseCore(PrologParsers.TermParser, input, string.Empty)
            .Fold(_ => MakeNone<Term>(), result => Some(result));

        public static event Action<string>? ParsingEvent;

        private static T TryParse<T>(Parser<T> parser, string input, string errorMessage) =>
            TryParseCore(parser, input, errorMessage)
            .Fold(error => throw error, result => result);

        private static Either<Exception, T> TryParseCore<T>(Parser<T> parser, string input, string errorMessage) =>
            (from result in parser
             from unsused in SkipWhitespaces.Then(Eof)
             select result)(new TextInput(input, 0))
             .Fold(
                parsingError => Left<Exception, T>(ParsingError($"{errorMessage} [{input}] {parsingError.Text} at {parsingError.Location.Position}")),
                result => Right<Exception, T>(result.Value));

        private static (
            Parser<IReadOnlyCollection<Rule>> ProgramParser, 
            Parser<IReadOnlyCollection<IReadOnlyCollection<ComplexTerm>>> QueryParser,
            Parser<Term> TermParser) BuildParsers()
        {
            var tracer = new ParsingTracer(text => ParsingEvent?.Invoke(text));

            var comma = tracer.Trace(
                        Lexem(","), 
                        "comma");

            var unquotedAtom = tracer.Trace(
                        from atomText in Lexem(
                            ch => char.IsLetter(ch) && char.IsLower(ch), 
                            ch => char.IsLetterOrDigit(ch) || ch == '_')
                        select Atom(atomText),
                        "unquotedAtom");

            var quotedAtom = tracer.Trace(
                        from unused1 in SkipWhitespaces.Then(Expect('\''))
                        from letters in Repeat(Expect(ch => ch != '\'', ignoreCommentCharacter: true))
                        from unused2 in Expect('\'')
                        select Atom(string.Join(string.Empty, letters)),
                        "quotedAtom");

            var atom = tracer.Trace(
                        Or(quotedAtom, unquotedAtom), 
                        "atom");

            var number = tracer.Trace(
                        from sign in Optional(Lexem("+", "-"))
                        let multiplier = sign.Map(s => s == "-" ? -1 : 1).OrElse(1)
                        from digits in SkipWhitespaces.Then(Repeat(Expect(char.IsDigit), atLeastOnce: true))
                        let v = string.Join(string.Empty, digits)
                        select Number(multiplier * int.Parse(v, CultureInfo.InvariantCulture)),
                        "number");

            var variable = tracer.Trace(
                        from name in Lexem(
                            ch => ch == '_' || (char.IsLetter(ch) && char.IsUpper(ch)),
                            ch => char.IsLetterOrDigit(ch) || ch == '_')
                        select name == "_" ? Builtin._ : Variable(name),
                        "variable");

            Parser<Term>? term = null;
            var complexTerm = tracer.Trace(
                        from functorName in SkipWhitespaces.Then(unquotedAtom)
                        from unused1 in Lexem("(")
// ReSharper disable once AccessToModifiedClosure
                        from arguments in Repeat(term!, separatorParser: comma, atLeastOnce: true)
                        from unused2 in Lexem(")")
                        select ComplexTerm(Functor(functorName.Characters, arguments.Count), arguments),
                        "complexTerm");

            var explicitList = tracer.Trace(
                        from delayUsage in ForwardDeclaration(term)
// ReSharper disable once AccessToModifiedClosure
                        from elements in Repeat(term!, separatorParser: comma)
                        select List(elements.Reverse()),
                        "explicitList");

            var barList = tracer.Trace(
                        from delayUsage in ForwardDeclaration(term)
// ReSharper disable once AccessToModifiedClosure
                        from head in term!
                        from unused in Lexem("|")
// ReSharper disable once AccessToModifiedClosure
                        from tail in term!
                        select Dot(head, tail),
                        "barList");

            var list = tracer.Trace(
                        from unused in Lexem("[")
                        from elements in Or(barList, explicitList)
                        from unused1 in Lexem("]")
                        select elements,
                        "list");

            term = tracer.Trace(
                Or(AsTerm(list), AsTerm(complexTerm), AsTerm(variable), AsTerm(number), AsTerm(atom)),
                "term");

            var infixExpression = tracer.Trace(
                        from leftPart in term!
                        from @operator in Lexem(Builtin.BinaryOperators.Keys.ToArray())
                        let complexTermFactory = Builtin.BinaryOperators[@operator]
                        from rightPart in term!
                        select complexTermFactory(leftPart, rightPart),
                        "infixExpression");

            var cut = tracer.Trace(
                        from unused in Lexem("!")
                        select Cut,
                        "cut");

            var fail = tracer.Trace(
                        from unused in Lexem("fail")
                        select Fail,
                        "fail");

            var premise = tracer.Trace(
                        Or(
                            cut, 
                            fail, 
                            infixExpression, 
                            complexTerm.Select(ct => Builtin.TryResolveFunctor(ct) ?? ct),
                            variable.Select(Call)),
                        "premise");

            var fact = tracer.Trace(
                        from conclusion in complexTerm
                        from unused in Lexem(".")
                        select Fact(conclusion),
                        "fact");

            var premisesGroup = tracer.Trace(
                        Repeat(premise, separatorParser: comma, atLeastOnce: true),
                        "premisesGroup");

            var premisesAlternatives = tracer.Trace(
                        Repeat(premisesGroup, separatorParser: Lexem(";"), atLeastOnce: true),
                        "premisesAlternatives");

            var rule = tracer.Trace(
                        from conclusion in complexTerm
                        from unused in Lexem(":-")
                        from premisesAlternative in premisesAlternatives
                        from unused1 in Lexem(".")
                        select premisesAlternative
                                .Select(premises => Rule(conclusion, premises))
                                .ToArray(),
                        "rule");

            var program = tracer.Trace(
                from ruleGroup in Repeat(Or(rule, fact.Select(f => new[] { f })))
                select ruleGroup.SelectMany(r => r).AsImmutable(),
                "program");

            return (program, premisesAlternatives, term);

            Parser<Term> AsTerm<T>(Parser<T> parser) where T : Term =>
                from v in parser
                select v as Term;
        }

        internal static readonly (
            Parser<IReadOnlyCollection<Rule>> ProgramParser, 
            Parser<IReadOnlyCollection<IReadOnlyCollection<ComplexTerm>>> QueryParser,
            Parser<Term> TermParser) PrologParsers = BuildParsers();
    }
}