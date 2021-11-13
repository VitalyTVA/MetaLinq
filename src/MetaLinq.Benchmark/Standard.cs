using System.Linq;

namespace MetaLinqBenchmark;

static class Standard {
    public static int[] SelectMany(TestData[] testData) => testData.SelectMany(static x => x.Ints).ToArray();
    public static int[] Select_Where(int[] ints) => ints.Select(static x => x * 10).Where(static x => x % 100 == 0).ToArray();
}
