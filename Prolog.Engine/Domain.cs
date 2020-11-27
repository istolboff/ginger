namespace Prolog.Engine
{
    public abstract record Term;

    public sealed record Atom(string Characters) : Term;

    public sealed record Number(int Value) : Term;

    public sealed record Variable(string Name) : Term;

    public sealed record Functor(string Name, int arity);

    public record ComplexTerm(Functor Functor, StructuralEquatableArray<Term> Arguments) : Term;
}
