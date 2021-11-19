using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using MetaLinq;
using MetaLinq.Internal;
using System.Linq;

namespace MetaLinqBenchmark;

class Program {
    static void Main(string[] args) {
        BenchmarkRunner.Run<SelectBenchmarks>();
        //BenchmarkRunner.Run<SortBenchmarks>();
        //BenchmarkRunner.Run<OrderByBenchmarks>();
        //BenchmarkRunner.Run<Benchmarks>();
    }
}

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
    public void Sort_ArraySortHelper_TComparer_WithMap() {
        SortHelper.SortToArray(testData, x => x.Value);
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
