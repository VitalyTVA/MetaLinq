using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MetaLinq.Internal;

public static class SortHelper {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Sort<T, TKey>(T[] source, int[] map, TKey[] sortKeys, bool descending) {
        var len = sortKeys.Length;
        var sorted = SortAndAllocateResultArray<T, TKey>(map, sortKeys, descending); ;
        for(int i = 0; i != len; i++) {
            sorted[i] = source[map[i]];
        }
        return sorted;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Sort<T, TKey>(IList<T> source, int[] map, TKey[] sortKeys, bool descending) {
        var len = sortKeys.Length;
        var sorted = SortAndAllocateResultArray<T, TKey>(map, sortKeys, descending);
        for(int i = 0; i != len; i++) {
            sorted[i] = source[map[i]];
        }
        return sorted;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static T[] SortAndAllocateResultArray<T, TKey>(int[] map, TKey[] sortKeys, bool descending) {
        var len = sortKeys.Length;
        var mapSpan = map.AsSpan().Slice(0, len);
        if(descending)
            ArraySorter<TKey, KeysComparerDescending<TKey>>.IntrospectiveSort(mapSpan, new KeysComparerDescending<TKey>(sortKeys));
        else
            ArraySorter<TKey, KeysComparer<TKey>>.IntrospectiveSort(mapSpan, new KeysComparer<TKey>(sortKeys));
        var sorted = new T[len];
        return sorted;
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

readonly struct KeysComparer<TKey> : IComparer<int> {
    readonly TKey[] keys;

    public KeysComparer(TKey[] keys) {
        this.keys = keys;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    readonly public int Compare(int val1, int val2) {
        var value1 = keys[val1];
        var value2 = keys[val2];
        if(typeof(TKey) == typeof(int)) {
#nullable disable
            return (int)(object)value1 - (int)(object)value2;
#nullable restore
        } else {
            throw new InvalidOperationException();
        }
    }
}

readonly struct KeysComparerDescending<TKey> : IComparer<int> {
    readonly TKey[] keys;

    public KeysComparerDescending(TKey[] keys) {
        this.keys = keys;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    readonly public int Compare(int val1, int val2) {
        var value1 = keys[val1];
        var value2 = keys[val2];
        if(typeof(TKey) == typeof(int)) {
#nullable disable
            return (int)(object)value2 - (int)(object)value1;
#nullable restore
        } else {
            throw new InvalidOperationException();
        }
    }
}

static class ArraySorter<T, TComparer> where TComparer : IComparer<int> {
    static void SwapIfGreater(Span<int> map, TComparer comparer, int i, int j) {
        if(CompareMap(comparer, map[i], map[j]) > 0) {
            int val = map[i];
            map[i] = map[j];
            map[j] = val;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int CompareMap(TComparer comparer, int index1, int index2) {
        if(index1 != index2) {
            int result = comparer.Compare(index1, index2);
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

    internal static void IntrospectiveSort(Span<int> map, TComparer comparer) {
        if(map.Length > 1) {
            IntroSort(map, 2 * (SortHelper.Log2((uint)map.Length) + 1), comparer);
        }
    }

    static void IntroSort(Span<int> map, int depthLimit, TComparer comparer) {
        int num = map.Length;
        while(num > 1) {
            if(num <= 16) {
                switch(num) {
                    case 2:
                        SwapIfGreater(map, comparer, 0, 1);
                        break;
                    case 3:
                        SwapIfGreater(map, comparer, 0, 1);
                        SwapIfGreater(map, comparer, 0, 2);
                        SwapIfGreater(map, comparer, 1, 2);
                        break;
                    default:
                        InsertionSort(map.Slice(0, num), comparer);
                        break;
                }
                break;
            }
            if(depthLimit == 0) {
                HeapSort(map.Slice(0, num), comparer);
                break;
            }
            depthLimit--;
            int num2 = PickPivotAndPartition(map.Slice(0, num), comparer);
            Span<int> span = map;
            IntroSort(span.Slice((num2 + 1), num - (num2 + 1))/*span[(num2 + 1)..num]*/, depthLimit, comparer);
            num = num2;
        }
    }

    static int PickPivotAndPartition(Span<int> map, TComparer comparer) {
        int num = map.Length - 1;
        int num2 = num >> 1;
        SwapIfGreater(map, comparer, 0, num2);
        SwapIfGreater(map, comparer, 0, num);
        SwapIfGreater(map, comparer, num2, num);
        int val = map[num2];
        Swap(map, num2, num - 1);
        int num3 = 0;
        int num4 = num - 1;
        while(num3 < num4) {
            while(CompareMap(comparer, map[++num3], val) < 0) {
            }
            while(CompareMap(comparer, val, map[--num4]) < 0) {
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

    static void HeapSort(Span<int> map, TComparer comparer) {
        int length = map.Length;
        for(int num = length >> 1; num >= 1; num--) {
            DownHeap(map, num, length, comparer);
        }
        for(int num2 = length; num2 > 1; num2--) {
            Swap(map, 0, num2 - 1);
            DownHeap(map, 1, num2 - 1, comparer);
        }
    }

    static void DownHeap(Span<int> map, int i, int n, TComparer comparer) {
        int val = map[i - 1];
        while(i <= n >> 1) {
            int num = 2 * i;
            if(num < n && CompareMap(comparer, map[num - 1], map[num]) < 0) {
                num++;
            }
            if(CompareMap(comparer, val, map[num - 1]) >= 0) {
                break;
            }
            map[i - 1] = map[num - 1];
            i = num;
        }
        map[i - 1] = val;
    }

    static void InsertionSort(Span<int> map, TComparer comparer) {
        for(int i = 0; i < map.Length - 1; i++) {
            int val = map[i + 1];
            int num = i;
            while(num >= 0 && CompareMap(comparer, val, map[num]) < 0) {
                map[num + 1] = map[num];
                num--;
            }
            map[num + 1] = val;
        }
    }
}
