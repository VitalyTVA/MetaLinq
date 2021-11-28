using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Linq;

namespace MetaLinqBenchmark;

[SimpleJob(RuntimeMoniker.Net60, warmupCount: 2, targetCount: 10)]
//[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MeanColumn]
[MemoryDiagnoser]
public class Select_OrderByBenchmarks {
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
    public int[] SelectOrderBy_Standard() => Standard.Select_OrderBy(testData);

    [Benchmark]
    public int[] SelectOrderBy_Meta() => Meta.Select_OrderBy(testData);

    //[Benchmark]
    //public int[] SelectOrderBy_AF() => AF.Select_OrderBy(testData);

    //[Benchmark]
    //public TestData[] SelectOrderBy_Hyper() => Hyper.Select_OrderBy(testData);
}
