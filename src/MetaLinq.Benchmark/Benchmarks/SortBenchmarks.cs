using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MetaLinqSpikes;
using System.Linq;

namespace MetaLinqBenchmark;

[SimpleJob(RuntimeMoniker.Net60, warmupCount: 2, targetCount: 10)]
[MeanColumn]
[MemoryDiagnoser]
public class SortBenchmarks {
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
    public void OrderBy_Standard() => Enumerable.ToArray(Enumerable.OrderBy(testData, x => x.Value));

    [Benchmark]
    public void Sort_Map_Comparison() {
        MetaLinqSpikes.SortMethods.Sort_Map_Comparison(testData, x => x.Value);
    }

    [Benchmark]
    public void Sort_ArraySortHelper_TComparer() {
        MetaLinqSpikes.SortMethods.Sort_ArraySortHelper_TComparer(testData, x => x.Value);
    }

    [Benchmark]
    public void SortHelper_SortToArray() {
        SortMethods.ArraySortToArray(testData, x => x.Value, descending: false);
    }


    //[Benchmark]
    //public void Sort_Map_Comparer() {
    //    MetaLinqSpikes.SortMethods.Sort_Map_Comparer(testData, x => x.Value);
    //}

    //[Benchmark]
    //public void Sort_Direct_Comparer() {
    //    MetaLinqSpikes.SortMethods.Sort_Direct_Comparer(testData, x => x.Value);
    //}

    //[Benchmark]
    //public void Sort_Direct_Comparison() {
    //    MetaLinqSpikes.SortMethods.Sort_Direct_Comparison(testData, x => x.Value);
    //}
}
