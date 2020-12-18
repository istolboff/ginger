using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;

using static Prolog.Engine.Builtin;

namespace Prolog.Engine
{
    public static class Proof
    {
#pragma warning disable CA1707
        public static readonly Variable _ = new Variable("_");
#pragma warning restore CA1707

        public static IEnumerable<UnificationResult> Find(Rule[] programRules, params ComplexTerm[] queries)
        {
            SetupTracing();
            var queryVariableNames = ListAllMentionedVariableNames(queries);
            return FindCore(
                        programRules, 
                        ImmutableList.Create<ComplexTerm>(queries),
                        ImmutableHashSet.CreateRange(queryVariableNames),
                        ImmutableDictionary.Create<Variable, Term>(),
                        useCutMode: false)
                    .Select(result => ResolveInternalInstantiations(result, queryVariableNames)
                            .Trace("yield Proof"));
        }

        private static IEnumerable<UnificationResult> FindCore(
                Rule[] programRules, 
                ImmutableList<ComplexTerm> queries,
                ImmutableHashSet<string> mentionedVariableNames,
                ImmutableDictionary<Variable, Term> variableInstantiations,
                bool useCutMode)
        {
            if (!queries.Any())
            {
                yield return new UnificationResult(
                    Succeeded: true, 
                    Instantiations: new StructuralEquatableDictionary<Variable, Term>(variableInstantiations));
            }
            else
            {
                var currentQuery = queries.First().Trace("currentQuery");
                if (currentQuery == Cut)
                {
                    foreach (var solution in FindCore(
                            programRules, 
                            queries.RemoveAt(0),
                            mentionedVariableNames,
                            variableInstantiations,
                            useCutMode: false))
                    {
                        yield return solution;
                    }                    
                }
                else
                {
                    foreach (var matchingRule in FindMatchingRules(programRules, currentQuery).Take(useCutMode ? 1 : int.MaxValue).Trace("matching rules"))
                    {
                        var matchingRuleContainsCut = matchingRule.Premises.Contains(Cut);
                        var ruleWithRenamedVariables = RenameRuleVariablesToMakeThemDifferentFromAlreadyUsedNames(matchingRule, mentionedVariableNames).Trace("ruleWithRenamedVariables");
                        var ruleConclusionUnificationResult = Unification.CarryOut(currentQuery, ruleWithRenamedVariables.Conclusion).Trace("ruleConclusionUnificationResult");
                        var updatedQueries = queries.RemoveAt(0).InsertRange(0, ruleWithRenamedVariables.Premises).Trace("updatedQueries");
                        var updatedQueriesWithSubstitutedVariables = updatedQueries.Select(p => ApplyVariableInstantiations(p, ruleConclusionUnificationResult.Instantiations)).Trace("updatedQueriesWithSubstitutedVariables");
                        foreach (var solution in FindCore(
                            programRules, 
                            ImmutableList.CreateRange<ComplexTerm>(updatedQueriesWithSubstitutedVariables),
                            mentionedVariableNames.Union(ListAllMentionedVariableNames(ruleWithRenamedVariables.Premises)),
                            variableInstantiations.AddRange(ruleConclusionUnificationResult.Instantiations),
                            useCutMode: useCutMode || matchingRuleContainsCut))
                        {
                            yield return solution;
                        }

                        if (matchingRuleContainsCut)
                        {
                            break;
                        }
                    }
                }
            }

            static IEnumerable<Rule> FindMatchingRules(Rule[] programRules, ComplexTerm query) =>
                query.Functor switch 
                {
                    Functor functor => programRules.Where(rule => Unification.IsPossible(rule.Conclusion, query)),
                    BuiltinFunctor builtinFunctor => builtinFunctor.Invoke(query.Arguments) ? new[] { new Rule(new ComplexTerm(new Functor("True", 0), new StructuralEquatableArray<Term>()), new StructuralEquatableArray<ComplexTerm>()) } : Enumerable.Empty<Rule>(),
                    _ => throw new InvalidOperationException($"Do not know how to handle {query.Functor} functor.")
                };
                

            static Rule RenameRuleVariablesToMakeThemDifferentFromAlreadyUsedNames(Rule rule, IReadOnlySet<string> usedNames)
            {
                var renamedVariables = new Dictionary<string, Variable>();
                return new Rule(
                    Conclusion: RenameComplexTermVariablesToMakeThemDifferentFromAlreadyUsedNames(rule.Conclusion, usedNames, renamedVariables),
                    Premises: new StructuralEquatableArray<ComplexTerm>(
                        rule.Premises.Select(
                            ct => RenameComplexTermVariablesToMakeThemDifferentFromAlreadyUsedNames(ct, usedNames, renamedVariables))));
            }

            static ComplexTerm RenameComplexTermVariablesToMakeThemDifferentFromAlreadyUsedNames(
                ComplexTerm complexTerm, 
                IReadOnlySet<string> usedNames,
                IDictionary<string, Variable> renamedVariables) =>
                new ComplexTerm(
                    complexTerm.Functor, 
                    new StructuralEquatableArray<Term>(
                        complexTerm.Arguments.Select(
                            argument => argument switch
                            {
                                Variable v when v == _ => GenerateNewVariable(),
                                Variable v when !usedNames.Contains(v.Name) => v,
                                Variable v => renamedVariables.TryGetValue(v.Name, out var renamedVariable)
                                        ? renamedVariable
                                        : AddElement(renamedVariables, v.Name, GenerateNewVariable()),
                                ComplexTerm ct => RenameComplexTermVariablesToMakeThemDifferentFromAlreadyUsedNames(ct, usedNames, renamedVariables),
                                _ => argument
                            }
                        )));

            static Variable AddElement(IDictionary<string, Variable> dictionary, string name, Variable variable)
            {
                dictionary.Add(name, variable);
                return variable;
            }
        }

        private static IReadOnlySet<string> ListAllMentionedVariableNames(IEnumerable<ComplexTerm> queries) =>
            new HashSet<string>(queries.SelectMany(q => ListAllMentionedVariableNames(q)));

        private static IEnumerable<string> ListAllMentionedVariableNames(ComplexTerm complexTerm) =>
            complexTerm.Arguments.SelectMany(
                argument => argument switch 
                {
                    Variable v => v == _ ? Array.Empty<string>() : new[] { v.Name },
                    ComplexTerm ct => ListAllMentionedVariableNames(ct),
                    _ => Enumerable.Empty<string>()
                });

        private static ComplexTerm ApplyVariableInstantiations(
            ComplexTerm complexTerm, 
            IReadOnlyDictionary<Variable, Term> instantiations,
            bool substituteUninstantiatedVariablesWith_ = false) =>
            new ComplexTerm(
                complexTerm.Functor,
                new StructuralEquatableArray<Term>(
                    complexTerm.Arguments.Select(argument => ApplyVariableInstantiationsCore(argument, instantiations, substituteUninstantiatedVariablesWith_))));

        private static Term ApplyVariableInstantiationsCore(
            Term term, 
            IReadOnlyDictionary<Variable, Term> instantiations, 
            bool substituteUninstantiatedVariablesWith_ = false) =>
            term switch
            {
                Variable variable when substituteUninstantiatedVariablesWith_ && !instantiations.ContainsKey(variable) => _,
                Variable variable => !instantiations.TryGetValue(variable, out var value) 
                    ? term 
                    : ApplyVariableInstantiationsCore(value, instantiations, substituteUninstantiatedVariablesWith_),
                ComplexTerm complexTerm => ApplyVariableInstantiations(complexTerm, instantiations, substituteUninstantiatedVariablesWith_),
                _ => term
            };

        private static UnificationResult ResolveInternalInstantiations(UnificationResult result, IReadOnlySet<string> variableNames) =>
            (result with 
            { 
                Instantiations = new StructuralEquatableDictionary<Variable, Term>(
                    result.Instantiations
                        .Where(it => variableNames.Contains(it.Key.Name))
                        .Select(it => KeyValuePair.Create(
                                        it.Key, 
                                        ApplyVariableInstantiationsCore(
                                            it.Value, 
                                            result.Instantiations, 
                                            substituteUninstantiatedVariablesWith_: true)))
                )
            });

        private static Variable GenerateNewVariable() =>
            new Variable(Name: $"_{++NextNewVariableIndex}", IsTemporary: true);

        private static string TraceFilePath => Path.Combine(Path.GetTempPath(), "Prolog.trace");

        private static void SetupTracing() => 
            File.Delete(TraceFilePath);

        private static T Trace<T>(this T @this, string? description = null)
        {
            if (description != null)
            {
                File.AppendAllText(TraceFilePath, $"{description}: ");
            }

            File.AppendAllText(TraceFilePath, Dump(@this));
            File.AppendAllLines(TraceFilePath, new[] { string.Empty });

            return @this;

            static string Dump<Q>(Q @this) =>
                @this switch
                {
                    Atom atom => atom.Characters,
                    Number number => number.Value.ToString(CultureInfo.InvariantCulture),
                    Variable variable => variable.Name,
                    ComplexTerm complexTerm => $"{complexTerm.Functor.Name}({string.Join(',', complexTerm.Arguments.Select(Dump))})",
                    Rule rule => $"{Dump(rule.Conclusion)}:-{string.Join(',', rule.Premises.Select(Dump))}",
                    UnificationResult unificationResult => string.Join(" & ",unificationResult.Instantiations.Select(i => $"{Dump(i.Key)} = {Dump(i.Value)}")),
                    string text => text,
                    IEnumerable collection => string.Join("; ", collection.Cast<object>().Select(Dump)),
                    _ => @this?.ToString() ?? "NULL"
                };
        }

        private static int NextNewVariableIndex;
    }
}