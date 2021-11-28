using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MetaLinq;
using System.Linq;

namespace MetaLinqBenchmark;

[SimpleJob(RuntimeMoniker.Net60, warmupCount: 2, targetCount: 10)]
//[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MeanColumn]
[MemoryDiagnoser]
public class SelectBenchmarks {
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
    public int[] Select_Standard() => Standard.Select(testData);

    [Benchmark]
    public int[] Select_Meta() => Meta.Select(testData);

    [Benchmark]
    public int[] AsFastAsPossible() {
        return ToArray(testData, x => x.Value);
    }
    static TResult[] ToArray<TSource, TResult>(TSource[] _source, Func<TSource, TResult> selector) {
        TResult[] array = new TResult[_source.Length];
        for(int i = 0; i < array.Length; i++) {
            array[i] = selector(_source[i]);
        }
        return array;
    }
    //[Benchmark]
    //public int[] Select_AF() => AF.Select(testData);

    [Benchmark]
    public int[] Select_Hyper() => Hyper.Select(testData);

}
