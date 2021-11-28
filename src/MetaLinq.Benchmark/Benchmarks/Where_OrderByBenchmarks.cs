using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Linq;

namespace MetaLinqBenchmark;

[SimpleJob(RuntimeMoniker.Net60, warmupCount: 2, targetCount: 10)]
//[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MeanColumn]
[MemoryDiagnoser]
public class Where_OrderByBenchmarks {
    TestData[] testData = { };

    [Params(10, 100, 1_000/*, 10_000*/)]
    public int N;

    [GlobalSetup]
    public void Setup() {
        testData = new TestData[N];
        var rnd = new Random(0);
        for(int i = 0; i < N; i++) {
            testData[i] = new TestData(Array.Empty<int>(), rnd.Next(N));
        }
    }

    [Benchmark(Baseline = true)]
    public TestData[] WhereOrderBy_Standard() => Standard.Where_OrderBy(testData);

    [Benchmark]
    public TestData[] WhereOrderBy_Meta() => Meta.Where_OrderBy(testData);

    //[Benchmark]
    //public TestData[] WhereOrderBy_AF() => AF.Where_OrderBy(testData);

    [Benchmark]
    public TestData[] Array_Where_ToArray_Fast() => MetaLinqSpikes.SortMethods.Array_Where_ToArray_Fast(testData, static x => x.Value % 3 == 0, x => x.Value);

    //[Benchmark]
    //public TestData[] WhereOrderBy_Hyper() => Hyper.Where_OrderBy(testData);
}
