Feature: Lemma Versions Disambiguating Annotations
    В тех случаях когда в паттерне есть слова, которые при парсинге имеют несколько возможных вариантов Lemma Versions,
    нужно возможность указать прямо в тексте паттерна, какую именно версию следует использовать.

Scenario: Lemma Versions Disambiguating Annotation
    Then the following variants should be proposed by the disambiguation API
        | Sentence with ambiguous lemma versions                         | Proposed disambiguation                |
        | если имеется свободный слот, то ЕГО занимает женщина           | (вин.,муж.); (вин.,ср.); (род.,ср.)    |
     And disambiguation annotation should be applied correctly
        | Sentence with disambiguation annotation                        | Ambiguous word  | Parsed Grammar Characteristics                                |
        | если имеется свободный слот, то ЕГО(вин.,ср.) занимает женщина | ЕГО             | PronounCharacteristics { Case: Винительный, Gender: Средний } |
