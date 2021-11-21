namespace MetaLinq.Tests;

public class Data {
    public static Data[] Array(int count) {
        return IEnumerable(count).ToArray();
    }
    public static List<Data> List(int count) {
        return IEnumerable(count).ToList();
    }
    public static IEnumerable<Data> IEnumerable(int count) {
        return Enumerable.Range(0, count).Select(x => new Data(x, new[] {  2 * x, 2 * x + 1 }, true));
    }

    public Data(int @int) {
        this.@int = @int;
        this.intArray = new int[0];
        intList = intArray.ToList();
        dataList = new List<Data>();
    }
    public Data(int @int, int[] intArray, bool isRoot) {
        this.@int = @int;
        this.intArray = intArray;
        intList = intArray.ToList();
        dataList = isRoot 
            ? intArray.Select(x => new Data(x, new[] { 2 * x, 2 * x + 1 }, false)).ToList()
            : new List<Data>();
    }

    public override string ToString() {
        return "Int: " + @int;
    }

    public int DataList_GetCount { get; private set; }
    readonly List<Data> dataList;
    public List<Data> DataList {
        get {
            DataList_GetCount++;
            return dataList;
        }
    }

    public int Int_GetCount { get; private set; }
    readonly int @int;
    public int Int {
        get {
            Int_GetCount++;
            return @int;
        }
    }

    public int IntArray_GetCount { get; private set; }
    readonly int[] intArray;
    public int[] IntArray {
        get {
            IntArray_GetCount++;
            return intArray;
        }
    }

    public int IntList_GetCount { get; private set; }
    readonly List<int> intList;
    public List<int> IntList {
        get {
            IntList_GetCount++;
            return intList;
        }
    }
}

public static class DataExtensions {
    public static TList Shuffle<TList>(this TList list) where TList : IList<Data> {
        var rnd = new Random(0);
        for(int i = 0; i < list.Count; i++) {
            var i1 = rnd.Next(list.Count);
            var i2 = rnd.Next(list.Count);
            var tmp = list[i1];
            list[i1] = list[i2];
            list[i2] = tmp;
        }
        return list;
    }
    public static void AssertAll(this IEnumerable<Data> source, Action<Data> assertion) {
        foreach(var item in source) {
            assertion(item);
        }
    }

    public static void AssertSortMethod(Func<Data[], Data[]> sort, bool isStable, Action<IEnumerable, IEnumerable> assertion) {
        AssertSort(sort, isStable, new[] { 1, 0, 1 }, assertion);
        foreach(var size in new[] { 0, 1, 2, 3, 4, 5, 8, 13, 21, 35, 1000 }) {
            var rnd = new Random(0);
            for(int i = 0; i < 3; i++) {
                var array = Enumerable.Repeat(0, size).Select(_ => new Data(rnd.Next(size))).ToArray();
                AssertSort(sort, isStable, array, assertion);
            }
        }
    }
    static void AssertSort(Func<Data[], Data[]> sort, bool isStable, int[] array, Action<IEnumerable, IEnumerable> assertion) {
        AssertSort(sort, isStable, array.Select(x => new Data(x)).ToArray(), assertion);
    }
    static void AssertSort(Func<Data[], Data[]> sort, bool isStable, Data[] array, Action<IEnumerable, IEnumerable> assertion) {
        assertion(
            array.OrderBy(x => x.Int).ToArray().Select(x => x.Int).ToArray(),
            sort(array).Select(x => x.Int).ToArray()
        );
        if(isStable) {
            assertion(
                array.OrderBy(x => x.Int).ToArray(),
                sort(array)
            );
        }
    }
}
