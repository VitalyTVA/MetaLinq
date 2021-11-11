namespace MetaLinq {
    class Program {
        static void Main(string[] args) {
            //MemoryProfiler.CollectAllocations(false);
            int[] data = data = Enumerable.ToArray(Enumerable.Range(0, 5));
            //GetResultMeta(data);
            //GetResultMeta(data);
            GetResultStandard(data);
            GetResultStandard(data);
            //MemoryProfiler.CollectAllocations(true);
            GetResultStandard(data);
            //MemoryProfiler.GetSnapshot();

            var testData = new[] {
                new TestData { Ints = new [] { 1, 2 } },
                new TestData { Ints = new [] { 3, 4 } },
            };
            var res = testData.SelectMany_Meta(x => x.Ints).ToLis_Meta();
            var res2 = testData.SelectMany_Meta(x => x.IntList);
        }

        class TestData {
            public int[] Ints { get; set; } = { };
            public List<int> IntList => Ints.ToList();
        }

        //static List<int> GetResultMeta(int[] data) {
        //    return data.Select_Meta(static x => x * 10).Where_Meta(static x => x % 100 == 0).ToList_Meta();
        //}
        static List<int> GetResultStandard(int[] data) {
            return Enumerable.ToList(Enumerable.Where(Enumerable.Select(data, static x => x * 10), static x => x % 100 == 0));
        }

    }
}
