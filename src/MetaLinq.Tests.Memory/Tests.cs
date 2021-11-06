﻿using JetBrains.dotMemoryUnit;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MetaLinq;
using System.Linq;

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
            testDataList = Enumerable.ToList(testDataArray);
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
        static int[] intArray = Enumerable.ToArray(Enumerable.Range(0, 5));
        static List<int> intList = Enumerable.ToList(Enumerable.Range(0, 5));

        class Foo { }
        class Bar { }
        struct DisposableStruct : IDisposable {
            public void Dispose() {
            }
        }

        #region common
        [Test]
        public static void AssertDifference_NoAllocs() {
            MemoryTestHelper.AssertDifference(() => { }, null);
        }
        [Test]
        public static void AssertDifference_SomeAllocs() {
            MemoryTestHelper.AssertDifference(() => { new Foo(); new Foo(); new Bar(); }, new[] {
                (typeof(Foo).FullName!, 2),
                (typeof(Bar).FullName!, 1),
            });
        }
        [Test]
        public static void AssertDifference_HiddenAllocs() {
            MemoryTestHelper.AssertDifference(() => { IDisposable boxed = new DisposableStruct(); }, new[] {
                (typeof(DisposableStruct).FullName!, 1),
            });
        }
        #endregion

        #region temp
        [Test]
        public static void Array_Select_Where_ToList() {
            MemoryTestHelper.AssertDifference(() => intArray.Select_Meta(static x => x * 10).Where_Meta(static x => x % 100 == 0).ToList_Meta(), ExpectedListOfIntsAllocations());
        }
        [Test]
        public static void Array_SelectMany_ToList() {
            MemoryTestHelper.AssertDifference(() => testDataArray.SelectMany_Meta(static x => x.IntArray).ToLis_Meta(), ExpectedListOfIntsAllocations());
        }
        [Test]
        public static void List_SelectMany_ToList() {
            MemoryTestHelper.AssertDifference(() => testDataList.SelectMany_Meta(static x => x.IntArray).ToLis_Meta(), ExpectedListOfIntsAllocations());
        }
        #endregion

        #region where
        [Test]
        public static void Array_Where_ToArray() {
            MemoryTestHelper.AssertDifference(() => intArray.Where(static x => x < 3).ToArray(), ExpectedArrayOfIntsAllocations());
        }
        [Test]
        public static void Array_Where_Foreach() {
            MemoryTestHelper.AssertDifference(() => {
                int sum = 0;
                foreach(int item in intArray.Where(static x => x < 4)) {
                    sum += item;
                }
                AssertValue(6, sum);
            }, null);
        }
        [Test]
        public static void List_Where_ToArray() {
            MemoryTestHelper.AssertDifference(() => intList.Where(static x => x < 3).ToArray(), ExpectedArrayOfIntsAllocations());
        }
        [Test]
        public static void List_Where_Foreach() {
            MemoryTestHelper.AssertDifference(() => {
                int sum = 0;
                foreach(int item in intList.Where(static x => x < 4)) {
                    sum += item;
                }
                AssertValue(6, sum);
            }, null);
        }
        #endregion

        #region select
        [Test]
        public static void Array_Select_ToArray() {
            MemoryTestHelper.AssertDifference(() => intArray.Select(static x => x * 2).ToArray(), ExpectedArrayOfIntsAllocations());
        }
        //[Test]
        //public static void Array_Where_Foreach() {
        //    MemoryTestHelper.AssertDifference(() => {
        //        int sum = 0;
        //        foreach(int item in intArray.Where(static x => x < 4)) {
        //            sum += item;
        //        }
        //        AssertValue(6, sum);
        //    }, null);
        //}
        //[Test]
        //public static void List_Where_ToArray() {
        //    MemoryTestHelper.AssertDifference(() => intList.Where(static x => x < 3).ToArray(), ExpectedArrayOfIntsAllocations());
        //}
        //[Test]
        //public static void List_Where_Foreach() {
        //    MemoryTestHelper.AssertDifference(() => {
        //        int sum = 0;
        //        foreach(int item in intList.Where(static x => x < 4)) {
        //            sum += item;
        //        }
        //        AssertValue(6, sum);
        //    }, null);
        //}
        #endregion

        static void AssertValue<T>(T expected, T actual) {
            if(!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new InvalidOperationException();
        }

        static (string, int)[] ExpectedListOfIntsAllocations() {
            return new[] {
                ("System.Collections.Generic.List`1[System.Int32]", 1),
                ("System.Int32[]", 1),
            };
        }
        static (string, int)[] ExpectedArrayOfIntsAllocations() {
            return new[] {
                ("System.Int32[]", 1),
            };
        }
    }
}
