using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MetaLinqSpikes;

internal interface IArraySortHelper<TKey, TComparer> where TComparer : IComparer<TKey> {
	void Sort(Span<TKey> keys, TComparer comparer);

	int BinarySearch(TKey[] keys, int index, int length, TKey value, TComparer comparer);
}


//[TypeDependency("System.Collections.Generic.GenericArraySortHelper`1")]
internal class ArraySortHelper<T, TComparer> : IArraySortHelper<T, TComparer> where TComparer : IComparer<T> {
	private static readonly IArraySortHelper<T, TComparer> s_defaultArraySortHelper = CreateArraySortHelper();

	public static IArraySortHelper<T, TComparer> Default => s_defaultArraySortHelper;

	//[DynamicDependency("#ctor", typeof(GenericArraySortHelper<>))]
	private static IArraySortHelper<T, TComparer> CreateArraySortHelper() {
		if(typeof(IComparable<T>).IsAssignableFrom(typeof(T))) {
			throw new InvalidOperationException();
			//return (IArraySortHelper<T>)RuntimeTypeHandle.Allocate(typeof(GenericArraySortHelper<string>).TypeHandle.Instantiate(new Type[1] { typeof(T) }));
		}
		return new ArraySortHelper<T, TComparer>();
	}

	public void Sort(Span<T> keys, TComparer comparer) {
		try {
			if(comparer == null) {
				throw new InvalidOperationException();
				//comparer = Comparer<T>.Default;
			}
			IntrospectiveSort(keys, comparer);
		} catch(IndexOutOfRangeException) {
			throw new ArgumentException("comparer");
			//ThrowHelper.ThrowArgumentException_BadComparer(comparer);
		} catch(Exception e) {
			throw new InvalidOperationException("ExceptionResource.InvalidOperation_IComparerFailed", e);
			//ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
		}
	}

	public int BinarySearch(T[] array, int index, int length, T value, TComparer comparer) {
		try {
			if(comparer == null) {
				throw new InvalidOperationException();
				//comparer = Comparer<T>.Default;
			}
			return InternalBinarySearch(array, index, length, value, comparer);
		} catch(Exception e) {
			throw new InvalidOperationException("ExceptionResource.InvalidOperation_IComparerFailed", e);
			//ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
			//return 0;
		}
	}

	internal static void Sort__(Span<T> keys, TComparer comparer) {
		try {
			IntrospectiveSort(keys, comparer);
		} catch(IndexOutOfRangeException) {
			throw new ArgumentException("comparer");
			//ThrowHelper.ThrowArgumentException_BadComparer(comparer);
		} catch(Exception e) {
			throw new InvalidOperationException("ExceptionResource.InvalidOperation_IComparerFailed", e);
			//ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
		}
	}

	internal static int InternalBinarySearch(T[] array, int index, int length, T value, IComparer<T> comparer) {
		int num = index;
		int num2 = index + length - 1;
		while(num <= num2) {
			int num3 = num + (num2 - num >> 1);
			int num4 = comparer.Compare(array[num3], value);
			if(num4 == 0) {
				return num3;
			}
			if(num4 < 0) {
				num = num3 + 1;
			} else {
				num2 = num3 - 1;
			}
		}
		return ~num;
	}

	private static void SwapIfGreater(Span<T> keys, TComparer comparer, int i, int j) {
		if(comparer.Compare(keys[i], keys[j]) > 0) {
			T val = keys[i];
			keys[i] = keys[j];
			keys[j] = val;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Swap(Span<T> a, int i, int j) {
		T val = a[i];
		a[i] = a[j];
		a[j] = val;
	}

	internal static void IntrospectiveSort(Span<T> keys, TComparer comparer) {
		if(keys.Length > 1) {
			IntroSort(keys, 2 * (BitOperations.Log2((uint)keys.Length) + 1), comparer);
		}
	}

	private static void IntroSort(Span<T> keys, int depthLimit, TComparer comparer) {
		int num = keys.Length;
		while(num > 1) {
			if(num <= 16) {
				switch(num) {
					case 2:
						SwapIfGreater(keys, comparer, 0, 1);
						break;
					case 3:
						SwapIfGreater(keys, comparer, 0, 1);
						SwapIfGreater(keys, comparer, 0, 2);
						SwapIfGreater(keys, comparer, 1, 2);
						break;
					default:
						InsertionSort(keys.Slice(0, num), comparer);
						break;
				}
				break;
			}
			if(depthLimit == 0) {
				HeapSort(keys.Slice(0, num), comparer);
				break;
			}
			depthLimit--;
			int num2 = PickPivotAndPartition(keys.Slice(0, num), comparer);
			Span<T> span = keys;
			IntroSort(span[(num2 + 1)..num], depthLimit, comparer);
			num = num2;
		}
	}

	private static int PickPivotAndPartition(Span<T> keys, TComparer comparer) {
		int num = keys.Length - 1;
		int num2 = num >> 1;
		SwapIfGreater(keys, comparer, 0, num2);
		SwapIfGreater(keys, comparer, 0, num);
		SwapIfGreater(keys, comparer, num2, num);
		T val = keys[num2];
		Swap(keys, num2, num - 1);
		int num3 = 0;
		int num4 = num - 1;
		while(num3 < num4) {
			while(comparer.Compare(keys[++num3], val) < 0) {
			}
			while(comparer.Compare(val, keys[--num4]) < 0) {
			}
			if(num3 >= num4) {
				break;
			}
			Swap(keys, num3, num4);
		}
		if(num3 != num - 1) {
			Swap(keys, num3, num - 1);
		}
		return num3;
	}

	private static void HeapSort(Span<T> keys, TComparer comparer) {
		int length = keys.Length;
		for(int num = length >> 1; num >= 1; num--) {
			DownHeap(keys, num, length, comparer);
		}
		for(int num2 = length; num2 > 1; num2--) {
			Swap(keys, 0, num2 - 1);
			DownHeap(keys, 1, num2 - 1, comparer);
		}
	}

	private static void DownHeap(Span<T> keys, int i, int n, TComparer comparer) {
		T val = keys[i - 1];
		while(i <= n >> 1) {
			int num = 2 * i;
			if(num < n && comparer.Compare(keys[num - 1], keys[num]) < 0) {
				num++;
			}
			if(comparer.Compare(val, keys[num - 1]) >= 0) {
				break;
			}
			keys[i - 1] = keys[num - 1];
			i = num;
		}
		keys[i - 1] = val;
	}

	private static void InsertionSort(Span<T> keys, TComparer comparer) {
		for(int i = 0; i < keys.Length - 1; i++) {
			T val = keys[i + 1];
			int num = i;
			while(num >= 0 && comparer.Compare(val, keys[num]) < 0) {
				keys[num + 1] = keys[num];
				num--;
			}
			keys[num + 1] = val;
		}
	}
}


internal class GenericArraySortHelper<T, TComparer> : IArraySortHelper<T, TComparer> where T : IComparable<T> where TComparer : IComparer<T> {
	private static readonly IArraySortHelper<T, TComparer> s_defaultArraySortHelper = CreateArraySortHelper();

	public static IArraySortHelper<T, TComparer> Default => s_defaultArraySortHelper;

	private static IArraySortHelper<T, TComparer> CreateArraySortHelper() {
		return new GenericArraySortHelper<T, TComparer>();
	}

	public void Sort(Span<T> keys, TComparer comparer) {
		try {
			if(comparer == null /*|| comparer == Comparer<T>.Default*/) {
				throw new InvalidOperationException();
				//if(keys.Length <= 1) {
				//	return;
				//}
				//if(typeof(T) == typeof(double) || typeof(T) == typeof(float) || typeof(T) == typeof(Half)) {
				//	int num = SortUtils.MoveNansToFront(keys, default(Span<byte>));
				//	if(num == keys.Length) {
				//		return;
				//	}
				//	keys = keys.Slice(num);
				//}
				//IntroSort(keys, 2 * (BitOperations.Log2((uint)keys.Length) + 1));
			} else {
				ArraySortHelper<T, TComparer>.IntrospectiveSort(keys, comparer);
			}
		} catch(IndexOutOfRangeException) {
			throw new ArgumentException("comparer");
			//ThrowHelper.ThrowArgumentException_BadComparer(comparer);
		} catch(Exception e) {
			throw new InvalidOperationException("ExceptionResource.InvalidOperation_IComparerFailed", e);
			//ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
		}
	}

	public int BinarySearch(T[] array, int index, int length, T value, TComparer comparer) {
		try {
			if(comparer == null /*|| comparer == Comparer<T>.Default*/) {
				throw new InvalidOperationException();
				//return BinarySearch(array, index, length, value);
			}
			return ArraySortHelper<T, TComparer>.InternalBinarySearch(array, index, length, value, comparer);
		} catch(Exception e) {
			throw new InvalidOperationException("ExceptionResource.InvalidOperation_IComparerFailed", e);
			//ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
			//return 0;
		}
	}

	private static int BinarySearch(T[] array, int index, int length, T value) {
		int num = index;
		int num2 = index + length - 1;
		while(num <= num2) {
			int num3 = num + (num2 - num >> 1);
			int num4 = ((array[num3] != null) ? array[num3].CompareTo(value) : ((value != null) ? (-1) : 0));
			if(num4 == 0) {
				return num3;
			}
			if(num4 < 0) {
				num = num3 + 1;
			} else {
				num2 = num3 - 1;
			}
		}
		return ~num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SwapIfGreater(ref T i, ref T j) {
		if(i != null && GreaterThan(ref i, ref j)) {
			Swap(ref i, ref j);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Swap(ref T i, ref T j) {
		T val = i;
		i = j;
		j = val;
	}

	private static void IntroSort(Span<T> keys, int depthLimit) {
		int num = keys.Length;
		while(num > 1) {
			if(num <= 16) {
				switch(num) {
					case 2:
						SwapIfGreater(ref keys[0], ref keys[1]);
						break;
					case 3: {
							ref T j = ref keys[2];
							ref T reference = ref keys[1];
							ref T i = ref keys[0];
							SwapIfGreater(ref i, ref reference);
							SwapIfGreater(ref i, ref j);
							SwapIfGreater(ref reference, ref j);
							break;
						}
					default:
						InsertionSort(keys.Slice(0, num));
						break;
				}
				break;
			}
			if(depthLimit == 0) {
				HeapSort(keys.Slice(0, num));
				break;
			}
			depthLimit--;
			int num2 = PickPivotAndPartition(keys.Slice(0, num));
			Span<T> span = keys;
			IntroSort(span[(num2 + 1)..num], depthLimit);
			num = num2;
		}
	}

	private static int PickPivotAndPartition(Span<T> keys) {
		ref T reference = ref MemoryMarshal.GetReference(keys);
		ref T j = ref Unsafe.Add(ref reference, keys.Length - 1);
		ref T reference2 = ref Unsafe.Add(ref reference, keys.Length - 1 >> 1);
		SwapIfGreater(ref reference, ref reference2);
		SwapIfGreater(ref reference, ref j);
		SwapIfGreater(ref reference2, ref j);
		ref T reference3 = ref Unsafe.Add(ref reference, keys.Length - 2);
		T left = reference2;
		Swap(ref reference2, ref reference3);
		ref T reference4 = ref reference;
		ref T reference5 = ref reference3;
		while(Unsafe.IsAddressLessThan(ref reference4, ref reference5)) {
			if(left == null) {
				while(Unsafe.IsAddressLessThan(ref reference4, ref reference3)) {
					ref T reference6 = ref Unsafe.Add(ref reference4, 1);
					reference4 = ref reference6;
					if(reference6 != null) {
						break;
					}
				}
				while(Unsafe.IsAddressGreaterThan(ref reference5, ref reference)) {
					ref T reference7 = ref Unsafe.Add(ref reference5, -1);
					reference5 = ref reference7;
					if(reference7 == null) {
						break;
					}
				}
			} else {
				while(Unsafe.IsAddressLessThan(ref reference4, ref reference3)) {
					ref T reference8 = ref Unsafe.Add(ref reference4, 1);
					reference4 = ref reference8;
					if(!GreaterThan(ref left, ref reference8)) {
						break;
					}
				}
				while(Unsafe.IsAddressGreaterThan(ref reference5, ref reference)) {
					ref T reference9 = ref Unsafe.Add(ref reference5, -1);
					reference5 = ref reference9;
					if(!LessThan(ref left, ref reference9)) {
						break;
					}
				}
			}
			if(!Unsafe.IsAddressLessThan(ref reference4, ref reference5)) {
				break;
			}
			Swap(ref reference4, ref reference5);
		}
		if(!Unsafe.AreSame(ref reference4, ref reference3)) {
			Swap(ref reference4, ref reference3);
		}
		return (int)((nint)Unsafe.ByteOffset(ref reference, ref reference4) / Unsafe.SizeOf<T>());
	}

	private static void HeapSort(Span<T> keys) {
		int length = keys.Length;
		for(int num = length >> 1; num >= 1; num--) {
			DownHeap(keys, num, length, 0);
		}
		for(int num2 = length; num2 > 1; num2--) {
			Swap(ref keys[0], ref keys[num2 - 1]);
			DownHeap(keys, 1, num2 - 1, 0);
		}
	}

	private static void DownHeap(Span<T> keys, int i, int n, int lo) {
		T left = keys[lo + i - 1];
		while(i <= n >> 1) {
			int num = 2 * i;
			if(num < n && (keys[lo + num - 1] == null || LessThan(ref keys[lo + num - 1], ref keys[lo + num]))) {
				num++;
			}
			if(keys[lo + num - 1] == null || !LessThan(ref left, ref keys[lo + num - 1])) {
				break;
			}
			keys[lo + i - 1] = keys[lo + num - 1];
			i = num;
		}
		keys[lo + i - 1] = left;
	}

	private static void InsertionSort(Span<T> keys) {
		for(int i = 0; i < keys.Length - 1; i++) {
			T left = Unsafe.Add(ref MemoryMarshal.GetReference(keys), i + 1);
			int num = i;
			while(num >= 0 && (left == null || LessThan(ref left, ref Unsafe.Add(ref MemoryMarshal.GetReference(keys), num)))) {
				Unsafe.Add(ref MemoryMarshal.GetReference(keys), num + 1) = Unsafe.Add(ref MemoryMarshal.GetReference(keys), num);
				num--;
			}
#nullable disable
			Unsafe.Add(ref MemoryMarshal.GetReference(keys), num + 1) = left;
#nullable restore
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool LessThan(ref T left, ref T right) {
		if(typeof(T) == typeof(byte)) {
			if((byte)(object)left >= (byte)(object)right) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(sbyte)) {
			if((sbyte)(object)left >= (sbyte)(object)right) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(ushort)) {
			if((ushort)(object)left >= (ushort)(object)right) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(short)) {
			if((short)(object)left >= (short)(object)right) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(uint)) {
			if((uint)(object)left >= (uint)(object)right) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(int)) {
			if((int)(object)left >= (int)(object)right) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(ulong)) {
			if((ulong)(object)left >= (ulong)(object)right) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(long)) {
			if((long)(object)left >= (long)(object)right) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(UIntPtr)) {
			if((nuint)(UIntPtr)(object)left >= (nuint)(UIntPtr)(object)right) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(IntPtr)) {
			if((nint)(IntPtr)(object)left >= (nint)(IntPtr)(object)right) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(float)) {
			if(!((float)(object)left < (float)(object)right)) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(double)) {
			if(!((double)(object)left < (double)(object)right)) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(Half)) {
			if(!((Half)(object)left < (Half)(object)right)) {
				return false;
			}
			return true;
		}
		if(left.CompareTo(right) >= 0) {
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool GreaterThan(ref T left, ref T right) {
		if(typeof(T) == typeof(byte)) {
			if((byte)(object)left <= (byte)(object)right) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(sbyte)) {
			if((sbyte)(object)left <= (sbyte)(object)right) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(ushort)) {
			if((ushort)(object)left <= (ushort)(object)right) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(short)) {
			if((short)(object)left <= (short)(object)right) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(uint)) {
			if((uint)(object)left <= (uint)(object)right) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(int)) {
			if((int)(object)left <= (int)(object)right) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(ulong)) {
			if((ulong)(object)left <= (ulong)(object)right) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(long)) {
			if((long)(object)left <= (long)(object)right) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(UIntPtr)) {
			if((nuint)(UIntPtr)(object)left <= (nuint)(UIntPtr)(object)right) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(IntPtr)) {
			if((nint)(IntPtr)(object)left <= (nint)(IntPtr)(object)right) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(float)) {
			if(!((float)(object)left > (float)(object)right)) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(double)) {
			if(!((double)(object)left > (double)(object)right)) {
				return false;
			}
			return true;
		}
		if(typeof(T) == typeof(Half)) {
			if(!((Half)(object)left > (Half)(object)right)) {
				return false;
			}
			return true;
		}
		if(left.CompareTo(right) <= 0) {
			return false;
		}
		return true;
	}
}

#nullable disable
internal static class SortUtils {
	public static int MoveNansToFront<TKey, TValue>(Span<TKey> keys, Span<TValue> values) {
		int num = 0;
		for(int i = 0; i < keys.Length; i++) {
			if((typeof(TKey) == typeof(double) && double.IsNaN((double)(object)keys[i])) || (typeof(TKey) == typeof(float) && float.IsNaN((float)(object)keys[i])) || (typeof(TKey) == typeof(Half) && Half.IsNaN((Half)(object)keys[i]))) {
				TKey val = keys[num];
				keys[num] = keys[i];
				keys[i] = val;
				if((uint)i < (uint)values.Length) {
					TValue val2 = values[num];
					values[num] = values[i];
					values[i] = val2;
				}
				num++;
			}
		}
		return num;
	}
}
#nullable restore


