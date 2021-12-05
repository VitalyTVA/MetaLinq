using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Linq;

namespace MetaLinqBenchmark;

[SimpleJob(RuntimeMoniker.Net60, warmupCount: 2, targetCount: 10)]
//[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MeanColumn]
[MemoryDiagnoser]
public class ListIterationsBenchmarks {
    List<TestData> testData = new();

    [Params(10, 100, 1000, 10000)]
    public int N;

    [GlobalSetup]
    public void Setup() {
        for(int i = 0; i < N; i++) {
            testData.Add(new TestData(new[] { i * 10, i * 10 + 1 }, i));
        }
    }
    [Benchmark(Baseline = true)]
    public int Foreach() {
        int sum = 0;
        foreach(var item in testData) {
            sum += item.Value;
        }
        return sum;
    }

    [Benchmark]
    public int For() {
        int sum = 0;
        var N = testData.Count;
        for(int i = 0; i < N; i++) {
            sum += testData[i].Value;
        }
        return sum;
    }
}