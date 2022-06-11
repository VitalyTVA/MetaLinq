using System.Linq;

namespace MetaLinqBenchmark;

static class Standard {
    public static int[] SelectMany(TestData[] testData) => testData.SelectMany(static x => x.Ints).ToArray();
    public static int[] Select(TestData[] testData) => testData.Select(static x => x.Value).ToArray();
    public static int[] Select_Where(int[] ints) => ints.Select(static x => x * 10).Where(static x => x % 100 == 0).ToArray();
    public static TestData[] OrderBy(TestData[] testData) => testData.OrderBy(static x => x.Value).ToArray();
    public static TestData[] OrderBy_ThenBy_ThenBy(TestData[] testData) => testData.OrderBy(static x => x.Value3).ThenBy(static x => x.Value2).ThenBy(static x => x.Value).ToArray();
    public static int[] Select_OrderBy(TestData[] testData) => testData.Select(x => x.Value).OrderBy(static x => -x).ToArray();
    public static TestData[] Where_OrderBy(TestData[] testData) => testData.Where(static x => x.Value % 3 == 0).OrderBy(static x => x.Value).ToArray();
    public static int[] Where_OrderBy_Select_Where_OrderByDescending(TestData[] testData) => testData.Where(x => x.Value > 2).OrderBy(x => x.Value).Select(x => x.Value).Where(x => x > 3).OrderByDescending(x => 2 * x).ToArray();

    public static HashSet<TestData> Where_ToHashSet(TestData[] data) => data.Where(static x => x.Value % 4 == 0).ToHashSet();
    public static HashSet<TestData> Select_ToHashSet(TestData[] data) => data.Select(static x => x).ToHashSet();

    public static Dictionary<int, TestData> Where_ToDictionary(TestData[] data) => data.Where(static x => x.Value % 4 == 0).ToDictionary(static x => x.Value);
    public static Dictionary<int, TestData> Select_ToDictionary(TestData[] data) => data.Select(static x => x).ToDictionary(static x => x.Value);

    public static TestData OrderBy_First(TestData[] testData) => testData.OrderBy(static x => x.Value).First(x => x.Value > testData.Length / 2);
    public static bool OrderBy_Any(TestData[] testData) => testData.OrderBy(static x => x.Value).Any(x => x.Value > testData.Length / 2);
    public static TestData? Where_Last(TestData[] testData) => testData.Where(static x => x.Value % 3 == 0).LastOrDefault(static x => x.Value % 2 == 0);
}
