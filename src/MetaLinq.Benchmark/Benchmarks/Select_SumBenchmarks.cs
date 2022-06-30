using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MetaLinq;
using System.Linq;

namespace MetaLinqBenchmark;

[SimpleJob(RuntimeMoniker.Net60, warmupCount: 2, targetCount: 10)]
//[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MeanColumn]
[MemoryDiagnoser]
public class Select_SumBenchmarks {
    TestData[] testData = { };

    [Params(100, 10_000)]
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
    public int Standard_() => Standard.Select_Sum(testData);

    [Benchmark]
    public int Meta_() => Meta.Select_Sum(testData);
}
