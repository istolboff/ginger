using System;
using System.Collections.Generic;

namespace Ginger.Runner.Solarix
{
    internal interface IRussianLexicon
    {
        string GenerateWordForm(string word, PartOfSpeech? partOfSpeech, GrammarCharacteristics characteristics);
        
        string GetPluralForm(LemmaVersion lemmaVersion);

        string GetPartOfSpeechName(PartOfSpeech partOfSpeech);

        string GetStateName(int categoryId, int stateId);

        (Type[] CoordinateTypes, int[] CoordinateEnumValues) ResolveCoordinates(IEnumerable<string> valueNames);
    }
}