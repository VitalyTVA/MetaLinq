using System;
using System.Collections.Generic;
using MetaLinq;

namespace MetaLinqBenchmark {
    static class Meta {
        public static List<int> SelectMany(TestData[] testData) => testData.SelectMany_Meta(static x => x.Ints).ToLis_Meta();
        public static List<int> Select_Where(int[] ints) => ints.Select_Meta(static x => x * 10).Where_Meta(static x => x % 100 == 0).ToList_Meta();
    }
}
