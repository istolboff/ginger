using Prolog.Engine.Miscellaneous;

namespace Ginger.Runner.Solarix
{
    internal static class SolarixExtensions
    {
        public static bool CompatibleTo(this GrammarCharacteristics @this, GrammarCharacteristics right) =>
            (@this.GetType() == right.GetType() ||
                @this.GetType().IsSubclassOf(right.GetType()) ||
                right.GetType().IsSubclassOf(@this.GetType())) &&
            (@this, right) switch 
            {
                (NounCharacteristics leftNoun, NounCharacteristics rightNoun) => 
                    CasesAreCompatible(leftNoun.Case, rightNoun.Case) &&
                    leftNoun.Number == rightNoun.Number,
                (AdjectiveCharacteristics leftAdjective, AdjectiveCharacteristics rightAdjective) =>
                    CasesAreCompatible(leftAdjective.Case, rightAdjective.Case) &&
                    leftAdjective.Number == rightAdjective.Number &&
                    leftAdjective.ComparisonForm == rightAdjective.ComparisonForm,
                _ => true
            };

        private static bool CasesAreCompatible(Case? left, Case? right) =>
            left == right ||
            left.IsOneOf(Case.Предложный, Case.Местный) && right.IsOneOf(Case.Предложный, Case.Местный);
    }
}