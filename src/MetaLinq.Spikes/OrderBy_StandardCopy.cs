using MetaLinq.Internal;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MetaLinqSpikes.OrderBy;

#nullable disable
public static class Order {
    public static OrderedEnumerable<TSource> OrderBy__<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
        return new OrderedEnumerable<TSource, TKey>(source, keySelector, null, descending: false, null);
    }
	public static TSource[] ToArray__<TSource>(this IEnumerable<TSource> source) {
		//if(source == null) {
		//	ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
		//}
		if(!(source is IIListProvider<TSource> iIListProvider)) {
			return EnumerableHelpers.ToArray(source);
		}
		return iIListProvider.ToArray();
	}
    public static TSource First___<TSource>(this OrderedEnumerable<TSource> source) {
        return source.TryGetFirst(out bool _);
    }
}

internal readonly struct Buffer<TElement> {
	internal readonly TElement[] _items;

	internal readonly int _count;

	internal Buffer(IEnumerable<TElement> source) {
		if(source is IIListProvider<TElement> iIListProvider) {
			_count = (_items = iIListProvider.ToArray()).Length;
		} else {
			_items = EnumerableHelpers.ToArray(source, out _count);
		}
	}
}

public abstract class OrderedEnumerable<TElement> : IPartition<TElement>, IIListProvider<TElement>, IEnumerable<TElement>, IEnumerable, IOrderedEnumerable<TElement> {
	internal IEnumerable<TElement> _source;

	public TElement[] ToArray() {
		Buffer<TElement> buffer = new Buffer<TElement>(_source);
		int count = buffer._count;
		if(count == 0) {
			return buffer._items;
		}
		TElement[] array = new TElement[count];
		int[] array2 = SortedMap(buffer);
		for(int i = 0; i != array.Length; i++) {
			array[i] = buffer._items[array2[i]];
		}
		return array;
	}

	public List<TElement> ToList() {
		Buffer<TElement> buffer = new Buffer<TElement>(_source);
		int count = buffer._count;
		List<TElement> list = new List<TElement>(count);
		if(count > 0) {
			int[] array = SortedMap(buffer);
			for(int i = 0; i != count; i++) {
				list.Add(buffer._items[array[i]]);
			}
		}
		return list;
	}

	public int GetCount(bool onlyIfCheap) {
		if(_source is IIListProvider<TElement> iIListProvider) {
			return iIListProvider.GetCount(onlyIfCheap);
		}
		if(onlyIfCheap && !(_source is ICollection<TElement>) && !(_source is ICollection)) {
			return -1;
		}
		return _source.Count();
	}

	internal TElement[] ToArray(int minIdx, int maxIdx) {
		Buffer<TElement> buffer = new Buffer<TElement>(_source);
		int count = buffer._count;
		if(count <= minIdx) {
			return Array.Empty<TElement>();
		}
		if(count <= maxIdx) {
			maxIdx = count - 1;
		}
		if(minIdx == maxIdx) {
			return new TElement[1] { GetEnumerableSorter().ElementAt(buffer._items, count, minIdx) };
		}
		int[] array = SortedMap(buffer, minIdx, maxIdx);
		TElement[] array2 = new TElement[maxIdx - minIdx + 1];
		int num = 0;
		while(minIdx <= maxIdx) {
			array2[num] = buffer._items[array[minIdx]];
			num++;
			minIdx++;
		}
		return array2;
	}

	internal List<TElement> ToList(int minIdx, int maxIdx) {
		Buffer<TElement> buffer = new Buffer<TElement>(_source);
		int count = buffer._count;
		if(count <= minIdx) {
			return new List<TElement>();
		}
		if(count <= maxIdx) {
			maxIdx = count - 1;
		}
		if(minIdx == maxIdx) {
			return new List<TElement>(1) { GetEnumerableSorter().ElementAt(buffer._items, count, minIdx) };
		}
		int[] array = SortedMap(buffer, minIdx, maxIdx);
		List<TElement> list = new List<TElement>(maxIdx - minIdx + 1);
		while(minIdx <= maxIdx) {
			list.Add(buffer._items[array[minIdx]]);
			minIdx++;
		}
		return list;
	}

	internal int GetCount(int minIdx, int maxIdx, bool onlyIfCheap) {
		int count = GetCount(onlyIfCheap);
		if(count <= 0) {
			return count;
		}
		if(count <= minIdx) {
			return 0;
		}
		return ((count <= maxIdx) ? count : (maxIdx + 1)) - minIdx;
	}

    public IPartition<TElement> Skip(int count) {
		return new OrderedPartition<TElement>(this, count, int.MaxValue);
	}

    public IPartition<TElement> Take(int count) {
		return new OrderedPartition<TElement>(this, 0, count - 1);
	}

	public TElement TryGetElementAt(int index, out bool found) {
		if(index == 0) {
			return TryGetFirst(out found);
		}
		if(index > 0) {
			Buffer<TElement> buffer = new Buffer<TElement>(_source);
			int count = buffer._count;
			if(index < count) {
				found = true;
				return GetEnumerableSorter().ElementAt(buffer._items, count, index);
			}
		}
		found = false;
		return default(TElement);
	}

	public TElement TryGetFirst(out bool found) {
		CachingComparer<TElement> comparer = GetComparer();
		using IEnumerator<TElement> enumerator = _source.GetEnumerator();
		if(!enumerator.MoveNext()) {
			found = false;
			return default(TElement);
		}
		TElement val = enumerator.Current;
		comparer.SetElement(val);
		while(enumerator.MoveNext()) {
			TElement current = enumerator.Current;
			if(comparer.Compare(current, cacheLower: true) < 0) {
				val = current;
			}
		}
		found = true;
		return val;
	}

	public TElement TryGetLast(out bool found) {
		using IEnumerator<TElement> enumerator = _source.GetEnumerator();
		if(!enumerator.MoveNext()) {
			found = false;
			return default(TElement);
		}
		CachingComparer<TElement> comparer = GetComparer();
		TElement val = enumerator.Current;
		comparer.SetElement(val);
		while(enumerator.MoveNext()) {
			TElement current = enumerator.Current;
			if(comparer.Compare(current, cacheLower: false) >= 0) {
				val = current;
			}
		}
		found = true;
		return val;
	}

	public TElement TryGetLast(int minIdx, int maxIdx, out bool found) {
		Buffer<TElement> buffer = new Buffer<TElement>(_source);
		int count = buffer._count;
		if(minIdx >= count) {
			found = false;
			return default(TElement);
		}
		found = true;
		if(maxIdx >= count - 1) {
			return Last(buffer);
		}
		return GetEnumerableSorter().ElementAt(buffer._items, count, maxIdx);
	}

	private TElement Last(Buffer<TElement> buffer) {
		CachingComparer<TElement> comparer = GetComparer();
		TElement[] items = buffer._items;
		int count = buffer._count;
		TElement val = items[0];
		comparer.SetElement(val);
		for(int i = 1; i != count; i++) {
			TElement val2 = items[i];
			if(comparer.Compare(val2, cacheLower: false) >= 0) {
				val = val2;
			}
		}
		return val;
	}

	protected OrderedEnumerable(IEnumerable<TElement> source) {
		_source = source;
	}

	private int[] SortedMap(Buffer<TElement> buffer) {
		return GetEnumerableSorter().Sort(buffer._items, buffer._count);
	}

	private int[] SortedMap(Buffer<TElement> buffer, int minIdx, int maxIdx) {
		return GetEnumerableSorter().Sort(buffer._items, buffer._count, minIdx, maxIdx);
	}

	public IEnumerator<TElement> GetEnumerator() {
		Buffer<TElement> buffer = new Buffer<TElement>(_source);
		if(buffer._count > 0) {
			int[] map = SortedMap(buffer);
			for(int i = 0; i < buffer._count; i++) {
				yield return buffer._items[map[i]];
			}
		}
	}

	internal IEnumerator<TElement> GetEnumerator(int minIdx, int maxIdx) {
		Buffer<TElement> buffer = new Buffer<TElement>(_source);
		int count = buffer._count;
		if(count <= minIdx) {
			yield break;
		}
		if(count <= maxIdx) {
			maxIdx = count - 1;
		}
		if(minIdx == maxIdx) {
			yield return GetEnumerableSorter().ElementAt(buffer._items, count, minIdx);
			yield break;
		}
		int[] map = SortedMap(buffer, minIdx, maxIdx);
		while(minIdx <= maxIdx) {
			yield return buffer._items[map[minIdx]];
			int num = minIdx + 1;
			minIdx = num;
		}
	}

	private EnumerableSorter<TElement> GetEnumerableSorter() {
		return GetEnumerableSorter(null);
	}

	internal abstract EnumerableSorter<TElement> GetEnumerableSorter(EnumerableSorter<TElement> next);

	private CachingComparer<TElement> GetComparer() {
		return GetComparer(null);
	}

	internal abstract CachingComparer<TElement> GetComparer(CachingComparer<TElement> childComparer);

	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}

	IOrderedEnumerable<TElement> IOrderedEnumerable<TElement>.CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending) {
		return new OrderedEnumerable<TElement, TKey>(_source, keySelector, comparer, descending, this);
	}

	public TElement TryGetLast(Func<TElement, bool> predicate, out bool found) {
		CachingComparer<TElement> comparer = GetComparer();
		using IEnumerator<TElement> enumerator = _source.GetEnumerator();
		while(enumerator.MoveNext()) {
			TElement val = enumerator.Current;
			if(!predicate(val)) {
				continue;
			}
			comparer.SetElement(val);
			while(enumerator.MoveNext()) {
				TElement current = enumerator.Current;
				if(predicate(current) && comparer.Compare(current, cacheLower: false) >= 0) {
					val = current;
				}
			}
			found = true;
			return val;
		}
		found = false;
		return default(TElement);
	}
}

internal sealed class OrderedEnumerable<TElement, TKey> : OrderedEnumerable<TElement> {
	private readonly OrderedEnumerable<TElement> _parent;

	private readonly Func<TElement, TKey> _keySelector;

	private readonly IComparer<TKey> _comparer;

	private readonly bool _descending;

	internal OrderedEnumerable(IEnumerable<TElement> source, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending, OrderedEnumerable<TElement> parent)
		: base(source) {
		//if(source == null) {
		//	ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
		//}
		//if(keySelector == null) {
		//	ThrowHelper.ThrowArgumentNullException(ExceptionArgument.keySelector);
		//}
		_parent = parent;
		_keySelector = keySelector;
		_comparer = comparer ?? Comparer<TKey>.Default;
		_descending = descending;
	}

	internal override EnumerableSorter<TElement> GetEnumerableSorter(EnumerableSorter<TElement> next) {
		IComparer<TKey> comparer = _comparer;
		if(typeof(TKey) == typeof(string) && comparer == Comparer<string>.Default) {
			comparer = (IComparer<TKey>)StringComparer.CurrentCulture;
		}
		EnumerableSorter<TElement> enumerableSorter = new EnumerableSorter<TElement, TKey>(_keySelector, comparer, _descending, next);
		if(_parent != null) {
			enumerableSorter = _parent.GetEnumerableSorter(enumerableSorter);
		}
		return enumerableSorter;
	}

	internal override CachingComparer<TElement> GetComparer(CachingComparer<TElement> childComparer) {
		CachingComparer<TElement> cachingComparer = ((childComparer == null) ? new CachingComparer<TElement, TKey>(_keySelector, _comparer, _descending) : new CachingComparerWithChild<TElement, TKey>(_keySelector, _comparer, _descending, childComparer));
		if(_parent == null) {
			return cachingComparer;
		}
		return _parent.GetComparer(cachingComparer);
	}
}

internal sealed class CachingComparerWithChild<TElement, TKey> : CachingComparer<TElement, TKey> {
	private readonly CachingComparer<TElement> _child;

	public CachingComparerWithChild(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending, CachingComparer<TElement> child)
		: base(keySelector, comparer, descending) {
		_child = child;
	}

	internal override int Compare(TElement element, bool cacheLower) {
		TKey val = _keySelector(element);
		int num = (_descending ? _comparer.Compare(_lastKey, val) : _comparer.Compare(val, _lastKey));
		if(num == 0) {
			return _child.Compare(element, cacheLower);
		}
		if(cacheLower == num < 0) {
			_lastKey = val;
			_child.SetElement(element);
		}
		return num;
	}

	internal override void SetElement(TElement element) {
		base.SetElement(element);
		_child.SetElement(element);
	}
}
public interface IIListProvider<TElement> : IEnumerable<TElement>, IEnumerable {
	TElement[] ToArray();

	List<TElement> ToList();

	int GetCount(bool onlyIfCheap);
}


public interface IPartition<TElement> : IIListProvider<TElement>, IEnumerable<TElement>, IEnumerable {
	IPartition<TElement> Skip(int count);

	IPartition<TElement> Take(int count);

	TElement TryGetElementAt(int index, out bool found);

	TElement TryGetFirst(out bool found);

	TElement TryGetLast(out bool found);
}

internal abstract class EnumerableSorter<TElement> {
	internal abstract void ComputeKeys(TElement[] elements, int count);

	internal abstract int CompareAnyKeys(int index1, int index2);

	private int[] ComputeMap(TElement[] elements, int count) {
		ComputeKeys(elements, count);
		int[] array = new int[count];
		for(int i = 0; i < array.Length; i++) {
			array[i] = i;
		}
		return array;
	}

	internal int[] Sort(TElement[] elements, int count) {
		int[] array = ComputeMap(elements, count);
		QuickSort(array, 0, count - 1);
		return array;
	}

	internal int[] Sort(TElement[] elements, int count, int minIdx, int maxIdx) {
		int[] array = ComputeMap(elements, count);
		PartialQuickSort(array, 0, count - 1, minIdx, maxIdx);
		return array;
	}

	internal TElement ElementAt(TElement[] elements, int count, int idx) {
		int[] map = ComputeMap(elements, count);
		if(idx != 0) {
			return elements[QuickSelect(map, count - 1, idx)];
		}
		return elements[Min(map, count)];
	}

	protected abstract void QuickSort(int[] map, int left, int right);

	protected abstract void PartialQuickSort(int[] map, int left, int right, int minIdx, int maxIdx);

	protected abstract int QuickSelect(int[] map, int right, int idx);

	protected abstract int Min(int[] map, int count);
}

internal sealed class EnumerableSorter<TElement, TKey> : EnumerableSorter<TElement> {
	private readonly Func<TElement, TKey> _keySelector;

	private readonly IComparer<TKey> _comparer;

	private readonly bool _descending;

	private readonly EnumerableSorter<TElement> _next;

	private TKey[] _keys;

	internal EnumerableSorter(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending, EnumerableSorter<TElement> next) {
		_keySelector = keySelector;
		_comparer = comparer;
		_descending = descending;
		_next = next;
	}

	internal override void ComputeKeys(TElement[] elements, int count) {
		_keys = new TKey[count];
		for(int i = 0; i < count; i++) {
			_keys[i] = _keySelector(elements[i]);
		}
		_next?.ComputeKeys(elements, count);
	}

	internal override int CompareAnyKeys(int index1, int index2) {
		int num = _comparer.Compare(_keys[index1], _keys[index2]);
		if(num == 0) {
			if(_next == null) {
				return index1 - index2;
			}
			return _next.CompareAnyKeys(index1, index2);
		}
		if(_descending == num > 0) {
			return -1;
		}
		return 1;
	}

	private int CompareKeys(int index1, int index2) {
		if(index1 != index2) {
			return CompareAnyKeys(index1, index2);
		}
		return 0;
	}

	protected override void QuickSort(int[] keys, int lo, int hi) {
		new Span<int>(keys, lo, hi - lo + 1).Sort(new Comparison<int>(CompareAnyKeys));
	}

	protected override void PartialQuickSort(int[] map, int left, int right, int minIdx, int maxIdx) {
		do {
			int num = left;
			int num2 = right;
			int index = map[num + (num2 - num >> 1)];
			while(true) {
				if(num < map.Length && CompareKeys(index, map[num]) > 0) {
					num++;
					continue;
				}
				while(num2 >= 0 && CompareKeys(index, map[num2]) < 0) {
					num2--;
				}
				if(num > num2) {
					break;
				}
				if(num < num2) {
					int num3 = map[num];
					map[num] = map[num2];
					map[num2] = num3;
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
					PartialQuickSort(map, left, num2, minIdx, maxIdx);
				}
				left = num;
			} else {
				if(num < right) {
					PartialQuickSort(map, num, right, minIdx, maxIdx);
				}
				right = num2;
			}
		}
		while(left < right);
	}

	protected override int QuickSelect(int[] map, int right, int idx) {
		int num = 0;
		do {
			int num2 = num;
			int num3 = right;
			int index = map[num2 + (num3 - num2 >> 1)];
			while(true) {
				if(num2 < map.Length && CompareKeys(index, map[num2]) > 0) {
					num2++;
					continue;
				}
				while(num3 >= 0 && CompareKeys(index, map[num3]) < 0) {
					num3--;
				}
				if(num2 > num3) {
					break;
				}
				if(num2 < num3) {
					int num4 = map[num2];
					map[num2] = map[num3];
					map[num3] = num4;
				}
				num2++;
				num3--;
				if(num2 > num3) {
					break;
				}
			}
			if(num2 <= idx) {
				num = num2 + 1;
			} else {
				right = num3 - 1;
			}
			if(num3 - num <= right - num2) {
				if(num < num3) {
					right = num3;
				}
				num = num2;
			} else {
				if(num2 < right) {
					num = num2;
				}
				right = num3;
			}
		}
		while(num < right);
		return map[idx];
	}

	protected override int Min(int[] map, int count) {
		int num = 0;
		for(int i = 1; i < count; i++) {
			if(CompareKeys(map[i], map[num]) < 0) {
				num = i;
			}
		}
		return map[num];
	}
}

internal abstract class CachingComparer<TElement> {
	internal abstract int Compare(TElement element, bool cacheLower);

	internal abstract void SetElement(TElement element);
}
internal class CachingComparer<TElement, TKey> : CachingComparer<TElement> {
	protected readonly Func<TElement, TKey> _keySelector;

	protected readonly IComparer<TKey> _comparer;

	protected readonly bool _descending;

	protected TKey _lastKey;

	public CachingComparer(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending) {
		_keySelector = keySelector;
		_comparer = comparer;
		_descending = descending;
	}

	internal override int Compare(TElement element, bool cacheLower) {
		TKey val = _keySelector(element);
		int num = (_descending ? _comparer.Compare(_lastKey, val) : _comparer.Compare(val, _lastKey));
		if(cacheLower == num < 0) {
			_lastKey = val;
		}
		return num;
	}

	internal override void SetElement(TElement element) {
		_lastKey = _keySelector(element);
	}
}
internal static class EnumerableHelpers {
	internal static T[] ToArray<T>(IEnumerable<T> source, out int length) {
		if(source is ICollection<T> collection) {
			int count = collection.Count;
			if(count != 0) {
				T[] array = new T[count];
				collection.CopyTo(array, 0);
				length = count;
				return array;
			}
		} else {
			using IEnumerator<T> enumerator = source.GetEnumerator();
			if(enumerator.MoveNext()) {
				T[] array2 = new T[4]
				{
					enumerator.Current,
					default(T),
					default(T),
					default(T)
				};
				int num = 1;
				while(enumerator.MoveNext()) {
					if(num == array2.Length) {
						int num2 = num << 1;
						if((uint)num2 > 2146435071u) {
							num2 = ((2146435071 <= num) ? (num + 1) : 2146435071);
						}
						Array.Resize(ref array2, num2);
					}
					array2[num++] = enumerator.Current;
				}
				length = num;
				return array2;
			}
		}
		length = 0;
		return Array.Empty<T>();
	}

	internal static bool TryGetCount<T>(IEnumerable<T> source, out int count) {
		if(source is ICollection<T> collection) {
			count = collection.Count;
			return true;
		}
		if(source is IIListProvider<T> iIListProvider) {
			return (count = iIListProvider.GetCount(onlyIfCheap: true)) >= 0;
		}
		count = -1;
		return false;
	}

	internal static void Copy<T>(IEnumerable<T> source, T[] array, int arrayIndex, int count) {
		if(source is ICollection<T> collection) {
			collection.CopyTo(array, arrayIndex);
		} else {
			IterativeCopy(source, array, arrayIndex, count);
		}
	}

	internal static void IterativeCopy<T>(IEnumerable<T> source, T[] array, int arrayIndex, int count) {
		int num = arrayIndex + count;
		foreach(T item in source) {
			array[arrayIndex++] = item;
		}
	}

	internal static T[] ToArray<T>(IEnumerable<T> source) {
		if(source is ICollection<T> collection) {
			int count = collection.Count;
			if(count == 0) {
				return Array.Empty<T>();
			}
			T[] array = new T[count];
			collection.CopyTo(array, 0);
			return array;
		}
		LargeArrayBuilder<T> largeArrayBuilder = new LargeArrayBuilder<T>();
        foreach(var item in source) {
			largeArrayBuilder.Add(item);
		}
		//largeArrayBuilder.AddRange(source);
		return largeArrayBuilder.ToArray();
	}
}
internal sealed class OrderedPartition<TElement> : IPartition<TElement>, IIListProvider<TElement>, IEnumerable<TElement>, IEnumerable {
	private readonly OrderedEnumerable<TElement> _source;

	private readonly int _minIndexInclusive;

	private readonly int _maxIndexInclusive;

	public OrderedPartition(OrderedEnumerable<TElement> source, int minIdxInclusive, int maxIdxInclusive) {
		_source = source;
		_minIndexInclusive = minIdxInclusive;
		_maxIndexInclusive = maxIdxInclusive;
	}

	public IEnumerator<TElement> GetEnumerator() {
		return _source.GetEnumerator(_minIndexInclusive, _maxIndexInclusive);
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}

	public IPartition<TElement> Skip(int count) {
		int num = _minIndexInclusive + count;
		if((uint)num <= (uint)_maxIndexInclusive) {
			return new OrderedPartition<TElement>(_source, num, _maxIndexInclusive);
		}
		return EmptyPartition<TElement>.Instance;
	}

	public IPartition<TElement> Take(int count) {
		int num = _minIndexInclusive + count - 1;
		if((uint)num >= (uint)_maxIndexInclusive) {
			return this;
		}
		return new OrderedPartition<TElement>(_source, _minIndexInclusive, num);
	}

	public TElement TryGetElementAt(int index, out bool found) {
		if((uint)index <= (uint)(_maxIndexInclusive - _minIndexInclusive)) {
			return _source.TryGetElementAt(index + _minIndexInclusive, out found);
		}
		found = false;
		return default(TElement);
	}

	public TElement TryGetFirst(out bool found) {
		return _source.TryGetElementAt(_minIndexInclusive, out found);
	}

	public TElement TryGetLast(out bool found) {
		return _source.TryGetLast(_minIndexInclusive, _maxIndexInclusive, out found);
	}

	public TElement[] ToArray() {
		return _source.ToArray(_minIndexInclusive, _maxIndexInclusive);
	}

	public List<TElement> ToList() {
		return _source.ToList(_minIndexInclusive, _maxIndexInclusive);
	}

	public int GetCount(bool onlyIfCheap) {
		return _source.GetCount(_minIndexInclusive, _maxIndexInclusive, onlyIfCheap);
	}
}
[DebuggerDisplay("Count = 0")]
internal sealed class EmptyPartition<TElement> : IPartition<TElement>, IIListProvider<TElement>, IEnumerable<TElement>, IEnumerable, IEnumerator<TElement>, IEnumerator, IDisposable {
	public static readonly IPartition<TElement> Instance = new EmptyPartition<TElement>();

	[ExcludeFromCodeCoverage(Justification = "Shouldn't be called, and as undefined can return or throw anything anyway")]
	public TElement Current => default(TElement);

	[ExcludeFromCodeCoverage(Justification = "Shouldn't be called, and as undefined can return or throw anything anyway")]
	object IEnumerator.Current => null;

	private EmptyPartition() {
	}

	public IEnumerator<TElement> GetEnumerator() {
		return this;
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return this;
	}

	public bool MoveNext() {
		return false;
	}

	void IEnumerator.Reset() {
	}

	void IDisposable.Dispose() {
	}

	public IPartition<TElement> Skip(int count) {
		return this;
	}

	public IPartition<TElement> Take(int count) {
		return this;
	}

	public TElement TryGetElementAt(int index, out bool found) {
		found = false;
		return default(TElement);
	}

	public TElement TryGetFirst(out bool found) {
		found = false;
		return default(TElement);
	}

	public TElement TryGetLast(out bool found) {
		found = false;
		return default(TElement);
	}

	public TElement[] ToArray() {
		return Array.Empty<TElement>();
	}

	public List<TElement> ToList() {
		return new List<TElement>();
	}

	public int GetCount(bool onlyIfCheap) {
		return 0;
	}
}
