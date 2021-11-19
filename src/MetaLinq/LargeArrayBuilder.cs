﻿using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MetaLinq.Internal;
/// <summary>
/// Helper type for building dynamically-sized arrays while minimizing allocations and copying.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
[StructLayout(LayoutKind.Auto)]
public struct LargeArrayBuilder<T>
    : ICollection<T>
    , IDisposable {
    const int defaultMinCapacity = 4;

    readonly ArrayPool<T> pool;
    readonly bool clearOnDispose;
    readonly int maxCapacity;  // The maximum capacity this builder can have.
    ArrayBuilder<T[]> buffers; // After ResizeLimit * 2, we store previous buffers we've filled out here.
    T[] current;               // Current buffer we're reading into. If _count <= ResizeLimit, this is _first.
    int index;                 // Index into the current buffer.
    int count;                 // Total number of items stored

    /// <summary>
    /// Constructs a new builder.
    /// </summary>
    public LargeArrayBuilder()
        : this(maxCapacity: int.MaxValue, pool: ArrayPool<T>.Shared, arrayBuilderPool: ArrayPool<T[]>.Shared, clearOnDispose: false) {
    }

    /// <summary>
    /// Constructs a new builder with the specified maximum capacity.
    /// </summary>
    /// <param name="maxCapacity">The maximum capacity this builder can have.</param>
    /// <remarks>
    /// Do not add more than <paramref name="maxCapacity"/> items to this builder.
    /// </remarks>
    public LargeArrayBuilder(int maxCapacity, ArrayPool<T> pool, ArrayPool<T[]> arrayBuilderPool, bool clearOnDispose) {
        Debug.Assert(maxCapacity >= 0);
        TestTrace.LargeArrayBuilderCreated();
        this.pool = pool;
        this.clearOnDispose = clearOnDispose;
        this.maxCapacity = maxCapacity;
        buffers = new ArrayBuilder<T[]>(arrayBuilderPool);
        current = Array.Empty<T>();
        index = 0;
        count = 0;
    }

    /// <summary>
    /// Gets the number of items added to the builder.
    /// </summary>
    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    public int Count => count;

    /// <summary>
    /// Adds an item to this builder.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item) {
        Debug.Assert(maxCapacity > count);

        var index = this.index;
        var current = this.current;

        // Must be >= and not == to enable range check elimination
        if((uint)index >= (uint)current.Length) {
            AddWithBufferAllocation(item);
        } else {
            current[index] = item;
            this.index = index + 1;
        }

        count++;
    }

    // Non-inline to improve code quality as uncommon path
    [MethodImpl(MethodImplOptions.NoInlining)]
    void AddWithBufferAllocation(T item) {
        AllocateBuffer();
        current[index++] = item;
    }

    /// <summary>
    /// Copies the contents of this builder to the specified array.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The index in <paramref name="array"/> to start copying to.</param>
    public readonly void CopyTo(T[] array, int arrayIndex) {
        var span = array.AsSpan();
        foreach(var buffer in buffers.AsSpan()) {
            buffer.AsSpan().CopyTo(span);
            span = span.Slice(buffer.Length);
        }
        if(span.Length is not 0)
            current.AsSpan(0, span.Length).CopyTo(span);
    }

    /// <summary>
    /// Creates an array from the contents of this builder.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T[] ToArray() {
        if(count is 0)
            return Array.Empty<T>();

        // ReSharper disable once HeapView.ObjectAllocation.Evident
        var array = Utils.AllocateUninitializedArray<T>(count);
        CopyTo(array, 0);
        return array;
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public readonly Lease<T> ToArray(ArrayPool<T> pool, bool clearOnDispose) {
    //    if(count is 0)
    //        return Lease.Empty<T>();

    //    var result = pool.Lease(count, clearOnDispose);
    //    CopyTo(result.Rented, 0);
    //    return result;
    //}

    [MethodImpl(MethodImplOptions.NoInlining)]
    void AllocateBuffer() {
        Debug.Assert((uint)maxCapacity > (uint)count);
        Debug.Assert(index == current.Length, $"{nameof(AllocateBuffer)} was called, but there's more space.");

        // Example scenario: Let's say _count == 64.
        // Then our buffers look like this: | 8 | 8 | 16 | 32 |
        // As you can see, our count will be just double the last buffer.
        // Now, say _maxCapacity is 100. We will find the right amount to allocate by
        // doing min(64, 100 - 64). The lhs represents double the last buffer,
        // the rhs the limit minus the amount we've already allocated.

        var nextCapacity = defaultMinCapacity;
        if(count is not 0) {
            buffers.Add(current);
            nextCapacity = Math.Min(count, maxCapacity - count);
        }

        current = pool.Rent(nextCapacity);
        index = 0;
    }

    public readonly void Dispose() {
        pool.Return(current, clearOnDispose);
        foreach(var item in buffers.AsSpan())
            pool.Return(item, clearOnDispose);
        buffers.Dispose();
    }

    bool ICollection<T>.IsReadOnly
        => true;

    [ExcludeFromCodeCoverage]
    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
        => throw new NotSupportedException();

    [ExcludeFromCodeCoverage]
    readonly IEnumerator IEnumerable.GetEnumerator()
        => throw new NotSupportedException();

    [ExcludeFromCodeCoverage]
    readonly void ICollection<T>.Add(T item)
        => throw new NotSupportedException();

    [ExcludeFromCodeCoverage]
    readonly void ICollection<T>.Clear()
        => throw new NotSupportedException();

    [ExcludeFromCodeCoverage]
    readonly bool ICollection<T>.Contains(T item)
        => throw new NotSupportedException();

    [ExcludeFromCodeCoverage]
    readonly bool ICollection<T>.Remove(T item)
        => throw new NotSupportedException();
}
public static partial class Utils {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] AllocateUninitializedArray<T>(int count)
#if NET5_0_OR_GREATER
            => GC.AllocateUninitializedArray<T>(count, pinned: false);
#else
        // ReSharper disable once HeapView.ObjectAllocation.Evident
        => new T[count];
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<TSource> AsList<TSource>(TSource[] source) {
        return source switch {
            // ReSharper disable once HeapView.ObjectAllocation.Evident
            { Length: 0 } => new List<TSource>(),
            _ => WrapArray(source)
        };

        static List<TSource> WrapArray(TSource[] source) {
            // ReSharper disable once HeapView.ObjectAllocation.Evident
            var result = new List<TSource>();
            var layout = Unsafe.As<List<TSource>, ListLayout<TSource>>(ref result);
            layout.items = source;
            layout.size = source.Length;
            layout.version = 0;
            result.Capacity = source.Length;
            return result;
        }
    }

    class ListLayout<TSource> {
        public TSource[]? items;

//#if !NETCOREAPP3_0_OR_GREATER
//#pragma warning disable IDE0051 // Remove unused private members
//#pragma warning disable 169
//        readonly object? syncRoot;
//#pragma warning restore 169
//#pragma warning restore IDE0051 // Remove unused private members
//#endif
        public int size;
        public int version;
    }
}
