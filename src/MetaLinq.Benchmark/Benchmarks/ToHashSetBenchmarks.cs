﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Linq;

namespace MetaLinqBenchmark;

[SimpleJob(RuntimeMoniker.Net60, warmupCount: 2, targetCount: 10)]
//[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MeanColumn]
[MemoryDiagnoser]
public class Select_ToHashSetBenchmarks {
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
    public void Standard() {
        testData.Select(x => x).ToHashSet();
    }

    [Benchmark]
    public void Meta() {
        var resutl = new HashSet<TestData>(testData.Length);
        var length = testData.Length;
        for(int i = 0; i < length; i++) {
            resutl.Add(testData[i]);
        }
    }
}

[SimpleJob(RuntimeMoniker.Net60, warmupCount: 2, targetCount: 10)]
//[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MeanColumn]
[MemoryDiagnoser]
public class Where_ToHashSetBenchmarks {
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
    public void Standard() {
        testData.Where(x => x.Value % 4 == 0).ToHashSet();
    }

    [Benchmark]
    public void Meta() {
        Func<TestData, bool> predicate = x => x.Value % 4 == 0;
        FillSet(predicate);
    }

    private void FillSet(Func<TestData, bool> predicate) {
        var resutl = new HashSet<TestData>();
        var length = testData.Length;
        for(int i = 0; i < length; i++) {
            var item = testData[i];
            if(predicate(item))
                resutl.Add(item);
        }
    }
}
