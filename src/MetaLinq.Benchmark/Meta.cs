using System.Linq;
using MetaLinq;
using MetaLinqSpikes;

namespace MetaLinqBenchmark;

static class Meta {
    public static int[] SelectMany(TestData[] testData) => testData.SelectMany(static x => x.Ints).ToArray();
    public static int[] Select_Where(int[] ints) => ints.Select(static x => x * 10).Where(static x => x % 100 == 0).ToArray();
    public static TestData[] OrderBy(TestData[] testData) => MetaEnumerable_Spike.OrderBy_Meta(testData, static x => x.Value).ToArray();
	public static TestData[] OrderBy_WithMap(TestData[] testData) => MetaEnumerable_SpikeWithMap.OrderBy_MetaWithMap(testData, static x => x.Value).ToArray();
}
