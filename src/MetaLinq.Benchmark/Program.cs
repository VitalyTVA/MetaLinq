using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using En = System.Linq.Enumerable;

namespace MetaLinq.Benchmark {
    class Program {
        static void Main(string[] args) {
            var summary = BenchmarkRunner.Run<Select_Where_ToList>();
        }
    }

    //[SimpleJob(RuntimeMoniker.Net50, warmupCount: 1, targetCount: 5)]
    [SimpleJob(RunStrategy.Throughput, RuntimeMoniker.Net50)]
    //[MinColumn, MaxColumn, MeanColumn, MedianColumn]
    [MeanColumn]
    [MemoryDiagnoser]
    public class Select_Where_ToList {
        private int[] data;

        [Params(5)]
        public int N;

        [GlobalSetup]
        public void Setup() {
            data = En.ToArray(En.Range(0, N));
            for(int i = 0; i < N; i++) {
                data[i] = i;
            }
        }

        [Benchmark]
        public List<int> Meta() => data.Select(static x => x * 10).Where(static x => x % 100 == 0).ToList();

        [Benchmark]
        public List<int> Standard() => En.ToList(En.Where(En.Select(data, static x => x * 10), static x => x % 100 == 0));
    }

}
