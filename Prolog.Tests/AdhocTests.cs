using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prolog.Engine;
using Prolog.Engine.Miscellaneous;

namespace Prolog.Tests
{
    using static DomainApi;

    [TestClass]
    public class AdhocTests
    {
        [TestMethod]
        public void HashCodesOfEqualRecordsShouldBeTheSame()
        {
            CheckPairOfRecords(() => Atom("фермер"));

            CheckPairOfRecords(() => Variable("ОдинБерег"));

            CheckPairOfRecords(() => 
                new StructuralEquatableArray<Term>(
                    Atom("фермер"), 
                    Variable("ОдинБерег")));

            CheckPairOfRecords(() =>
                ComplexTerm( 
                        Functor("находиться", 2), 
                        Atom("фермер"), 
                        Variable("ОдинБерег")));

            CheckPairOfRecords(() => 
                new StructuralEquatableDictionary<Variable, Term>(
                    new Dictionary<Variable, Term>
                    {
                        [Variable("ОдинБерег")] = Atom("фермер"),
                        [Variable("ДругойБерег")] = ComplexTerm( 
                                                        Functor("находиться", 2), 
                                                        Atom("фермер"), 
                                                        Variable("ОдинБерег"))
                    }
                ));
        }

        private static void CheckPairOfRecords<T>(Func<T> createRecord) =>
            CheckPairOfRecords(createRecord(), createRecord());

        private static void CheckPairOfRecords<T>(T firstRecord, T secondRecord)
        {
            Assert.AreEqual(firstRecord, secondRecord);
            Assert.AreEqual(firstRecord!.GetHashCode(), secondRecord!.GetHashCode());
        }
    }
}