using System;
using System.Collections.Generic;
using System.Linq;

namespace MetaLinq.Generator {
    public static class Extensions {
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueCreator) {
            TValue value;
            if(!dictionary.TryGetValue(key, out value)) {
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
    }
}
