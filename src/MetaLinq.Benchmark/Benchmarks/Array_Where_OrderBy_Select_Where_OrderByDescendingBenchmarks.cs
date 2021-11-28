using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Linq;

namespace MetaLinqBenchmark;

[SimpleJob(RuntimeMoniker.Net60, warmupCount: 2, targetCount: 10)]
//[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MeanColumn]
[MemoryDiagnoser]
public class Array_Where_OrderBy_Select_Where_OrderByDescendingBenchmarks {
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
    public int[] Array_Where_OrderBy_Select_Where_OrderByDescending_Standard() => Standard.Where_OrderBy_Select_Where_OrderByDescending(testData);

    [Benchmark]
    public int[] Array_Where_OrderBy_Select_Where_OrderByDescending_Meta() => Meta.Where_OrderBy_Select_Where_OrderByDescending(testData);

    //[Benchmark]
    //public TestData[] WhereOrderBy_AF() => AF.Where_OrderBy(testData);

    //[Benchmark]
    //public TestData[] WhereOrderBy_Hyper() => Hyper.Where_OrderBy(testData);
}
