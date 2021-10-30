using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using System;
using MetaLinq;
using System.Collections.Generic;
using System.Linq;

namespace MetaLinqBenchmark {
    class Program {
        static void Main(string[] args) {
            var summary = BenchmarkRunner.Run<Benchmarks>();
        }
    }

    [SimpleJob(RuntimeMoniker.Net50, warmupCount: 2, targetCount: 10)]
    //[MinColumn, MaxColumn, MeanColumn, MedianColumn]
    [MeanColumn]
    [MemoryDiagnoser]
    public class Benchmarks {
        int[] ints;
        TestData[] testData;

        [Params(100000)]
        public int N;

        [GlobalSetup]
        public void Setup() {
            ints = System.Linq.Enumerable.Range(0, N).ToArray();
            testData = new TestData[N];
            for(int i = 0; i < N; i++) {
                testData[i] = new TestData(new[] { i * 10, i * 10 + 1 });
            }
        }

        [Benchmark]
        public List<int> SelectMany_Meta() => Meta.SelectMany(testData);
        [Benchmark]
        public List<int> SelectMany_Standard() => Standard.SelectMany(testData);
        [Benchmark]
        public List<int> SelectMany_AF() => AF.SelectMany(testData);

        [Benchmark]
        public List<int> Select_Where_Meta() => Meta.Select_Where(ints);
        [Benchmark]
        public List<int> Select_Where_Standard() => Standard.Select_Where(ints);
        [Benchmark]
        public List<int> Select_Where_Hyper() => Hyper.Select_Where(ints);
        [Benchmark]
        public List<int> Select_Where_AF() => AF.Select_Where(ints);
    }
    class TestData {
        public TestData(int[] ints) {
            Ints = ints;
        }
        public int[] Ints { get; }
    }
}
