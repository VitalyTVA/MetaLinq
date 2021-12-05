using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Linq;

namespace MetaLinqBenchmark;

[SimpleJob(RuntimeMoniker.Net60, warmupCount: 2, targetCount: 10)]
//[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MeanColumn]
[MemoryDiagnoser]
public class Select_ToDictionaryBenchmarks {
    TestData[] testData = { };

    [Params(10, 100, 1000)]
    public int N;

    [GlobalSetup]
    public void Setup() {
        testData = new TestData[N];
        for(int i = 0; i < N; i++) {
            testData[i] = new TestData(new[] { i * 10, i * 10 + 1 }, i);
        }
    }

    [Benchmark(Baseline = true)]
    public void Standard_() {
        Standard.Select_ToHashSet(testData);
    }

    [Benchmark]
    public void Meta_() {
        Meta.Select_ToDictionary(testData);

        //Func<TestData, int> getKey = x => x.Value;

        //DoWork(x => x, getKey);
    }

    //private void DoWork(Func<TestData, TestData> selector, Func<TestData, int> getKey) {
    //    var resutl = new Dictionary<int, TestData>(testData.Length);
    //    var length = testData.Length;
    //    for(int i = 0; i < length; i++) {
    //        var item = testData[i];
    //        var item2 = selector(item);
    //        resutl.Add(getKey(item), item2);
    //    }
    //}
}

[SimpleJob(RuntimeMoniker.Net60, warmupCount: 2, targetCount: 10)]
//[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MeanColumn]
[MemoryDiagnoser]
public class Where_ToDictionaryBenchmarks {
    TestData[] testData = { };

    [Params(10, 100, 1000)]
    public int N;

    [GlobalSetup]
    public void Setup() {
        testData = new TestData[N];
        for(int i = 0; i < N; i++) {
            testData[i] = new TestData(new[] { i * 10, i * 10 + 1 }, i);
        }
    }

    [Benchmark(Baseline = true)]
    public void Standard_() {
        Standard.Where_ToDictionary(testData);
    }

    [Benchmark]
    public void Meta_() {
        Meta.Where_ToDictionary(testData);

        //Func<TestData, bool> predicate = x => x.Value % 4 == 0;
        //FillSet(predicate, x => x.Value);
    }

    //private void FillSet(Func<TestData, bool> predicate, Func<TestData, int> getKey) {
    //    var length = testData.Length;
    //    var resutl = new Dictionary<int, TestData>();
    //    for(int i = 0; i < length; i++) {
    //        var item = testData[i];
    //        if(predicate(item)) {
    //            resutl.Add(getKey(item), item);
    //        }
    //    }
    //}
}
