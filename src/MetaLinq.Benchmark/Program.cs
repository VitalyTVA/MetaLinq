using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using MetaLinq;
using System.Linq;

namespace MetaLinqBenchmark;

class Program {
    static void Main(string[] args) {
        BenchmarkRunner.Run<OrderByBenchmarks>();
        //BenchmarkRunner.Run<Benchmarks>();
    }
}

[SimpleJob(RuntimeMoniker.Net60, warmupCount: 2, targetCount: 10)]
//[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MeanColumn]
[MemoryDiagnoser]
public class OrderByBenchmarks {
    TestData[] testData = { };

    [Params(10, 100, 1_000, 10_000)]
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

    [Benchmark]
    public TestData[] OrderBy_MetaWithMap() => Meta.OrderBy_WithMap(testData);

    [Benchmark]
    public TestData[] OrderBy_AF() => AF.OrderBy(testData);

    //[Benchmark]
    //public TestData[] OrderBy_Hyper() => Hyper.OrderBy(testData);

}

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
public class TestData {
    public TestData(int[] ints, int value) {
        Ints = ints;
        Value = value;
    }
    public int[] Ints { get; }
    public int Value;
}
