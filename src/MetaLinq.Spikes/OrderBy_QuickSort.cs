namespace MetaLinqSpikes;

public static partial class MetaEnumerable_Spike {
	public static Meta_Array_Spike<TSource>.OrderByEn0_Int OrderBy_Meta<TSource>(TSource[] source, Func<TSource, int> keySelector)
		=> new Meta_Array_Spike<TSource>.OrderByEn0_Int(source, keySelector);
}
public static partial class Meta_Array_Spike<TSource> {
	public readonly struct OrderByEn0_Int {
		readonly TSource[] source;
		readonly Func<TSource, int> keySelector;
		public OrderByEn0_Int(TSource[] source, Func<TSource, int> keySelector) {
			this.source = source;
			this.keySelector = keySelector;
		}
		public TSource[] ToArray() {
			var source = this;
			var source0 = this.source;
			var len0 = source0.Length;

			var sorted = new TSource[source0.Length];
			Array.Copy(source0, sorted, sorted.Length);
			var sortKeys = new int[source0.Length];

			for(int i = 0; i < len0; i++) {
				sortKeys[i] = keySelector(source0[i]);
			}
			PartialQuickSort(sortKeys, sorted, 0, len0 - 1, 0, len0 - 1);
			return sorted;


			//for(int i0 = 0; i0 < len0; i0++) {
			//	var item0 = source0[i0];
			//	var item1 = item0;
			//	//if(!source.predicate(item1))
			//	//	continue;
			//	result.Add(item1);
			//}
			//return result.ToArray();
		}
		static Comparison<int> _Comparison = (x1, x2) => x2 - x1;
		static int CompareKeys(int[] keys, int index1, int index2) {
			if(index1 != index2) {
				return keys[index1] - keys[index2];
				//return _Comparison(keys[index1], keys[index2]);
			}
			return 0;
		}
        //internal override int CompareAnyKeys(int index1, int index2) {
        //	int num = _comparer.Compare(_keys[index1], _keys[index2]);
        //	if(num == 0) {
        //		if(_next == null) {
        //			return index1 - index2;
        //		}
        //		return _next.CompareAnyKeys(index1, index2);
        //	}
        //	if(_descending == num > 0) {
        //		return -1;
        //	}
        //	return 1;
        //}
        static void PartialQuickSort(int[] keys, TSource[] array, int left, int right, int minIdx, int maxIdx) {
			do {
				int num = left;
				int num2 = right;
				int index = num + (num2 - num >> 1);
				while(true) {
					if(num < keys.Length && CompareKeys(keys, index, num) > 0) {
						num++;
						continue;
					}
					while(num2 >= 0 && CompareKeys(keys, index, num2) < 0) {
						num2--;
					}
					if(num > num2) {
						break;
					}
					if(num < num2) {
						TSource num3 = array[num];
						array[num] = array[num2];
						array[num2] = num3;
					}
					num++;
					num2--;
					if(num > num2) {
						break;
					}
				}
				if(minIdx >= num) {
					left = num + 1;
				} else if(maxIdx <= num2) {
					right = num2 - 1;
				}
				if(num2 - left <= right - num) {
					if(left < num2) {
						PartialQuickSort(keys, array, left, num2, minIdx, maxIdx);
					}
					left = num;
				} else {
					if(num < right) {
						PartialQuickSort(keys, array, num, right, minIdx, maxIdx);
					}
					right = num2;
				}
			}
			while(left < right);
		}
	}
}

