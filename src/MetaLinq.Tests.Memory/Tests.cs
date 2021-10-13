using JetBrains.dotMemoryUnit;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MetaLinq;
using En = System.Linq.Enumerable;

//[assembly: Apartment(System.Threading.ApartmentState.STA)]

namespace MetaLinqTests.Memory {
    
    [TestFixture]
    [DotMemoryUnit(CollectAllocations = true)]
    public class Tests {
        static Tests() {
            //Debugger.Launch();

            testDataArray = new TestData[5];
            for(int i = 0; i < 5; i++) {
                testDataArray[i] = new TestData(new[] { i * 10, i * 10 + 1 });
            }
            testDataList =En.ToList(testDataArray);
        }
        class TestData {
            public TestData(int[] ints) {
                IntArray = ints;
                //IntList = ints.ToList();
            }
            public int[] IntArray { get; }
            //public List<int> IntList { get; }
        }
        static TestData[] testDataArray;
        static List<TestData> testDataList;
        static int[] data = En.ToArray(En.Range(0, 5));

        class Foo { }
        class Bar { }
        [Test]
        public void TheTest() {
            AssertDifference();
            Select_Where_Meta();
            Select_Where_Standard();
            SelectMany_Meta_Array();
            SelectMany_Meta_List();
            //SelectMany_Standard();
        }
        struct DisposableStruct : IDisposable {
            public void Dispose() {
            }
        }
        static void AssertDifference() {
            MemoryTestHelper.AssertDifference(() => { }, null);
            MemoryTestHelper.AssertDifference(() => { new Foo(); new Foo(); new Bar(); }, new[] {
                (typeof(Foo).FullName, 2),
                (typeof(Bar).FullName, 1),

            });
            MemoryTestHelper.AssertDifference(() => { IDisposable boxed = new DisposableStruct(); }, new[] {
                (typeof(DisposableStruct).FullName, 1),
            });
        }
        static void Select_Where_Meta() {
            MemoryTestHelper.AssertDifference(() => GetResultMeta(data), new[] {
                ("System.Collections.Generic.List`1[System.Int32]", 1),
                ("System.Int32[]", 1),
            });
        }
        static void Select_Where_Standard() {
            MemoryTestHelper.AssertDifference(() => GetResultStandard(data), new[] {
                ("System.Collections.Generic.List`1[System.Int32]", 1),
                ("System.Int32[]", 1),
                ("System.Linq.Enumerable+WhereEnumerableIterator`1[System.Int32]", 1),
                ("System.Linq.Enumerable+WhereSelectArrayIterator`2[System.Int32,System.Int32]", 1),
            });
        }
        static void SelectMany_Meta_Array() {
            MemoryTestHelper.AssertDifference(() => testDataArray.SelectMany(static x => x.IntArray).ToList(), new[] {
                ("System.Collections.Generic.List`1[System.Int32]", 1),
                ("System.Int32[]", 3),
            });
        }
        static void SelectMany_Meta_List() {
            MemoryTestHelper.AssertDifference(() => testDataList.SelectMany(static x => x.IntArray).ToList(), new[] {
                ("System.Collections.Generic.List`1[System.Int32]", 1),
                ("System.Int32[]", 3),
            });
        }

        //static void SelectMany_Standard() {
        //    MemoryTestHelper.AssertDifference(() => En.ToList(En.SelectMany(testData, static x => x.Ints)), new[] {
        //        ("System.Collections.Generic.List`1[System.Int32]", 1),
        //        ("System.Int32[]", 3),
        //    });
        //}


        static List<int> GetResultMeta(int[] data) {
            return data.Select(static x => x * 10).Where(static x => x % 100 == 0).ToList();
        }
        static List<int> GetResultStandard(int[] data) {
            return En.ToList(En.Where(En.Select(data, static x => x * 10), static x => x % 100 == 0));
        }
    }
}
