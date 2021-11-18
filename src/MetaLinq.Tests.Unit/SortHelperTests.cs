using MetaLinq.Internal;
using MetaLinq.Tests;
using System.Buffers;

namespace MetaLinqTests.SortHelperTests;

[TestFixture]
public class SortHelperTests {
    [Test]
    public void SortHelper_SortToArray() {
        AssertSortMethod(x => SortHelper.SortToArray(x, x => x.Int), isStable: true);
    }

    public static void AssertSortMethod(Func<Data[], Data[]> sort, bool isStable) {
        DataExtensions.AssertSortMethod(sort, isStable, CollectionAssert.AreEqual);
    }
}
