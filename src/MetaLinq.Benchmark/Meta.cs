using System;
using System.Collections.Generic;
using MetaLinq;

namespace MetaLinqBenchmark {
    static class Meta {
        public static List<int> SelectMany(TestData[] testData) => testData.SelectMany(static x => x.Ints).ToList();
        public static List<int> Select_Where(int[] ints) => ints.Select(static x => x * 10).Where(static x => x % 100 == 0).ToList();
    }
}
