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
			return SortMethods.Sort_ArraySortHelper_TComparer(source, keySelector);
		}
	}
}

