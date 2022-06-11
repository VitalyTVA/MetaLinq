using JetBrains.dotMemoryUnit;
using MetaLinq;
using MetaLinq.Tests;
using MetaLinqSpikes;

//[assembly: Apartment(System.Threading.ApartmentState.STA)]

namespace MetaLinqTests.Memory;

[TestFixture]
[DotMemoryUnit(CollectAllocations = true)]
public class Tests {
    static Tests() {
        //Debugger.Launch();

        testDataArray = new TestData[5];
        for(int i = 0; i < 5; i++) {
            testDataArray[i] = new TestData(new[] { i * 10, i * 10 + 1 }, i);
        }
        testDataList = Enumerable.ToList(testDataArray);

        var rnd = new Random(0);
        testDataArray_Shuffled = Enumerable.ToArray(Enumerable.Select(Enumerable.Range(0, 20), x => new TestData(new int[] { }, x) { Value2 = rnd.Next(5), Value3 = rnd.Next(3) }));
        for(int i = 0; i < 40; i++) {
            var i1 = rnd.Next(testDataArray_Shuffled.Length);
            var i2 = rnd.Next(testDataArray_Shuffled.Length);
            var tmp = testDataArray_Shuffled[i1];
            testDataArray_Shuffled[i1] = testDataArray_Shuffled[i2];
            testDataArray_Shuffled[i2] = tmp;
        }
    }
    record struct TestStruct(int Value);
    class TestData {
        public TestData(int[] ints, int value) {
            IntArray = ints;
            Value = value;
            //IntList = ints.ToList();
        }
        public int[] IntArray { get; }
        public int Value { get; }
        public int Value2 { get; set; }
        public int Value3 { get; set; }
        //public List<int> IntList { get; }
    }
    static TestData[] testDataArray;
    static TestData[] testDataArray_Shuffled;
    static Foo[] fooArray = new[] { new Bar(), new Bar() };
    static List<TestData> testDataList;
    static int[] intArray = Enumerable.ToArray(Enumerable.Range(0, 5));
    static List<int> intList = Enumerable.ToList(Enumerable.Range(0, 5));
    static CustomCollection<int> intCustomCollection = new CustomCollection<int>(Enumerable.ToArray(Enumerable.Range(0, 5)));

    class Foo { }
    class Bar : Foo { }
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

    [Test]
    public static void Array_OfType_ToArray() {
        MemoryTestHelper.AssertDifference(() => fooArray.OfType<Bar>().ToArray(),
            new[] {
                ($"{typeof(Bar).FullName}[]", 1),
            }
        );
    }
    [Test]
    public static void Array_Where_OfType_ToArray() {
        MemoryTestHelper.AssertDifference(() => fooArray.Where(x => true).OfType<Bar>().ToArray(),
            new[] {
                ($"{typeof(Bar).FullName}[]", 1),
            }
        );
    }
    [Test]
    public static void Array_Where_Cast_ToArray() {
        MemoryTestHelper.AssertDifference(() => fooArray.Where(x => true).Cast<Bar>().ToArray(),
            new[] {
                ($"{typeof(Bar).FullName}[]", 1),
            }
        );
    }
    [Test]
    public static void Array_TakeWhile_SkipWhile_ToArray() {
        MemoryTestHelper.AssertDifference(() => fooArray.TakeWhile(x => true).SkipWhile(x => false).Cast<Bar>().ToArray(),
            new[] {
                ($"{typeof(Bar).FullName}[]", 1),
            }
        );
    }

    #region select where
    [Test]
    public static void Array_Select_Where_ToList() {
        MemoryTestHelper.AssertDifference(() => intArray.Select(static x => x * 10).Where(static x => x % 100 == 0).ToList(), ExpectedListOfIntsAllocations());
    }
    [Test]
    public static void Array_Where_Select_ToList() {
        MemoryTestHelper.AssertDifference(() => intArray.Where(static x => x % 2 == 0).Select(static x => x * 10).ToList(), ExpectedListOfIntsAllocations());
    }
    [Test]
    public static void Array_Where_Select_First() {
        MemoryTestHelper.AssertDifference(() => intArray.Where(static x => x % 2 == 0).Select(static x => x * 10).First(x => x > 1), null);
    }
    [Test]
    public static void Array_Where_Select_Any() {
        MemoryTestHelper.AssertDifference(() => intArray.Where(static x => x % 2 == 0).Select(static x => x * 10).Any(x => x > 1), null);
    }
    #endregion

    #region temp
    [Test]
    public static void Array_SelectMany_ToList() {
        MemoryTestHelper.AssertDifference(() => testDataArray.SelectMany(static x => x.IntArray).ToList(), ExpectedListOfIntsAllocations());
    }
    [Test]
    public static void List_SelectMany_ToList() {
        MemoryTestHelper.AssertDifference(() => testDataList.SelectMany(static x => x.IntArray).ToList(), ExpectedListOfIntsAllocations());
    }
    [Test]
    public static void Array_SelectMany_ForEach() {
        MemoryTestHelper.AssertDifference(() => {
            int sum = 0;
            foreach(int item in testDataArray.SelectMany(static x => x.IntArray)) {
                sum += item;
            }
            AssertValue(205, sum);
        }, null);
    }
    #endregion

    #region order by
    [Test]
    public static void CustomCollection_OrderBy_First() {
        MemoryTestHelper.AssertDifference(() => intCustomCollection.OrderBy(static x => 2 * x).First(x => x > 1), null);
    }
    [Test]
    public static void Array_OrderBy_ToArray() {
        MemoryTestHelper.AssertDifference(() => testDataArray_Shuffled.OrderBy(static x => x.Value).ToArray(), ExpectedOrderByAllocations());
    }
    [Test]
    public static void Array_OrderBy_ThenBy_ThenBy_ToArray() {
        MemoryTestHelper.AssertDifference(() => testDataArray_Shuffled.OrderBy(static x => x.Value3).ThenBy(static x => x.Value2).ThenBy(static x => x.Value).ToArray(),
            new[] {
                ($"{typeof(TestData).FullName}[]", 1),
                ("System.Int32[]", 3),
            }
        );
    }
    [Test]
    public static void Array_Select_OrderBy_ToArray() {
        MemoryTestHelper.AssertDifference(() => testDataArray_Shuffled.Select(static x => new TestStruct(x.Value)).OrderBy(static x => x.Value).ToArray(),
            new[] {
                ($"{typeof(TestStruct).FullName}[]", 2),
                ("System.Int32[]", 1),
            }
        );
    }
    [Test]
    public static void Array_Where_OrderBy_ToArray() {
        MemoryTestHelper.AssertDifference(() => testDataArray_Shuffled.Where(static x => x.Value % 3 == 0).OrderBy(static x => x.Value).ToArray(),
            new[] {
                ($"{typeof(TestData).FullName}[]", 2),
                ("System.Int32[]", 1),
            }
        );
    }
    //[Test]
    //public static void Array_Where_OrderBy_ToArray_Standard() {
    //    MemoryTestHelper.AssertDifference(() => Enumerable.ToArray(Enumerable.OrderBy(Enumerable.Where(testDataArray_Shuffled, static x => x.Value % 3 == 0), static x => x.Value)),
    //        new[] {
    //            ($"{typeof(TestData).FullName}[]", 2),
    //            ("System.Int32[]", 2),
    //        }
    //    );
    //}
    #endregion

    #region where
    [Test]
    public static void Array_Where_ToDictionary() {
        MemoryTestHelper.AssertDifference(() => intArray.Where(static x => x < 3).ToDictionary(x => x * 10), ExpectedDictionaryOfIntsAllocations());
    }
    [Test]
    public static void Array_Where_ToHashSet() {
        MemoryTestHelper.AssertDifference(() => intArray.Where(static x => x < 3).ToHashSet(), ExpectedHashSetOfIntsAllocations());
    }
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
    public static void CustomCollection_Where_ToArray() {
        MemoryTestHelper.AssertDifference(() => intCustomCollection.Where(static x => x < 3).ToArray(), ExpectedArrayOfIntsAllocations());
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
    public static void Array_Select_ToDictionary() {
        MemoryTestHelper.AssertDifference(() => intArray.Select(static x => x * 2).ToDictionary(x => x * 10), ExpectedDictionaryOfIntsAllocations()
        );
    }

    [Test]
    public static void Array_Select_ToHashSet() {
        MemoryTestHelper.AssertDifference(() => intArray.Select(static x => x * 2).ToHashSet(), ExpectedHashSetOfIntsAllocations()
        );
    }

    [Test]
    public static void Array_Select_ToArray() {
        MemoryTestHelper.AssertDifference(() => intArray.Select(static x => x * 2).ToArray(), ExpectedArrayOfIntsAllocations());
    }
    [Test]
    public static void Array_Select_Foreach() {
        MemoryTestHelper.AssertDifference(() => {
            int sum = 0;
            foreach(int item in intArray.Select(static x => x * 10)) {
                sum += item;
            }
            AssertValue(100, sum);
        }, null);
    }
    [Test]
    public static void List_Select_ToArray() {
        MemoryTestHelper.AssertDifference(() => intList.Select(static x => x * 2).ToArray(), ExpectedArrayOfIntsAllocations());
    }
    [Test]
    public static void List_Select_Foreach() {
        MemoryTestHelper.AssertDifference(() => {
            int sum = 0;
            foreach(int item in intList.Select(static x => x * 10)) {
                sum += item;
            }
            AssertValue(100, sum);
        }, null);
    }
    #endregion

    static void AssertValue<T>(T expected, T actual) {
        if(!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new InvalidOperationException($"Expected: {expected}, but was {actual}");
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
    static (string, int)[] ExpectedOrderByAllocations() {
        return new[] {
                ($"{typeof(TestData).FullName}[]", 1),
                ("System.Int32[]", 1),
            };
    }
    static (string, int)[] ExpectedHashSetOfIntsAllocations() => new[] {
        ("System.Collections.Generic.HashSet`1[System.Int32]", 1),
        ("System.Collections.Generic.HashSet`1+Entry[System.Int32][]", 1),
        ("System.Int32[]", 1),
    };
    static (string, int)[] ExpectedDictionaryOfIntsAllocations() => new[] {
        ("System.Collections.Generic.Dictionary`2[System.Int32,System.Int32]", 1),
        ("System.Collections.Generic.Dictionary`2+Entry[System.Int32,System.Int32][]", 1),
        ("System.Int32[]", 1),
    };
}
