using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MetaLinq.Internal;

public ref struct ExactSizeOrderByArrayBuilder<T, TKey> {
    readonly bool descending;
    int index;
    readonly TKey[] sortKeys;
    readonly int[] map;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ExactSizeOrderByArrayBuilder(int len, bool descending) {
        index = 0;
        this.descending = descending;
        sortKeys = new TKey[len];
        map = ArrayPool<int>.Shared.Rent(len);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T[] ToArray(T[] source) {
        var len = source.Length;
        T[] sorted = SortAndAllocateResultArray(len);
        for(int i = 0; i != len; i++) {
            sorted[i] = source[map[i]];
        }
        return sorted;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T[] ToArray(IList<T> source) {
        var len = source.Count;
        T[] sorted = SortAndAllocateResultArray(len);
        for(int i = 0; i != len; i++) {
            sorted[i] = source[map[i]];
        }
        return sorted;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    readonly T[] SortAndAllocateResultArray(int len) {
        var mapSpan = map.AsSpan().Slice(0, len);
        if(descending)
            ArraySorter<TKey, NoComparerDescending<TKey>>.IntrospectiveSort(mapSpan, sortKeys, default);
        else
            ArraySorter<TKey, NoComparer<TKey>>.IntrospectiveSort(mapSpan, sortKeys, default);
        var sorted = new T[len];
        return sorted;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(TKey value) {
        sortKeys[index] = value;
        map[index] = index;
        index++;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() {
        Debug.Assert(index == sortKeys.Length);
        ArrayPool<int>.Shared.Return(map);
    }
}
public static class SortHelper {
    public static TSource[] ArraySortToArray<TSource, TKey>(TSource[] source, Func<TSource, TKey> keySelector, bool descending) {
        using var builder = new ExactSizeOrderByArrayBuilder<TSource, TKey>(source.Length, descending);
        var len = source.Length;
        for(int i = 0; i < len; i++) {
            builder.Add(keySelector(source[i]));
        }
        return builder.ToArray(source);
    }
    public static TSource[] ListSortToArray<TSource, TKey>(IList<TSource> source, Func<TSource, TKey> keySelector, bool descending) {
        using var builder = new ExactSizeOrderByArrayBuilder<TSource, TKey>(source.Count, descending);
        var len = source.Count;
        for(int i = 0; i < len; i++) {
            builder.Add(keySelector(source[i]));
        }
        return builder.ToArray(source);
    }
    public static int Log2(uint value) {
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        return Unsafe.AddByteOffset(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(Log2DeBruijn), (IntPtr)(int)(value * 130329821 >> 27));
    }
    static ReadOnlySpan<byte> Log2DeBruijn => new byte[32] {
        0, 9, 1, 10, 13, 21, 2, 29, 11, 14,
        16, 18, 22, 25, 3, 30, 8, 12, 20, 28,
        15, 17, 24, 7, 19, 27, 23, 6, 26, 5,
        4, 31
    };
}

readonly struct NoComparer<T> : IComparer<T> {
    readonly public int Compare(T? val1, T? val2) {
        throw new NotImplementedException();
    }
}
readonly struct NoComparerDescending<T> : IComparer<T> {
    readonly public int Compare(T? val1, T? val2) {
        throw new NotImplementedException();
    }
}
static class ArraySorter<T, TComparer> where TComparer : IComparer<T> {
    private static void SwapIfGreater(Span<int> map, Span<T> keys, TComparer comparer, int i, int j) {
        if(CompareMap(comparer, map[i], map[j], keys) > 0) {
            int val = map[i];
            map[i] = map[j];
            map[j] = val;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int CompareMap(TComparer comparer, int index1, int index2, Span<T> keys) {
        if(index1 != index2) {
            var value1 = keys[index1];
            var value2 = keys[index2];
            int result;
            if(typeof(TComparer) == typeof(NoComparer<T>)) {
                if(typeof(T) == typeof(int)) {
#nullable disable
                    result = (int)(object)value1 - (int)(object)value2;
#nullable restore
                } else {
                    throw new InvalidOperationException();
                }
            } else if(typeof(TComparer) == typeof(NoComparerDescending<T>)) {
                if(typeof(T) == typeof(int)) {
#nullable disable
                    result = (int)(object)value2 - (int)(object)value1;
#nullable restore
                } else {
                    throw new InvalidOperationException();
                }
            } else {
                throw new InvalidOperationException();
            }
            if(result == 0)
                return index1 - index2;
            return result;
        }
        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Swap(Span<int> a, int i, int j) {
        int val = a[i];
        a[i] = a[j];
        a[j] = val;
    }

    internal static void IntrospectiveSort(Span<int> map, Span<T> keys, TComparer comparer) {
        if(map.Length > 1) {
            IntroSort(map, keys, 2 * (SortHelper.Log2((uint)map.Length) + 1), comparer);
        }
    }

    static void IntroSort(Span<int> map, Span<T> keys, int depthLimit, TComparer comparer) {
        int num = map.Length;
        while(num > 1) {
            if(num <= 16) {
                switch(num) {
                    case 2:
                        SwapIfGreater(map, keys, comparer, 0, 1);
                        break;
                    case 3:
                        SwapIfGreater(map, keys, comparer, 0, 1);
                        SwapIfGreater(map, keys, comparer, 0, 2);
                        SwapIfGreater(map, keys, comparer, 1, 2);
                        break;
                    default:
                        InsertionSort(map.Slice(0, num), keys, comparer);
                        break;
                }
                break;
            }
            if(depthLimit == 0) {
                HeapSort(map.Slice(0, num), keys, comparer);
                break;
            }
            depthLimit--;
            int num2 = PickPivotAndPartition(map.Slice(0, num), keys, comparer);
            Span<int> span = map;
            IntroSort(span.Slice((num2 + 1), num - (num2 + 1))/*span[(num2 + 1)..num]*/, keys, depthLimit, comparer);
            num = num2;
        }
    }

    static int PickPivotAndPartition(Span<int> map, Span<T> keys, TComparer comparer) {
        int num = map.Length - 1;
        int num2 = num >> 1;
        SwapIfGreater(map, keys, comparer, 0, num2);
        SwapIfGreater(map, keys, comparer, 0, num);
        SwapIfGreater(map, keys, comparer, num2, num);
        int val = map[num2];
        Swap(map, num2, num - 1);
        int num3 = 0;
        int num4 = num - 1;
        while(num3 < num4) {
            while(CompareMap(comparer, map[++num3], val, keys) < 0) {
            }
            while(CompareMap(comparer, val, map[--num4], keys) < 0) {
            }
            if(num3 >= num4) {
                break;
            }
            Swap(map, num3, num4);
        }
        if(num3 != num - 1) {
            Swap(map, num3, num - 1);
        }
        return num3;
    }

    static void HeapSort(Span<int> map, Span<T> keys, TComparer comparer) {
        int length = map.Length;
        for(int num = length >> 1; num >= 1; num--) {
            DownHeap(map, keys, num, length, comparer);
        }
        for(int num2 = length; num2 > 1; num2--) {
            Swap(map, 0, num2 - 1);
            DownHeap(map, keys, 1, num2 - 1, comparer);
        }
    }

    static void DownHeap(Span<int> map, Span<T> keys, int i, int n, TComparer comparer) {
        int val = map[i - 1];
        while(i <= n >> 1) {
            int num = 2 * i;
            if(num < n && CompareMap(comparer, map[num - 1], map[num], keys) < 0) {
                num++;
            }
            if(CompareMap(comparer, val, map[num - 1], keys) >= 0) {
                break;
            }
            map[i - 1] = map[num - 1];
            i = num;
        }
        map[i - 1] = val;
    }

    static void InsertionSort(Span<int> map, Span<T> keys, TComparer comparer) {
        for(int i = 0; i < map.Length - 1; i++) {
            int val = map[i + 1];
            int num = i;
            while(num >= 0 && CompareMap(comparer, val, map[num], keys) < 0) {
                map[num + 1] = map[num];
                num--;
            }
            map[num + 1] = val;
        }
    }
}
