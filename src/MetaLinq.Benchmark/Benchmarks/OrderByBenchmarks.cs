using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MetaLinq;
using System.Linq;

namespace MetaLinqBenchmark;

[SimpleJob(RuntimeMoniker.Net60, warmupCount: 2, targetCount: 10)]
//[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MeanColumn]
[MemoryDiagnoser]
public class OrderByBenchmarks {
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
    public TestData[] OrderBy_Standard() => Standard.OrderBy(testData);

    [Benchmark]
    public TestData[] OrderBy_Meta() => Meta.OrderBy(testData);

    //[Benchmark]
    //public TestData[] OrderBy_AF() => AF.OrderBy(testData);

    //[Benchmark]
    //public TestData[] OrderBy_Hyper() => Hyper.OrderBy(testData);
}
