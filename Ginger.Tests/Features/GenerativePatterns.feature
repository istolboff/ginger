Feature: Generative Patterns
    We are able to formulate patterns of the natural language so that they can vary their structure to some degree.

Scenario: Демонстрация того, как из обобщенного паттерна генерируется набор конкретных паттернов
    Then the following concrete patterns should be generated from a given generative patterns
    | Generative Pattern                                | Resulting Concrete Patterns                                            |
    |---------------------------------------------------|------------------------------------------------------------------------|
# Паттерны с единственным и множественным числом ({мн.}), но без реплицируемого слова
    | заяц переходит с одного размера куста{мн.}        |  заяц переходит с одного размера куста на другой;                      |
    | на другой                                         |  заяц переходит с одного размера кустов на другой                      |
    |---------------------------------------------------|------------------------------------------------------------------------|
# Паттерны с реплицируемым словом  ({и проч.})
    | медведь{и проч.} является{мн.} отрицательным{мн.} |  медведь является отрицательным героем;                                |
    | героем{мн.}                                       |  медведь и летчик являются отрицательными героями;                     |
    |                                                   |  медведь, летчик и наводчик являются отрицательными героями;           |
    |                                                   |  медведь, летчик, наводчик и поэт являются отрицательными героями      |
    |---------------------------------------------------|------------------------------------------------------------------------|
    | у реки ~есть~ левый{и проч.} берег{мн.}           |  у реки ~есть~ левый берег;                                            |
    |                                                   |  у реки ~есть~ левый и красный берега;                                 |
    |                                                   |  у реки ~есть~ левый, красный и длинный берега;                        |
    |                                                   |  у реки ~есть~ левый, красный, длинный и теплый берега                 |
    |---------------------------------------------------|------------------------------------------------------------------------|
    | ~если имеется{мн.}~ свободный{мн.} слот{мн.},     |  ~если имеется~ свободный слот, то его занимает женщина;               |
    | то его{мн.} занимает{мн.} женщина {и проч.}       |  ~если имеются~ свободные слоты, то их занимают женщина и летчик;      |
    |                                                   |  ~если имеются~ свободные слоты, то их занимают женщина, летчик        |
    |                                                   |  и наводчик;                                                           |
    |                                                   |  ~если имеются~ свободные слоты, то их занимают женщина, летчик,       |
    |                                                   |  наводчик и поэт                                                       |
    |---------------------------------------------------|------------------------------------------------------------------------|
    | морковь нравится зайцу{и проч.}                   |  морковь нравится зайцу;                                               |
    |                                                   |  морковь нравится зайцу и летчику;                                     |
    |                                                   |  морковь нравится зайцу, летчику и наводчику;                          |
    |                                                   |  морковь нравится зайцу, летчику, наводчику и поэту                    |
    |---------------------------------------------------|------------------------------------------------------------------------|
# Обобщенный паттерн с реплицируемым словом может содержать так же и disambiguating annotations
    | если имеется{мн.} свободный{мн.} слот{мн.},       |  если имеется свободный слот, то его(вин.,муж.) занимает женщина;      |
    | то его(вин.,муж.){мн.} занимает{мн.}              |  если имеются свободные слоты, то их(вин.) занимают женщина и летчик;  |
    | женщина {и проч.}                                 |  если имеются свободные слоты, то их(вин.) занимают женщина, летчик    |
    |                                                   |  и наводчик;                                                           |
    |                                                   |  если имеются свободные слоты, то их(вин.) занимают женщина, летчик,   |
    |                                                   |  наводчик и поэт                                                       |
    |---------------------------------------------------|------------------------------------------------------------------------|
# Паттерн с конечным набором альтернатив в данном месте предложения (∥(..., ...))
    | медведь лежит на ~∥(солнечной, теневой)~ стороне  | медведь лежит на ~солнечной~ стороне улицы;                            |
    | улицы                                             | медведь лежит на ~теневой~ стороне улицы                               |
    |---------------------------------------------------|------------------------------------------------------------------------|
# Паттерн с комбинацией конечного набора альтернатив и реплицируемого слова)
    | если ∥(солнечная, теневая) сторона улицы свободна,| если солнечная сторона улицы свободна, то ее занимает медведь;         |
    | то ее занимает{мн.} медведь{и проч.}              | если солнечная сторона улицы свободна, то ее занимают медведь и        |
    |                                                   | летчик;                                                                |
    |                                                   | если солнечная сторона улицы свободна, то ее занимают медведь, летчик  |
    |                                                   | и наводчик;                                                            |
    |                                                   | если солнечная сторона улицы свободна, то ее занимают медведь, летчик, |
    |                                                   | наводчик и поэт;                                                       |
    |                                                   | если теневая сторона улицы свободна, то ее занимает медведь;           |
    |                                                   | если теневая сторона улицы свободна, то ее занимают медведь и летчик;  |
    |                                                   | если теневая сторона улицы свободна, то ее занимают медведь, летчик и  |
    |                                                   | наводчик;                                                              |
    |                                                   | если теневая сторона улицы свободна, то ее занимают медведь, летчик,   |
    |                                                   | наводчик и поэт;                                                       |
    |---------------------------------------------------|------------------------------------------------------------------------|


Scenario: Предложение с единственным объектом обобщается на случай, когда перечисляется несколько объектов
    Given the following Patterns
    | Id | Pattern                                                          | Meaning                        |
    |----|------------------------------------------------------------------|--------------------------------|
    | 1  | медведь{и проч.} является{мн.} отрицательным{мн.} героем{мн.}    | отрицательныйГерой(медведь).   |
    |----|------------------------------------------------------------------|--------------------------------|
    | 2  | у реки ~есть~ левый{и проч.} берег{мн.}                          | берег(река, левый).            |
    |----|------------------------------------------------------------------|--------------------------------|
    | 3  | ~если имеется{мн.}~ свободный{мн.} слот{мн.}, то его (вин.,муж.) | занимать(X, женщина) :-        |
    |    | {мн.} занимает{мн.} женщина{и проч.}                             |     слот(X), свободный(X).     |
    |----|------------------------------------------------------------------|--------------------------------|
    | 4  | сейчас медведь лежит на ~∥(одной, другой)~ стороне улицы         | сейчас([                       |
    |    |                                                                  |        лежать(медведь, на, X), |
    |    |                                                                  |        сторона(улица, X),      |
    |    |                                                                  |        X = '∥'                 |
    |    |                                                                  |     ]).                        |
    |----|------------------------------------------------------------------|--------------------------------|
    Then the following understandings should be possible
    | Sentence                                              | Meaning                       | Recognized with Pattern |
    |-------------------------------------------------------|-------------------------------|-------------------------| 
    | Волк является перевозимым существом                   | перевозимыйСущество(волк).    | 1                       |
    |-------------------------------------------------------|-------------------------------|-------------------------| 
    | Волк и коза являются перевозимыми существами          | перевозимыйСущество(волк).    | 1                       |
    |                                                       | перевозимыйСущество(коза).    |                         |
    |-------------------------------------------------------|-------------------------------|-------------------------| 
    | Волк, коза и капуста являются перевозимыми существами | перевозимыйСущество(волк).    | 1                       |
    |                                                       | перевозимыйСущество(коза).    |                         |
    |                                                       | перевозимыйСущество(капуста). |                         |
    |-------------------------------------------------------|-------------------------------|-------------------------| 
    | у мартышки есть верхняя челюсть                       | челюсть(мартышка, верхний).   | 2                       |
    |-------------------------------------------------------|-------------------------------|-------------------------| 
    | у мартышки есть верхняя и нижняя челюсти              | челюсть(мартышка, верхний).   | 2                       |
    |                                                       | челюсть(мартышка, нижний).    |                         |
    |-------------------------------------------------------|-------------------------------|-------------------------| 
    | у куртки есть маленький, средний и большой карманы    | карман(куртка, маленький).    | 2                       |
    |                                                       | карман(куртка, средний).      |                         |
    |                                                       | карман(куртка, большой).      |                         |
    |-------------------------------------------------------|-------------------------------|-------------------------| 
    | если имеется зеленый шар, то его хватает мартышка     | хватать(X, мартышка) :-       | 3                       |
    |                                                       |    шар(X), зеленый(X).        |                         |
    |-------------------------------------------------------|-------------------------------|-------------------------| 
    | если имеются зеленые шары, то их хватают мартышка и   | хватать(X, мартышка) :-       | 3                       |
    | гиббон                                                |    шар(X), зеленый(X).        |                         |
    |                                                       | хватать(X, гиббон) :-         |                         |
    |                                                       |    шар(X), зеленый(X).        |                         |
    |-------------------------------------------------------|-------------------------------|-------------------------| 
    | если имеются зеленые шары, то их хватают мартышка,    | хватать(X, мартышка) :-       | 3                       |
    | гиббон и горилла                                      |    шар(X), зеленый(X).        |                         |
    |                                                       | хватать(X, гиббон) :-         |                         |
    |                                                       |    шар(X), зеленый(X).        |                         |
    |                                                       | хватать(X, горилла) :-        |                         |
    |                                                       |    шар(X), зеленый(X).        |                         |
    |-------------------------------------------------------|-------------------------------|-------------------------| 
    | сегодня фермер находится на одном береге реки         | сегодня([                     | 4                       |
    |                                                       |    находиться(фермер, на, X), |                         |
    |                                                       |    берег(река, X),            |                         |
    |                                                       |    X = один                   |                         |
    |                                                       |   ]).                         |                         |
    |-------------------------------------------------------|-------------------------------|-------------------------| 
    | сегодня фермер находится на другом береге реки        | сегодня([                     | 4                       |
    |                                                       |    находиться(фермер, на, X), |                         |
    |                                                       |    берег(река, X),            |                         |
    |                                                       |    X = другой                 |                         |
    |                                                       |   ]).                         |                         |
    |-------------------------------------------------------|-------------------------------|-------------------------| 
    And the following sentences should fail to be understood
    | Sentence                                            | Reason                                                  |
    | Волк, коза и капуста являются перевозимым существом | 'перевозимым существом' - требуется множественное число |

Scenario: Паттерн с конечным набором альтернатив в данном месте предложения (∥(..., ...)) когда Meaning зависит от выбранной альтернативы
    Given the following Patterns
    | Id  | Pattern                                               | Meaning                                          |
    | ----| ------------------------------------------------------| -------------------------------------------------|
    | 1   | медведь лежит на ∥(одной, другой) стороне улицы       | лежать(медведь, @variable(∥, сторона)),          |
    |     |                                                       | сторона(улица, @variable(∥, сторона))            |
    | ----| ------------------------------------------------------| -------------------------------------------------|
    Then the following understandings should be possible
    | Sentence                                 | Meaning                                                    |
    | фермер находится на одном береге реки    | находиться(фермер, ОдинБерег), берег(река, ОдинБерег)      |
    | фермер находится на другом береге реки   | находиться(фермер, ДругойБерег), берег(река, ДругойБерег)  |


Scenario: Ограничения, накладываемые на обобщенные паттерны
    Then Parsing of the following generative patterns should fail
    | Pattern                                      | Failure reason                                            |
    |----------------------------------------------|-----------------------------------------------------------|
    | кукушка{и проч.} является{мн.} героиней{мн.} | Слова, переходящие во множественное число при репликации, |
    |                                              | всегда должны иметь мужской род                           |