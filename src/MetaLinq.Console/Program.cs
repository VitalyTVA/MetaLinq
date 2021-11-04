using System;
using MetaLinq;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Profiler.Api;

namespace MetaLinq {
    class Program {
        static void Main(string[] args) {
            //MemoryProfiler.CollectAllocations(false);
            int[] data = data = Enumerable.ToArray(Enumerable.Range(0, 5));
            GetResultMeta(data);
            GetResultMeta(data);
            GetResultStandard(data);
            GetResultStandard(data);
            //MemoryProfiler.CollectAllocations(true);
            GetResultStandard(data);
            //MemoryProfiler.GetSnapshot();

            var testData = new[] {
                new TestData { Ints = new [] { 1, 2 } },
                new TestData { Ints = new [] { 3, 4 } },
            };
            var res = testData.SelectMany(x => x.Ints).ToList();
            var res2 = testData.SelectMany(x => x.IntList);
        }

        class TestData {
            public int[] Ints { get; set; } = { };
            public List<int> IntList => Ints.ToList();
        }

        static List<int> GetResultMeta(int[] data) {
            return data.Select(static x => x * 10).Where(static x => x % 100 == 0).ToList();
        }
        static List<int> GetResultStandard(int[] data) {
            return Enumerable.ToList(Enumerable.Where(Enumerable.Select(data, static x => x * 10), static x => x % 100 == 0));
        }

    }
}
