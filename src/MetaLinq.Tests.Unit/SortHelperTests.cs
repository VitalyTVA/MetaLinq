using MetaLinq.Internal;
using MetaLinq.Tests;
using MetaLinqSpikes;
using MetaLinqTests.Unit;
using System.Buffers;
using System.Numerics;

namespace MetaLinqTests.SortHelperTests;

[TestFixture]
public class SortHelperTests : BaseFixture {
    [Test]
    public void SortHelper_SortToArray() {
        AssertSortMethod(x => SortMethods.ArraySortToArray(x, x => x.Int, descending: false), isStable: true, thenBy: false);
    }
    [Test]
    public void SortHelper_SortToArrayDescending() {
        AssertSortMethod(x => SortMethods.ArraySortToArray(x, x => -x.Int, descending: true), isStable: true, thenBy: false);
    }

    [Test]
    public void SortHelper_SortToArray_ThenBy() {
        AssertSortMethod(x => SortMethods.ArraySortToArray(x, x => x.Long, x => x.Int), isStable: true, thenBy: true);
    }

    public static void AssertSortMethod(Func<Data[], Data[]> sort, bool isStable, bool thenBy) {
        DataExtensions.AssertSortMethod(sort, isStable, CollectionAssert.AreEqual, thenBy);
    }

    [Test]
    public void Log2Test() {
        void AssertLog2(uint num) {
            Assert.AreEqual(BitOperations.Log2(num), SortHelper.Log2(num));
        }

        for(uint i = 0; i < 100; i++) {
            AssertLog2(i);
        }
        uint num = 1;
        for(int i = 0; i < 64; i++) {
            AssertLog2(num);
            num = num << 1;
        }
        var rnd = new Random(0);
        for(int i = 0; i < 100; i++) {
            AssertLog2((uint)rnd.Next(10000000));
            AssertLog2((uint)rnd.Next(1000));
        }
    }
}
