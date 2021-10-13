using System;
using System.Linq;
using System.Collections.Generic;

namespace MetaLinqBenchmark {
    static class Standard {
        public static List<int> SelectMany(TestData[] testData) => testData.SelectMany(static x => x.Ints).ToList();
        public static List<int> Select_Where(int[] ints) => ints.Select(static x => x * 10).Where(static x => x % 100 == 0).ToList();
    }
}
