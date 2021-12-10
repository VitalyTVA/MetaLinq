using System.Runtime.CompilerServices;

namespace MetaLinq.Internal;

public static class Allocator {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Array<T>(int length) {
        TestTrace.ArrayCreated();
        return new T[length];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Dictionary<TKey, T> Dictionary<TKey, T>() {
        TestTrace.DictionaryCreated();
        return new Dictionary<TKey, T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Dictionary<TKey, T> Dictionary<TKey, T>(int capacity) {
        TestTrace.DictionaryWithCapacityCreated();
        return new Dictionary<TKey, T>(capacity);
    }
}
