using NetFabric.Hyperlinq;

namespace MetaLinqBenchmark;

static class Hyper {
    //public static List<int> SelectMany(TestData[] testData) => testData.AsValueEnumerable().SelectMany(static x => x.Ints).ToList();
    public static int[] Select_Where(int[] ints) => ints.AsValueEnumerable().Select(static x => x * 10).Where(static x => x % 100 == 0).ToArray();
}
