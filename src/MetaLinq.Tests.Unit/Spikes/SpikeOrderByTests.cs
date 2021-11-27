using MetaLinq.Internal;
using MetaLinq.Tests;
using MetaLinqSpikes;
using MetaLinqSpikes.OrderBy;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MetaLinqTests.SortHelperTests;
[TestFixture]
public class SpikeOrderByTests {
	[Test]
	public void OrderBy_Standard() {
		var source = new[] { 4, 3, 1, 6, 2 }.Select(x => new Data(x)).ToList();
		var en = source.OrderBy(x => x.Int);
		var res = en.ToArray();
		var res2 = en.ToArray();
		CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 6 }, res.Select(x => x.Int).ToArray());
	}

	[Test]
    public void Where_OrderBy_Copied() {
		var source = new[] { 4, 3, 10, 6, 11, 2 }.Select(x => new Data(x)).ToArray();
		var en = source.Where(x => x.Int < 8).OrderBy__(x => x.Int);
        var res = en.ToArray__();
		var res2 = en.ToArray__();
		CollectionAssert.AreEqual(new[] { 2, 3, 4, 6 }, res.Select(x => x.Int).ToArray());
    }
	[Test]
	public void OrderBy_Copied() {
		var source = new[] { 4, 3, 1, 6, 2 }.Select(x => new Data(x)).ToList();
		var en = source.OrderBy__(x => x.Int);
		var res = en.ToArray__().Select(x => x.Int).ToArray();
		CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 6 }, res);
	}

    [Test]
	public void TestCopiedSort() {
		SortHelperTests.AssertSortMethod(x => x.OrderBy__(x => x.Int).ToArray(), isStable: true, thenBy: false);
	}
	[Test]
	public void Sort_Direct_Comparer() {
		SortHelperTests.AssertSortMethod(x => SortMethods.Sort_Direct_Comparer(x, x => x.Int), isStable: false, thenBy: false);
	}
	[Test]
	public void Sort_Direct_Comparison() {
		SortHelperTests.AssertSortMethod(x => SortMethods.Sort_Direct_Comparison(x, x => x.Int), isStable: false, thenBy: false);
	}
	[Test]
	public void Sort_Map_Comparison() {
		SortHelperTests.AssertSortMethod(x => SortMethods.Sort_Map_Comparison(x, x => x.Int), isStable: true, thenBy: false);
	}
	[Test]
	public void Sort_Map_Comparer() {
		SortHelperTests.AssertSortMethod(x => SortMethods.Sort_Map_Comparer(x, x => x.Int), isStable: true, thenBy: false);
	}
	[Test]
	public void Sort_ArraySortHelper_TComparer() {
		SortHelperTests.AssertSortMethod(x => SortMethods.Sort_ArraySortHelper_TComparer(x, x => x.Int), isStable: true, thenBy: false);
	}
	//[Test]
	//public void Sort_Direct_QuickSort() {
	//	AssertSortMethod(x => SortMethods.Sort_Direct_QuickSort(x, x => x.Id));
	//}
}