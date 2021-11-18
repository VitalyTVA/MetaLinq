using MetaLinq.Internal;
using MetaLinq.Tests;
using MetaLinqSpikes;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MetaLinqTests.Unit.Spikes.OrderBy;

[TestFixture]
public class SpikeOrderByTests {
	public class Data {
		public int Id { get; set; }
        public override string ToString() {
            return "Id: " + Id;
        }
    }
	[Test]
	public void OrderBy_Standard() {
		var source = new[] { 4, 3, 1, 6, 2 }.Select(x => new Data { Id = x }).ToList();
		var en = source.OrderBy(x => x.Id);
		var res = en.ToArray();
		var res2 = en.ToArray();
		CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 6 }, res.Select(x => x.Id).ToArray());
	}

	[Test]
    public void Where_OrderBy_Copied() {
		var source = new[] { 4, 3, 10, 6, 11, 2 }.Select(x => new Data { Id = x }).ToArray();
		var en = source.Where(x => x.Id < 8).OrderBy__(x => x.Id);
        var res = en.ToArray__();
		var res2 = en.ToArray__();
		CollectionAssert.AreEqual(new[] { 2, 3, 4, 6 }, res.Select(x => x.Id).ToArray());
    }
	[Test]
	public void OrderBy_Copied() {
		var source = new[] { 4, 3, 1, 6, 2 }.Select(x => new Data { Id = x }).ToList();
		var en = source.OrderBy__(x => x.Id);
		var res = en.ToArray__().Select(x => x.Id).ToArray();
		CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 6 }, res);
	}

    [Test]
    public void OrderBy_Meta() {
		AssertSortMethod(x => MetaEnumerable_Spike.OrderBy_Meta(x, x => x.Id).ToArray(), isStable: true);
    }

    [Test]
	public void TestCopiedSort() { 
		AssertSortMethod(x => x.OrderBy__(x => x.Id).ToArray(), isStable: true);
	}
	[Test]
	public void Sort_Direct_Comparer() {
		AssertSortMethod(x => SortMethods.Sort_Direct_Comparer(x, x => x.Id), isStable: false);
	}
	[Test]
	public void Sort_Direct_Comparison() {
		AssertSortMethod(x => SortMethods.Sort_Direct_Comparison(x, x => x.Id), isStable: false);
	}
	[Test]
	public void Sort_Map_Comparison() {
		AssertSortMethod(x => SortMethods.Sort_Map_Comparison(x, x => x.Id), isStable: true);
	}
	[Test]
	public void Sort_Map_Comparer() {
		AssertSortMethod(x => SortMethods.Sort_Map_Comparer(x, x => x.Id), isStable: true);
	}
	[Test]
	public void Sort_ArraySortHelper_TComparer() {
		AssertSortMethod(x => SortMethods.Sort_ArraySortHelper_TComparer(x, x => x.Id), isStable: true);
	}
	//[Test]
	//public void Sort_Direct_QuickSort() {
	//	AssertSortMethod(x => SortMethods.Sort_Direct_QuickSort(x, x => x.Id));
	//}
	void AssertSortMethod(Func<Data[], Data[]> sort, bool isStable) {
		AssertSort(sort, isStable, new[] { 1, 0 , 1 });
		foreach(var size in new[] { 0, 1, 2, 3, 4, 5, 8, 13, 21, 35, 1000 }) {
			var rnd = new Random(0);
			for(int i = 0; i < 3; i++) {
                var array = Enumerable.Repeat(0, size).Select(_ => new Data { Id = rnd.Next(size) }).ToArray();
                AssertSort(sort, isStable, array);
            }
        }
	}
	static void AssertSort(Func<Data[], Data[]> sort, bool isStable, int[] array) {
		AssertSort(sort, isStable, array.Select(x => new Data { Id = x }).ToArray());
	}
    static void AssertSort(Func<Data[], Data[]> sort, bool isStable, Data[] array) {
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