using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MetaLinq.Internal;

public struct ExactSizeArrayBuilder<T> : IDisposable {
    readonly T[] array;
    int index;
    public ExactSizeArrayBuilder(int length) {
        array = new T[length];
        index = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] ToArray() => array;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T value) => array[index++] = value;

    public void Dispose() { 
        Debug.Assert(index == array.Length);
    }
}
