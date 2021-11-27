using MetaLinq.Internal;
using System.Runtime.CompilerServices;

namespace MetaLinqSpikes;

readonly struct NoComparer<T> : IComparer<T> {
    readonly public int Compare(T? val1, T? val2) {
        if(typeof(T) == typeof(int)) {
#nullable disable
            return (int)(object)val1 - (int)(object)val2;
#nullable restore
        } else {
            throw new InvalidOperationException();
        }
    }
}
readonly struct NoComparerDescending<T> : IComparer<T> {
    readonly public int Compare(T? val1, T? val2) {
        if(typeof(T) == typeof(int)) {
#nullable disable
            return (int)(object)val2 - (int)(object)val1;
#nullable restore
        } else {
            throw new InvalidOperationException();
        }
    }
}
static class ArraySorterNoComparer<T, TComparer> where TComparer : IComparer<T> {
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
            int result = comparer.Compare(value1, value2);
            //            int result;
            //            if(typeof(TComparer) == typeof(NoComparer<T>)) {
            //                if(typeof(T) == typeof(int)) {
            //#nullable disable
            //                    result = (int)(object)value1 - (int)(object)value2;
            //#nullable restore
            //                } else {
            //                    throw new InvalidOperationException();
            //                }
            //            } else if(typeof(TComparer) == typeof(NoComparerDescending<T>)) {
            //                if(typeof(T) == typeof(int)) {
            //#nullable disable
            //                    result = (int)(object)value2 - (int)(object)value1;
            //#nullable restore
            //                } else {
            //                    throw new InvalidOperationException();
            //                }
            //            } else {
            //                throw new InvalidOperationException();
            //            }
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
