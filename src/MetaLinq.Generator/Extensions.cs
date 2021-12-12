namespace MetaLinq.Generator;

public static class Extensions {
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueCreator) {
        if(!dictionary.TryGetValue(key, out TValue value)) {
            value = valueCreator();
            dictionary.Add(key, value);
        }
        return value;
    }
    public static IEnumerable<T> Unfold<T>(T seed, Func<T, T?> next, Func<T?, bool>? stop = null) {
        for(var current = seed; current != null && (stop == null || !stop(current)); current = next(current)) {
            yield return current;
        }
    }
    public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> getItems) {
        return source.Flatten((x, _) => getItems(x));
    }
    struct EnumeratorAndLevel<T> {
        public readonly IEnumerator<T> En;
        public readonly int Level;
        public EnumeratorAndLevel(IEnumerator<T> en, int level) {
            En = en;
            Level = level;
        }
    }
    public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, int, IEnumerable<T>> getItems) {
        //simple and slow:
        //return source.FlattenCore(getItems, 0);

        //complex and fast
        var stack = new Stack<EnumeratorAndLevel<T>>();
        try {
            var root = source.GetEnumerator();
            if(root.MoveNext())
                stack.Push(new EnumeratorAndLevel<T>(root, 0));
            while(stack.Count != 0) {
                var top = stack.Peek();
                var current = top.En.Current;
                yield return current;
                if(!top.En.MoveNext())
                    stack.Pop();

                var children = getItems(current, top.Level)?.GetEnumerator();
                if(children?.MoveNext() == true) {
                    stack.Push(new EnumeratorAndLevel<T>(children, top.Level + 1));
                }
            }
        } finally {
            foreach(var enumAndLevel in stack)
                enumAndLevel.En.Dispose();
        }
    }
    public static int CompareNullable<T>(T? value1, T? value2) where T : struct {
        if(value1 == null && value2 == null)
            return 0;
        if(value1 != null && value2 == null)
            throw new NotImplementedException();
        if(value1 == null && value2 != null)
            throw new NotImplementedException();
        return Comparer<T>.Default.Compare(value1!.Value, value2!.Value);
    }

    public static T[] YieldToArray<T>(this T? item) => item != null ? new[] { item } : Array.Empty<T>();

    //public static IEqualityComparer<T> CreatequalityComparer<T>(Func<T, int> getHashCode, Func<T, T, bool> equals) {
    //    if(getHashCode == null) {
    //        throw new ArgumentNullException(nameof(getHashCode));
    //    }
    //    if(equals == null) {
    //        throw new ArgumentNullException(nameof(equals));
    //    }
    //    return new EqualityComparer<T>(getHashCode, equals);
    //}

    //class EqualityComparer<T> : IEqualityComparer<T> {
    //    private readonly Func<T, int> _getHashCode;
    //    private readonly Func<T, T, bool> _equals;

    //    public EqualityComparer(Func<T, int> getHashCode, Func<T, T, bool> equals) {
    //        _getHashCode = getHashCode;
    //        _equals = equals;
    //    }
    //    public bool Equals(T x, T y) => _equals(x, y);
    //    public int GetHashCode(T obj) => _getHashCode(obj);
    //}
}
