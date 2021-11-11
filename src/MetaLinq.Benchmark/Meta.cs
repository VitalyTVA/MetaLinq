using System.Linq;
using MetaLinq;

namespace MetaLinqBenchmark;

static class Meta {
    public static List<int> SelectMany(TestData[] testData) => testData.SelectMany_Meta(static x => x.Ints).ToLis_Meta();
    public static int[] Select_Where(int[] ints) => ints.Select(static x => x * 10).Where(static x => x % 100 == 0).ToArray();
}
