using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using static Prolog.Engine.Builtin;

namespace Prolog.Engine
{
    public static class Proof
    {
        public static IEnumerable<UnificationResult> Find(IReadOnlyCollection<Rule> programRules, params ComplexTerm[] queries)
        {
            var queryVariableNames = ListAllMentionedVariableNames(queries);
            return FindCore(
                        Builtin.Rules.Concat(programRules).ToArray(), 
                        ImmutableList.Create<ComplexTerm>(queries),
                        ImmutableHashSet.CreateRange(queryVariableNames),
                        ImmutableDictionary.Create<Variable, Term>(),
                        useCutMode: queries.Contains(Cut),
                        nestingLevel: 0)
                    .Where(result => result.Succeeded)
                    .Select(result => ResolveInternalInstantiations(result, queryVariableNames)
                            .Trace(0, "yield Proof"));
        }

        public static event Action<string?, int, object>? ProofEvent;

        private static IEnumerable<UnificationResult> FindCore(
                IReadOnlyCollection<Rule> programRules,
                ImmutableList<ComplexTerm> queries,
                ImmutableHashSet<string> mentionedVariableNames,
                ImmutableDictionary<Variable, Term> variableInstantiations,
                bool useCutMode,
                int nestingLevel)
        {
            if (!queries.Any())
            {
                yield return new UnificationResult(
                    Succeeded: true, 
                    Instantiations: new StructuralEquatableDictionary<Variable, Term>(variableInstantiations));
            }
            else
            {
                var currentQueryRaw = queries.First().Trace(nestingLevel, "currentQueryRaw");
                if (currentQueryRaw == Fail)
                {
                    yield return Unification.Failure;
                }
                else if (currentQueryRaw == Cut)
                {
                    foreach (var solution in FindCore(
                            programRules, 
                            queries.RemoveAt(0),
                            mentionedVariableNames,
                            variableInstantiations,
                            useCutMode: false,
                            nestingLevel: nestingLevel + 1))
                    {
                        yield return solution;
                    }                    
                }
                else
                {
                    var currentQuery = currentQueryRaw.Functor == Call
                        ? UnwrapCall(currentQueryRaw, variableInstantiations) 
                        : currentQueryRaw;
                    var matchingRules = FindMatchingRules(programRules, currentQuery);
                    foreach (var matchingRule in matchingRules.Trace(nestingLevel, "matching rules"))
                    {
                        var ruleWithRenamedVariables = RenameRuleVariablesToMakeThemDifferentFromAlreadyUsedNames(matchingRule, mentionedVariableNames);
                        var ruleConclusionUnificationResult = Unification.CarryOut(currentQuery, ruleWithRenamedVariables.Conclusion).Trace(nestingLevel, "ruleConclusionUnificationResult");
                        var updatedQueries = queries.RemoveAt(0).InsertRange(0, ruleWithRenamedVariables.Premises);
                        var updatedQueriesWithSubstitutedVariables = updatedQueries.Select(p => ApplyVariableInstantiations(p, ruleConclusionUnificationResult.Instantiations)).Trace(nestingLevel, "updatedQueriesWithSubstitutedVariables");
                        var matchingRuleContainsCut = matchingRule.Premises.Contains(Cut);
                        var encounteredAtLeastOneProof = false;
                        foreach (var solution in FindCore(
                            programRules, 
                            ImmutableList.CreateRange<ComplexTerm>(updatedQueriesWithSubstitutedVariables),
                            mentionedVariableNames.Union(ListAllMentionedVariableNames(ruleWithRenamedVariables.Premises)),
                            variableInstantiations.AddRange(ruleConclusionUnificationResult.Instantiations),
                            useCutMode: useCutMode || matchingRuleContainsCut,
                            nestingLevel: nestingLevel + 1))
                        {
                            encounteredAtLeastOneProof = true;
                            yield return solution;
                        }

                        if ((useCutMode || matchingRuleContainsCut) && encounteredAtLeastOneProof)
                        {
                            break;
                        }
                    }
                }
            }

            static ComplexTerm UnwrapCall(ComplexTerm currentQueryRaw, IReadOnlyDictionary<Variable, Term> variableInstantiations) =>
                currentQueryRaw.Arguments.SingleOrDefault() switch 
                {
                    ComplexTerm complexTerm => complexTerm,
                    Variable variableToCall => variableInstantiations.TryGetValue(variableToCall, out var term)
                        ? (term is ComplexTerm result
                            ? result
                            : throw new InvalidOperationException($"Variable {variableToCall.Name} is instantiated to {term} while it is supposed to be instantiated to a complex term in order to be callable."))
                        : throw new InvalidOperationException($"Variable {variableToCall.Name} is supposed to be instantiated in order to be callable."),
                    _ => throw new InvalidOperationException($"Can not apply call() to {currentQueryRaw.Arguments}. Only single variable is accepted.")
                };

            static IEnumerable<Rule> FindMatchingRules(IReadOnlyCollection<Rule> programRules, ComplexTerm query) =>
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

        private static T Trace<T>(this T @this, int nestingLevel, string? description = null)
        {
            ProofEvent?.Invoke(description, nestingLevel, @this!);
            return @this;
        }

        private static int NextNewVariableIndex;
    }
}