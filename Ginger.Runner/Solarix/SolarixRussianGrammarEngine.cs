using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Prolog.Engine.Miscellaneous;
using SolarixGrammarEngineNET;

namespace Ginger.Runner.Solarix
{
    using static MayBe;
    using static MakeCompilerHappy;

    internal sealed class SolarixRussianGrammarEngine : IRussianGrammarParser
    {
        public SolarixRussianGrammarEngine()
        {
            _engineHandle = new DisposableIntPtr(
                GrammarEngine.sol_CreateGrammarEngineW(null), 
                handle => SuppressCa1806(GrammarEngine.sol_DeleteGrammarEngine(handle)),
                "GrammarEngine");

            var loadStatus = GrammarEngine.sol_LoadDictionaryExW(
                    _engineHandle,
                    DefaultSolarixDictionaryXmlPath,
                    GrammarEngine.EngineInstanceFlags.SOL_GREN_LAZY_LEXICON);

            if (loadStatus != 1)
            {
                throw new InvalidOperationException($"Could not load Dictionary from {DefaultSolarixDictionaryXmlPath}. {DescribeError()}");
            }

            _grammarCharacteristicsBuilders =
                new Dictionary<PartOfSpeech, Func<IntPtr, int, GrammarCharacteristics>>
                {
                    {
                        PartOfSpeech.Существительное,
                        (hNode, versionIndex) =>
                            TryGetInfinitive(hNode)
                            .Fold(
                                infinitive => new VerbalNounCharacteristics(
                                    GetNodeVersionCoordinateState<Case>(hNode, versionIndex),
                                    GetNodeVersionCoordinateState<Number>(hNode, versionIndex),
                                    GetNodeVersionCoordinateState<Gender>(hNode, versionIndex),
                                    TryGetNodeVersionCoordinateState<Form>(hNode, versionIndex),
                                    infinitive),
                                () => new NounCharacteristics(
                                    GetNodeVersionCoordinateState<Case>(hNode, versionIndex),
                                    GetNodeVersionCoordinateState<Number>(hNode, versionIndex),
                                    GetNodeVersionCoordinateState<Gender>(hNode, versionIndex),
                                    TryGetNodeVersionCoordinateState<Form>(hNode, versionIndex)))
                    },
                    {
                        PartOfSpeech.Глагол,
                        (hNode, versionIndex) =>
                            new VerbCharacteristics(
                                TryGetNodeVersionCoordinateState<Case>(hNode, versionIndex),
                                GetNodeVersionCoordinateState<Number>(hNode, versionIndex),
                                GetNodeVersionCoordinateState<VerbForm>(hNode, versionIndex),
                                TryGetNodeVersionCoordinateState<Person>(hNode, versionIndex),
                                GetNodeVersionCoordinateState<VerbAspect>(hNode, versionIndex),
                                GetNodeVersionCoordinateState<Tense>(hNode, versionIndex),
                                TryGetNodeVersionCoordinateState<Transitiveness>(hNode, versionIndex))
                    },
                    {
                        PartOfSpeech.Прилагательное,
                        (hNode, versionIndex) =>
                            new AdjectiveCharacteristics(
                                TryGetNodeVersionCoordinateState<Case>(hNode, versionIndex),
                                TryGetNodeVersionCoordinateState<Number>(hNode, versionIndex),
                                TryGetNodeVersionCoordinateState<Gender>(hNode, versionIndex),
                                GetBoolCoordinateState(hNode, versionIndex, GrammarEngineAPI.SHORTNESS_ru, AdjectiveForm.Краткое, AdjectiveForm.Полное),
                                GetNodeVersionCoordinateState<ComparisonForm>(hNode, versionIndex))
                    },
                    {
                        PartOfSpeech.Наречие,
                        (hNode, versionIndex) =>
                            new AdverbCharacteristics(GetNodeVersionCoordinateState<ComparisonForm>(hNode, versionIndex))
                    },
                    {
                        PartOfSpeech.Деепричастие,
                        (hNode, versionIndex) =>
                            new GerundCharacteristics(
                                GetNodeVersionCoordinateState<Case>(hNode, versionIndex),
                                GetNodeVersionCoordinateState<VerbAspect>(hNode, versionIndex))
                    },
                    {
                        PartOfSpeech.Местоимение,
                        (hNode, versionIndex) =>
                            new PronounCharacteristics(
                                GetNodeVersionCoordinateState<Gender>(hNode, versionIndex),
                                GetNodeVersionCoordinateState<Number>(hNode, versionIndex),
                                GetNodeVersionCoordinateState<Person>(hNode, versionIndex))
                    },
                    {
                        PartOfSpeech.Инфинитив,
                        (hNode, versionIndex) =>
                            GetNodeVersionCoordinateState<VerbAspect>(hNode, versionIndex).Apply(
                                verbAspect => verbAspect == VerbAspect.Совершенный 
                                    ? new InfinitiveCharacteristics(
                                        GetNodeVersionCoordinateState<VerbAspect>(hNode, versionIndex),
                                        GetNodeVersionCoordinateState<Transitiveness>(hNode, versionIndex),
                                        GrammarEngine.sol_GetNodeContentsFX(hNode))
                                    : new InfinitiveCharacteristics(
                                        GetNodeVersionCoordinateState<VerbAspect>(hNode, versionIndex),
                                        GetNodeVersionCoordinateState<Transitiveness>(hNode, versionIndex),
                                        GetPerfectFormOfInfinitive(hNode).OrElse(string.Empty)))
                    }
                };
        }

        public IReadOnlyCollection<SentenceElement> Parse(string text)
        {
            IntPtr hPack = IntPtr.Zero;
            try
            {
                hPack = GrammarEngine.sol_SyntaxAnalysis(
                    _engineHandle,
                    text,
                    GrammarEngine.MorphologyFlags.SOL_GREN_MODEL,
                    GrammarEngine.SyntaxFlags.DEFAULT,
                    (20 << 22) | 30000,
                    RussianLanguage);

                if (hPack == IntPtr.Zero)
                {
                    throw new InvalidOperationException($"Could parse the text: {text}. {DescribeError()}");
                }

                var ngrafs = GrammarEngine.sol_CountGrafs(hPack);
                if (ngrafs <= 0)
                {
                    throw new InvalidOperationException($"No graphs were parsed from the text: {text}.");
                }

                return Enumerable.Range(1, GrammarEngine.sol_CountRoots(hPack, 0) - 2)
                    .Select(i => CreateSentenceElement(GrammarEngine.sol_GetRoot(hPack, 0, i)))
                    .AsImmutable();
            }
            finally
            {
                GrammarEngine.sol_DeleteResPack(hPack);
            }
        }

        private SentenceElement CreateSentenceElement(IntPtr hNode, int? leafType = null)
        {
            var content = GrammarEngine.sol_GetNodeContentsFX(hNode);

            var lemmaVersions = Enumerable.Range(0, GrammarEngine.sol_GetNodeVersionCount(_engineHandle, hNode))
                .Select(versionIndex => 
                new
                {
                    VersionIndex = versionIndex,
                    EntryVersionId = GrammarEngine.sol_GetNodeVerIEntry(_engineHandle, hNode, versionIndex)
                })
                .Select(item =>
                {
                    var entryVersionId = item.EntryVersionId;
                    var versionIndex = item.VersionIndex;
                    var lemma = new StringBuilder(LongestWordLength);
                    SuppressCa1806(GrammarEngine.sol_GetEntryName(_engineHandle, entryVersionId, lemma));

                    var partOfSpeechIndex = GrammarEngine.sol_GetEntryClass(_engineHandle, entryVersionId);
                    var partOfSpeech = partOfSpeechIndex < 0 ? (PartOfSpeech?)null : (PartOfSpeech)partOfSpeechIndex;
                    var grammarCharacteristcs = BuldGrammarCharacteristics(hNode, versionIndex, partOfSpeech);
                    foreach (var coordinateId in new[] { GrammarEngineAPI.CASE_ru, GrammarEngineAPI.NUMBER_ru, GrammarEngineAPI.GENDER_ru,
                                                 GrammarEngineAPI.VERB_FORM_ru, GrammarEngineAPI.PERSON_ru, GrammarEngineAPI.ASPECT_ru,
                                                 GrammarEngineAPI.TENSE_ru, GrammarEngineAPI.SHORTNESS_ru,
                                                 GrammarEngineAPI.FORM_ru, GrammarEngineAPI.COMPAR_FORM_ru })
                    {
                        var stateName = new StringBuilder(100);
                        SuppressCa1806(GrammarEngine.sol_GetCoordName(_engineHandle, coordinateId, stateName));
                        var stateValue = new StringBuilder(100);
                        if (GrammarEngine.sol_CountCoordStates(_engineHandle, coordinateId) != 0)
                        {
                            var coordState = GrammarEngine.sol_GetNodeVerCoordState(hNode, versionIndex, coordinateId);

                            if (coordState < 0)
                            {
                                continue;
                            }

                            SuppressCa1806(GrammarEngine.sol_GetCoordStateName(_engineHandle, coordinateId, coordState, stateValue));
                        }
                    }

                    return new LemmaVersion(lemma.ToString(), partOfSpeech, grammarCharacteristcs);
                });

            var children = Enumerable.Range(0, GrammarEngine.sol_CountLeafs(hNode))
                .Select(leaveIndex => CreateSentenceElement(
                    GrammarEngine.sol_GetLeaf(hNode, leaveIndex), 
                    GrammarEngine.sol_GetLeafLinkType(hNode, leaveIndex)));

            return new SentenceElement(
                Content: content,
                LeafLinkType: leafType == null || leafType.Value < 0 ? (LinkType?)null : (LinkType)leafType.Value,
                LemmaVersions: lemmaVersions.AsImmutable(), 
                Children: children.ToList());
        }

        public void Dispose()
        {
            _engineHandle.Dispose();
        }

        private string DescribeError()
        {
            var errorLength = GrammarEngine.sol_GetErrorLen(_engineHandle);
            var errorBuffer = new StringBuilder(errorLength);
            var errorCode = GrammarEngine.sol_GetError(_engineHandle, errorBuffer, errorLength);
            return errorCode == 1 ? errorBuffer.ToString() : "Unknown error";
        }

        private static string DefaultSolarixDictionaryXmlPath => 
            Path.GetFullPath(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location)!.FullName, @"dictionary.xml"));

        private GrammarCharacteristics BuldGrammarCharacteristics(IntPtr hNode, int versionIndex, PartOfSpeech? partOfSpeech) => 
            partOfSpeech == null || !_grammarCharacteristicsBuilders.TryGetValue(partOfSpeech.Value, out var builder)
                    ? new NullGrammarCharacteristics()
                    : builder(hNode, versionIndex);

        private static TState GetNodeVersionCoordinateState<TState>(IntPtr hNode, int versionIndex) where TState: struct 
        {
            var result = TryGetNodeVersionCoordinateState<TState>(hNode, versionIndex);
            if (result == null)
            {
                throw new InvalidOperationException($"Could not get {typeof(TState).Name} coordinate");
            }

            return result.Value;
        }

        private MayBe<string> TryGetInfinitive(IntPtr hNode)
        {
            var wordList = GrammarEngine.sol_SeekThesaurus(_engineHandle, GrammarEngine.sol_GetNodeIEntry(_engineHandle, hNode), 0, 0, 0, 0, 0);
            var potentialInfinitives = SolarixIntArrayToSystemArray(wordList);
            GrammarEngine.sol_DeleteInts(wordList);

            return potentialInfinitives
                .Select(id => new
                {
                    id,
                    classId = GrammarEngine.sol_GetEntryClass(_engineHandle, id),
                    aspect = GrammarEngine.sol_GetEntryCoordState(_engineHandle, id, GrammarEngineAPI.ASPECT_ru)
                })
                .Where(item => item.classId == GrammarEngineAPI.INFINITIVE_ru &&
                               item.aspect.IsOneOf(GrammarEngineAPI.PERFECT_ru, GrammarEngineAPI.IMPERFECT_ru))
                .OrderBy(item => item.aspect == GrammarEngineAPI.PERFECT_ru ? 0 : 1)
                .TryFirst()
                .Map(item => GetEntryName(item.id));
        }

        private MayBe<string> GetPerfectFormOfInfinitive(IntPtr hNode)
        {
            var linksList = GrammarEngine.sol_ListLinksTxt(_engineHandle, GrammarEngine.sol_GetNodeIEntry(_engineHandle, hNode), GrammarEngineAPI.TO_PERFECT, 0);
            if (linksList == IntPtr.Zero)
            {
                return None;
            }

            var linkListCount = GrammarEngine.sol_LinksInfoCount(_engineHandle, linksList);
            if (linkListCount == 0)
            {
                SuppressCa1806(GrammarEngine.sol_DeleteLinksInfo(_engineHandle, linksList));
                return None;
            }

            string result = string.Empty;
            for (var i = 0; i != linkListCount; ++i)
            {
                var linkedWordId = GrammarEngine.sol_LinksInfoEKey2(_engineHandle, linksList, i);
                if (GrammarEngine.sol_GetEntryCoordState(_engineHandle, linkedWordId, GrammarEngineAPI.ASPECT_ru) == GrammarEngineAPI.PERFECT_ru)
                {
                    result = GetEntryName(linkedWordId);
                    break;
                }
            }

            SuppressCa1806(GrammarEngine.sol_DeleteLinksInfo(_engineHandle, linksList));

            return Some(result);
        }

        private static TState? TryGetNodeVersionCoordinateState<TState>(IntPtr hNode, int versionIndex) where TState : struct
        {
            var coordinateId = CoordinateStateTypeToCoordinateIdMap[typeof(TState)];
            var coordinateState = GrammarEngine.sol_GetNodeVerCoordState(hNode, versionIndex, coordinateId);
            return (coordinateState < 0) ? (TState?)null : (TState)(object)coordinateState;
        }

        private static TState GetBoolCoordinateState<TState>(IntPtr hNode, int versionIndex, int coordinateId, TState trueValue, TState falseValue)
        {
            var valueCode = GrammarEngine.sol_GetNodeVerCoordState(hNode, versionIndex, coordinateId);
            switch (valueCode)
            {
                case 0:
                    return falseValue;
                case 1:
                    return trueValue;
                default:
                    throw new InvalidOperationException($"GetBoolCoordinateState<{typeof(TState).Name}>({coordinateId}) returned {valueCode} instead of 0 or 1.");
            }
        }

        private static int[] SolarixIntArrayToSystemArray(IntPtr solarixIntArray)
        {
            var arrayLength = GrammarEngine.sol_CountInts(solarixIntArray);
            if (arrayLength <= 0)
            {
                return Array.Empty<int>();
            }

            var result = new int[arrayLength];
            for (var i = 0; i != arrayLength; ++i)
            {
                result[i] = GrammarEngine.sol_GetInt(solarixIntArray, i);
            }

            return result;
        }

        private string GetEntryName(int entryId)
        {
            var buffer = new StringBuilder(LongestWordLength);
            SuppressCa1806(GrammarEngine.sol_GetEntryName(_engineHandle, entryId, buffer));
            return buffer.ToString();
        }

        private readonly DisposableIntPtr _engineHandle;
        private readonly IDictionary<PartOfSpeech, Func<IntPtr, int, GrammarCharacteristics>> _grammarCharacteristicsBuilders; 

        private static readonly IDictionary<Type, int> CoordinateStateTypeToCoordinateIdMap = 
            new Dictionary<Type, int>
            {
                { typeof(Case), GrammarEngineAPI.CASE_ru },
                { typeof(Number), GrammarEngineAPI.NUMBER_ru },
                { typeof(Gender), GrammarEngineAPI.GENDER_ru },
                { typeof(Form), GrammarEngineAPI.FORM_ru },
                { typeof(Person), GrammarEngineAPI.PERSON_ru },
                { typeof(VerbForm), GrammarEngineAPI.VERB_FORM_ru },
                { typeof(VerbAspect), GrammarEngineAPI.ASPECT_ru },
                { typeof(Tense), GrammarEngineAPI.TENSE_ru },
                { typeof(ComparisonForm), GrammarEngineAPI.COMPAR_FORM_ru },
                { typeof(Transitiveness), GrammarEngineAPI.TRANSITIVENESS_ru }
            };

        private const int RussianLanguage = 2;
        private const int LongestWordLength = 100;
    }
}