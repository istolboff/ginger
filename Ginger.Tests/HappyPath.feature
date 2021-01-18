Feature: We generate testing scenarios from the formal description of the System Under Test

@Ignore
Scenario: Wolf-Goat-Cabbage riddle solving
   Given SUT is described as follows
    | Type              | Phrasing                                                                                      |
    |-------------------|-----------------------------------------------------------------------------------------------|
    | Сущность          | Река имеет левый и правый берега                                                              |
    |-------------------|-----------------------------------------------------------------------------------------------|
    | Сущность          | Волк, коза и капуста являются перевозимыми существами                                         |
    |-------------------|-----------------------------------------------------------------------------------------------|
    | Воздействие       |  Действие 'фермер перевозит перевозимое существо на другой берег' переводит из состояния      |
    |                   | 'фермер и перевозимое существо находятся на одном береге' в состояние                         |
    |                   | 'фермер и перевозимое существо находятся на другом береге'.                                   |
    |-------------------|-----------------------------------------------------------------------------------------------|
    | Воздействие       | Действие 'фермер переправляется на другой берег' переводит из состояния                       |
    |                   | 'фермер находится на одном береге' в состояние                                                |
    |                   | 'фермер находится на другом береге'.                                                          |
    |-------------------|-----------------------------------------------------------------------------------------------|
    | Начальное условие | Фермер, волк, коза и капуста находятся на левом береге реки                                   |
    |-------------------|-----------------------------------------------------------------------------------------------|
    | Правило           | Если волк, коза и капуста находятся на правом береге реки, то миссия заканчивается успехом    |
    |-------------------|-----------------------------------------------------------------------------------------------|
    | Правило           | Если волк и коза находятся на одном береге реки, а фермер находится на другом береге реки,    |
    |                   | то миссия заканчивается неудачей с формулировкой 'волк съел козу'                             |
    |-------------------|-----------------------------------------------------------------------------------------------|
    | Правило           | Если коза и капуста находятся на одном береге реки, а фермер находится на другом береге реки, |
    |                   | то миссия заканчивается неудачей с формулировкой 'коза съела капусту'                         |
    |-------------------|-----------------------------------------------------------------------------------------------|
    Then the following scenarios should be generated
        | Expected Outcome             | Scenario Steps                                                                                                                                                                                                                                                                              |
        | миссия заканчивается успехом | [[перевозит(фермер, коза, левый, правый), переправляется(фермер, правый, левый), перевозит(фермер, волк, левый, правый), перевозит(фермер, коза, правый, левый), перевозит(фермер, капуста, левый, правый), переправляется(фермер, правый, левый), перевозит(фермер, коза, левый, правый)]] |
        | волк съел козу               | [[перевозит(фермер, капуста, левый, правый)], [перевозит(фермер, коза, левый, правый), переправляется(фермер, правый, левый), перевозит(фермер, волк, левый, правый), переправляется(фермер, правый, левый)]]                                                                               |
        | коза съела капусту           | [[перевозит(фермер, волк, левый, правый)], [перевозит(фермер, коза, левый, правый), переправляется(фермер, правый, левый), перевозит(фермер, капуста, левый, правый), переправляется(фермер, правый, левый)]]                                                                               |
