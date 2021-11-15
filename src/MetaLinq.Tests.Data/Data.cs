namespace MetaLinq.Tests;

public class Data {
    public static Data[] Array(int count) {
        return IEnumerable(count).ToArray();
    }
    public static List<Data> List(int count) {
        return IEnumerable(count).ToList();
    }
    public static IEnumerable<Data> IEnumerable(int count) {
        return Enumerable.Range(0, count).Select(x => new Data(x, new[] {  2 * x, 2 * x + 1 }));
    }

    public Data(int @int, int[] intArray) {
        this.@int = @int;
        this.intArray = intArray;
        intList = intArray.ToList();
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

public static class DataAssertions {
    public static void AssertAll(this IEnumerable<Data> source, Action<Data> assertion) {
        foreach(var item in source) {
            assertion(item);
        }
    }
}
