namespace MetaLinqSpikes;

public static partial class MetaEnumerable_SpikeWithMap {
	public static Meta_Array_SpikeWithMap<TSource>.OrderByEn0_Int OrderBy_MetaWithMap<TSource>(TSource[] source, Func<TSource, int> keySelector)
		=> new Meta_Array_SpikeWithMap<TSource>.OrderByEn0_Int(source, keySelector);

}
public static partial class Meta_Array_SpikeWithMap<TSource> {
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

			var sortKeys = new int[source0.Length];
			var map = new int[source0.Length];

			for(int i = 0; i < len0; i++) {
				sortKeys[i] = keySelector(source0[i]);
			}
			int Comparison(int index1, int index2) {
                if(index1 != index2) {
                    return sortKeys[index1] - sortKeys[index2];
                }
                return 0;
			}
			new Span<int>(map, 0, len0).Sort(Comparison);
			var sorted = new TSource[source0.Length];
			for(int i = 0; i != len0; i++) {
				sorted[i] = source0[map[i]];
			}
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
			}
			return 0;
		}
	}
}
