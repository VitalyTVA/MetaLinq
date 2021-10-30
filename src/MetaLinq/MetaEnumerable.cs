using System;
using System.Buffers;
namespace MetaLinq {
    //public static class MetaEnumerable {
    //    public static ArrayWhereEnumerable<TSource> Where<TSource>(this TSource[] source, Func<TSource, bool> predicate)
    //        => new ArrayWhereEnumerable<TSource>(source, predicate);
    //}
    //public struct ArrayWhereEnumerable<T> {
    //    public readonly T[] source;
    //    public readonly Func<T, bool> predicate;
    //    public ArrayWhereEnumerable(T[] source, Func<T, bool> predicate) {
    //        this.source = source;
    //        this.predicate = predicate;
    //    }
    //    public T[] ToArray() {
    //        using var result = new LargeArrayBuilder<T>(ArrayPool<T>.Shared, false);
    //        var len = source.Length;
    //        for(int i = 0; i < len; i++) {
    //            var item = source[i];
    //            if(predicate(item)) {
    //                result.Add(item);
    //            }
    //        }
    //        return result.ToArray();
    //    }
    //}
}
