using System;
using System.Collections.Generic;
using LinqAF;

namespace MetaLinqBenchmark {
    static class AF {
        public static List<int> SelectMany(TestData[] testData) => testData.SelectMany(static x => x.Ints).ToList();
        public static int[] Select_Where(int[] ints) => ints.Select(static x => x * 10).Where(static x => x % 100 == 0).ToArray();
    }
}
