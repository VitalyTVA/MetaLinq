using MetaLinq.Internal;
using System.Buffers;

namespace MetaLinqTests.SortHelperTests;

public class Data {
    public int Id { get; set; }
    public override string ToString() {
        return "Id: " + Id;
    }
}

[TestFixture]
public class SortHelperTests {
    [Test]
    public void SortHelper_SortToArray() {
        AssertSortMethod(x => SortHelper.SortToArray(x, x => x.Id), isStable: true);
    }

    public static void AssertSortMethod(Func<Data[], Data[]> sort, bool isStable) {
        AssertSort(sort, isStable, new[] { 1, 0, 1 });
        foreach(var size in new[] { 0, 1, 2, 3, 4, 5, 8, 13, 21, 35, 1000 }) {
            var rnd = new Random(0);
            for(int i = 0; i < 3; i++) {
                var array = Enumerable.Repeat(0, size).Select(_ => new Data { Id = rnd.Next(size) }).ToArray();
                AssertSort(sort, isStable, array);
            }
        }
    }
    public static void AssertSort(Func<Data[], Data[]> sort, bool isStable, int[] array) {
        AssertSort(sort, isStable, array.Select(x => new Data { Id = x }).ToArray());
    }
    public static void AssertSort(Func<Data[], Data[]> sort, bool isStable, Data[] array) {
        CollectionAssert.AreEqual(
            array.OrderBy(x => x.Id).ToArray().Select(x => x.Id).ToArray(),
            sort(array).Select(x => x.Id).ToArray()
        );
        if(isStable) {
            CollectionAssert.AreEqual(
                array.OrderBy(x => x.Id).ToArray(),
                sort(array)
            );
        }
    }
}
