using MetaLinq.Internal;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace MetaLinqSpikes;

public static partial class SortMethods {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Sort<T, TKey>(T[] source, int[] map, TKey[] sortKeys, bool descending) {
        var len = sortKeys.Length;

        T[] sorted;
        if(descending)
            sorted = SortHelper.Sort(source, map, new KeysComparerDescending<TKey>(sortKeys), len);
        else
            sorted = SortHelper.Sort(source, map, new KeysComparer<TKey>(sortKeys), len);

        return sorted;
    }

    public static TSource[] ArraySortToArray<TSource, TKey>(TSource[] source, Func<TSource, TKey> keySelector, bool descending) {
        var (result, sortKeys, map) = (source, new TKey[source.Length], ArrayPool<int>.Shared.Rent(source.Length));
        var len = source.Length;
        for(int i = 0; i < len; i++) {
            var item = source[i];
            sortKeys[i] = keySelector(item);
            map[i] = i;
        }
        ArrayPool<int>.Shared.Return(map);
        return Sort(result, map, sortKeys, descending);
    }

    public static TSource[] ArraySortToArray<TSource, TKey1, TKey2>(TSource[] source, Func<TSource, TKey1> keySelector1, Func<TSource, TKey2> keySelector2) {
        var (keys1, keys2, map) = (new TKey1[source.Length], new TKey2[source.Length], ArrayPool<int>.Shared.Rent(source.Length));
        var len = source.Length;
        for(int i = 0; i < len; i++) {
            var item = source[i];
            keys1[i] = keySelector1(item);
            keys2[i] = keySelector2(item);
            map[i] = i;
        }
        ArrayPool<int>.Shared.Return(map);

        return SortHelper.Sort(
            source,
            map, 
            new KeysComparer<TKey1, KeysComparer<TKey2>>(keys1, new KeysComparer<TKey2>(keys2)), 
            len
        );
    }

    public static TSource[] ArraySortToArray<TSource, TKey1, TKey2, TKey3>(TSource[] source, Func<TSource, TKey1> keySelector1, Func<TSource, TKey2> keySelector2, Func<TSource, TKey3> keySelector3) {
        var (keys1, keys2, keys3, map) = (new TKey1[source.Length], new TKey2[source.Length], new TKey3[source.Length], ArrayPool<int>.Shared.Rent(source.Length));
        var len = source.Length;
        for(int i = 0; i < len; i++) {
            var item = source[i];
            keys1[i] = keySelector1(item);
            keys2[i] = keySelector2(item);
            keys3[i] = keySelector3(item);
            map[i] = i;
        }
        ArrayPool<int>.Shared.Return(map);

        return SortHelper.Sort(
            source,
            map,
            new KeysComparer<TKey1, KeysComparer<TKey2, KeysComparer<TKey3>>>(keys1, new KeysComparer<TKey2, KeysComparer<TKey3>>(keys2, new KeysComparer<TKey3>(keys3))),
            len
        );
    }

    public static TSource[] Array_Where_ToArray_Fast<TSource, TKey>(TSource[] source, Func<TSource, bool> predicate, Func<TSource, TKey> keySelector) {
        TSource[] result1;
        {
            using var result = new LargeArrayBuilder<TSource>();
            var source0 = source;
            var len0 = source0.Length;
            for(int i0 = 0; i0 < len0; i0++) {
                var item0 = source0[i0];
                var item1 = item0;
                if(!predicate(item1))
                    continue;
                var item2 = keySelector(item1);
                result.Add(item1);
            }
            result1 = result.ToArray();
        }
        {
            var len0 = result1.Length;
            var sortKeys = new TKey[len0];
            var map = new int[len0];
            for(int i0 = 0; i0 < len0; i0++) {
                var item0 = result1[i0];
                var item1 = keySelector(item0);
                sortKeys[i0] = item1;
                map[i0] = i0;
            }
            return Sort(result1, map, sortKeys, descending: false);
        }
    }

    public static TSource[] Sort_Map_Comparer<TSource>(TSource[] source, Func<TSource, int> keySelector) {
        var len0 = source.Length;

        var sortKeys = new int[source.Length];
        var map = new int[source.Length];

        for(int i = 0; i < len0; i++) {
            sortKeys[i] = keySelector(source[i]);
            map[i] = i;
        }
        new Span<int>(map, 0, len0).Sort(new KeysComparer(sortKeys));
        var sorted = new TSource[source.Length];
        for(int i = 0; i != len0; i++) {
            sorted[i] = source[map[i]];
        }
        return sorted;
    }
    readonly struct KeysComparer : IComparer<int> { 
        readonly int[] keys;
        public KeysComparer(int[] keys) {
            this.keys = keys;
        }
        readonly public int Compare(int index1, int index2) {
            if(index1 != index2) {
                var result = keys[index1] - keys[index2];
                if(result == 0)
                    return index1 - index2;
                return result;
            }
            return 0;
        }
    }

    public static TSource[] Sort_ArraySortHelper_TComparer<TSource>(TSource[] source, Func<TSource, int> keySelector) {
        var len0 = source.Length;

        var sortKeys = new int[source.Length];
        var map = new int[source.Length];

        for(int i = 0; i < len0; i++) {
            sortKeys[i] = keySelector(source[i]);
            map[i] = i;
        }

        GenericArraySortHelper<int, KeysComparer>.Default.Sort(map, new KeysComparer(sortKeys));

        var sorted = new TSource[source.Length];
        for(int i = 0; i != len0; i++) {
            sorted[i] = source[map[i]];
        }
        return sorted;
    }
    public static TSource[] Sort_Map_Comparison<TSource>(TSource[] source, Func<TSource, int> keySelector) {
        var len0 = source.Length;

        var sortKeys = new int[source.Length];
        var map = new int[source.Length];

        for(int i = 0; i < len0; i++) {
            sortKeys[i] = keySelector(source[i]);
            map[i] = i;
        }
        int Comparison(int index1, int index2) {
            if(index1 != index2) {
                var result = sortKeys[index1] - sortKeys[index2];
                if(result == 0)
                    return index1 - index2;
                return result;
            }
            return 0;
        }
        new Span<int>(map, 0, len0).Sort(Comparison);
        var sorted = new TSource[source.Length];
        for(int i = 0; i != len0; i++) {
            sorted[i] = source[map[i]];
        }
        return sorted;
    }
    public static TSource[] Sort_Direct_Comparer<TSource>(TSource[] source, Func<TSource, int> keySelector) {
        var len0 = source.Length;

        var sorted = new TSource[source.Length];
        Array.Copy(source, sorted, sorted.Length);
        var sortKeys = new int[source.Length];

        for(int i = 0; i < len0; i++) {
            sortKeys[i] = keySelector(source[i]);
        }
        var span = new Span<int>(sortKeys);
        span.Sort<int, TSource, Comparer<int>>(sorted, Comparer<int>.Default);
        //PartialQuickSort(sortKeys, sorted, 0, len0 - 1, 0, len0 - 1);
        return sorted;
    }
	public static TSource[] Sort_Direct_Comparison<TSource>(TSource[] source, Func<TSource, int> keySelector) {
		var len0 = source.Length;

		var sorted = new TSource[source.Length];
		Array.Copy(source, sorted, sorted.Length);
		var sortKeys = new int[source.Length];

		for(int i = 0; i < len0; i++) {
			sortKeys[i] = keySelector(source[i]);
		}
        int Comparison(int value1, int value2) {
            return value1 - value2;
        }
        var span = new Span<int>(sortKeys);
        span.Sort<int, TSource>(sorted, Comparison);
        //PartialQuickSort(sortKeys, sorted, 0, len0 - 1, 0, len0 - 1);
        return sorted;
    }

	//public static TSource[] Sort_Direct_QuickSort<TSource>(TSource[] source, Func<TSource, int> keySelector) {
	//	var len0 = source.Length;

	//	var sorted = new TSource[source.Length];
	//	Array.Copy(source, sorted, sorted.Length);
	//	var sortKeys = new int[source.Length];

	//	for(int i = 0; i < len0; i++) {
	//		sortKeys[i] = keySelector(source[i]);
	//	}
	//	PartialQuickSort(sortKeys, sorted, 0, len0 - 1, 0, len0 - 1);
	//	return sorted;
	//}

 //   static void PartialQuickSort<TSource>(int[] keys, TSource[] array, int left, int right, int minIdx, int maxIdx) {
 //       do {
 //           int num = left;
 //           int num2 = right;
 //           int index = num + (num2 - num >> 1);
 //           while(true) {
 //               if(num < keys.Length && CompareKeys(keys, index, num) > 0) {
 //                   num++;
 //                   continue;
 //               }
 //               while(num2 >= 0 && CompareKeys(keys, index, num2) < 0) {
 //                   num2--;
 //               }
 //               if(num > num2) {
 //                   break;
 //               }
 //               if(num < num2) {
 //                   TSource num3 = array[num];
 //                   array[num] = array[num2];
 //                   array[num2] = num3;
 //               }
 //               num++;
 //               num2--;
 //               if(num > num2) {
 //                   break;
 //               }
 //           }
 //           if(minIdx >= num) {
 //               left = num + 1;
 //           } else if(maxIdx <= num2) {
 //               right = num2 - 1;
 //           }
 //           if(num2 - left <= right - num) {
 //               if(left < num2) {
 //                   PartialQuickSort(keys, array, left, num2, minIdx, maxIdx);
 //               }
 //               left = num;
 //           } else {
 //               if(num < right) {
 //                   PartialQuickSort(keys, array, num, right, minIdx, maxIdx);
 //               }
 //               right = num2;
 //           }
 //       }
 //       while(left < right);
 //   }
 //   static int CompareKeys(int[] keys, int index1, int index2) {
 //       if(index1 != index2) {
 //           return keys[index1] - keys[index2];
 //           //return _Comparison(keys[index1], keys[index2]);
 //       }
 //       return 0;
 //   }
}
