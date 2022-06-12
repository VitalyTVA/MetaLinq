using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MetaLinq;
using System.Linq;

namespace MetaLinqBenchmark;

[SimpleJob(RuntimeMoniker.Net60, warmupCount: 2, targetCount: 10)]
//[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MeanColumn]
[MemoryDiagnoser]
public class OrderBy_SingleBenchmarks {
    TestData[] testData = { };

    [Params(10, 100, 1_000/*, 10_000*/)]
    public int N;

    [GlobalSetup]
    public void Setup() {
        testData = new TestData[N];
        for(int i = 0; i < N; i++) {
            testData[i] = new TestData(Array.Empty<int>(), i);
        }
    }

    [Benchmark(Baseline = true)]
    public TestData Standard_() => Standard.OrderBy_Single(testData);

    [Benchmark]
    public TestData Meta_() {
        return Meta.OrderBy_Single(testData);
    }
}
