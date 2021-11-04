using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace MetaLinq {
    //public static class MetaEnumerable {
    //    public static ArrayWhereEnumerable<TSource> Where<TSource>(this TSource[] source, Func<TSource, bool> predicate)
    //        => new ArrayWhereEnumerable<TSource>(source, predicate);
    //}
    //public struct ArrayWhereEnumerable<T> : IEnumerable<T> {
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
    //    public struct Enumerator {
    //        ArrayWhereEnumerable<T> source;
    //        int index;
    //        public Enumerator(ArrayWhereEnumerable<T> source) {
    //            this.source = source;
    //            index = -1;
    //        }
    //        public T Current => source.source[index];
    //        public bool MoveNext() {
    //            var len = source.source.Length;
    //            do {
    //                index++;
    //                if(source.predicate(source.source[index]))
    //                    return true;
    //            } while(index < len);
    //            return false;
    //        }
    //    }
    //    public Enumerator GetEnumerator() => new Enumerator(this);
    //    IEnumerator<T> IEnumerable<T>.GetEnumerator() {
    //        var len = source.Length;
    //        for(int i = 0; i < len; i++) {
    //            var item = source[i];
    //            if(predicate(item)) {
    //                yield return item;
    //            }
    //        }
    //    }
    //    IEnumerator IEnumerable.GetEnumerator() {
    //        throw new NotImplementedException();
    //    }
    //}
}
