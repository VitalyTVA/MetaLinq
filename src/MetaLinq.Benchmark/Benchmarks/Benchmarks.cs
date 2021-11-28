using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MetaLinq;
using System.Linq;

namespace MetaLinqBenchmark;

[SimpleJob(RuntimeMoniker.Net60, warmupCount: 2, targetCount: 10)]
//[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MeanColumn]
[MemoryDiagnoser]
public class Benchmarks {
    int[] ints = { };
    TestData[] testData = { };

    [Params(100000)]
    public int N;

    [GlobalSetup]
    public void Setup() {
        ints = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Range(0, N));
        testData = new TestData[N];
        for(int i = 0; i < N; i++) {
            testData[i] = new TestData(new[] { i * 10, i * 10 + 1 }, i);
        }
    }

    [Benchmark]
    public int[] SelectMany_Meta() => Meta.SelectMany(testData);
    [Benchmark]
    public int[] SelectMany_Standard() => Standard.SelectMany(testData);
    [Benchmark]
    public int[] SelectMany_AF() => AF.SelectMany(testData);

    [Benchmark]
    public int[] Select_Where_Meta() => Meta.Select_Where(ints);
    [Benchmark]
    public int[] Select_Where_Standard() => Standard.Select_Where(ints);
    [Benchmark]
    public int[] Select_Where_Hyper() => Hyper.Select_Where(ints);
    [Benchmark]
    public int[] Select_Where_AF() => AF.Select_Where(ints);
}
