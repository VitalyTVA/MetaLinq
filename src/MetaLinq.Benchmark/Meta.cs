﻿using System.Linq;
using MetaLinq;
using MetaLinqSpikes;

namespace MetaLinqBenchmark;

static class Meta {
    public static int[] SelectMany(TestData[] testData) => testData.SelectMany(static x => x.Ints).ToArray();
    public static int[] Select_Where(int[] ints) => ints.Select(static x => x * 10).Where(static x => x % 100 == 0).ToArray();
    public static TestData[] OrderBy(TestData[] testData) => testData.OrderBy(static x => x.Value).ToArray();
}
