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
		var array = new[] { 4, 3, 6, 2 }.Select(x => new Data { Id = x }).ToArray();
		var res = MetaEnumerable_Spike.OrderBy_Meta(array, x => x.Id).ToArray();
		CollectionAssert.AreEqual(new[] { 2, 3, 4, 6 }, res.Select(x => x.Id).ToArray());
	}

	[Test]
	public void TestAllSortMethods() { 
		void AssertSortMethod(Func<Data[], Data[]> sort) {
            foreach(var size in new[] { 0, 1, 2, 3, 4, 5, 8, 13, 21, 35, 1000 }) {
				var rnd = new Random(0);
                for(int i = 0; i < 3; i++) {
					var array = Enumerable.Repeat(0, size).Select(_ => new Data { Id = rnd.Next(size) }).ToArray();
					CollectionAssert.AreEqual(array.OrderBy(x => x.Id).ToArray(), sort(array));
				}
			}
			AssertSortMethod(x => x.OrderBy__(x => x.Id).ToArray());
			AssertSortMethod(x => MetaEnumerable_Spike.OrderBy_Meta(x, x => x.Id).ToArray());
			AssertSortMethod(x => MetaEnumerable_SpikeWithMap.OrderBy_MetaWithMap(x, x => x.Id).ToArray());
		}
	}
}