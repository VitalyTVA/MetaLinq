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
        Int = @int;
        IntArray = intArray;
        IntList = intArray.ToList();
    }
    public int Int { get; }
    public int[] IntArray { get; }
    public List<int> IntList { get; }
}
