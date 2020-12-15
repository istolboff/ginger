namespace Prolog.Engine
{
#pragma warning disable SA1313 // ParameterNamesMustBeginWithLowerCaseLetter
    public abstract record Term;

    public sealed record Atom(string Characters) : Term;

    public sealed record Number(int Value) : Term;

#pragma warning disable CA1801 // Review unused parameters
    public sealed record Variable(string Name, bool IsTemporary = false) : Term;
#pragma warning restore CA1801

    public sealed record Functor(string Name, int arity);

    public sealed record ComplexTerm(Functor Functor, StructuralEquatableArray<Term> Arguments) : Term;

    public sealed record Rule(ComplexTerm Conclusion, StructuralEquatableArray<ComplexTerm> Premises);

#pragma warning disable CA2227 // Collection properties should be read only
    public sealed record UnificationResult(bool Succeeded, StructuralEquatableDictionary<Variable, Term> Instantiations);
#pragma warning restore CA2227
#pragma warning restore SA1313
}
