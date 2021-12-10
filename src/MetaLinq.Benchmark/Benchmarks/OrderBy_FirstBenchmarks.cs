using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MetaLinq;
using System.Linq;

namespace MetaLinqBenchmark;

[SimpleJob(RuntimeMoniker.Net60, warmupCount: 2, targetCount: 10)]
//[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MeanColumn]
[MemoryDiagnoser]
public class OrderBy_FirstBenchmarks {
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
    public TestData Standard_() => Standard.OrderBy_First(testData);

    [Benchmark]
    public TestData Meta_() {
        return Meta.OrderBy_First(testData);

        //Func<TestData, bool> predicate = x => x.Value > testData.Length / 2;
        //Func<TestData, int> getKey = x => x.Value;


        //var result = default(TestData);
        //var minKey = default(int);
        //bool found = false;
        //var length = testData.Length;
        //for(int i = 0; i < length; i++) {
        //    var item = testData[i];
        //    if(predicate(item)) {
        //        var key = getKey(item);
        //        if(!found || key < minKey) {
        //            minKey = key;
        //            result = item;
        //        }
        //        found = true;
        //    }
        //}
        ////if(result != Standard_())
        ////    throw new Exception("Expected " + Standard_().Value + " was " + result!.Value);
        //return result!;
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

    //[Benchmark]
    //public TestData[] OrderBy_AF() => AF.OrderBy(testData);

    //[Benchmark]
    //public TestData[] OrderBy_Hyper() => Hyper.OrderBy(testData);
}
