using System.Buffers;

namespace MetaLinq;

public static class MetaEnumerable {
    //public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
    //    throw NotImplemented();
    //}
    //public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source) {
    //    throw NotImplemented();
    //}

    //public struct WhereResult<TSource> : IEnumerable<TSource> {
    //    IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator() {
    //        throw new NotImplementedException();
    //    }
    //    IEnumerator IEnumerable.GetEnumerator() {
    //        throw new NotImplementedException();
    //    }
    //}
    //public static WhereResult<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
    //    throw NotImplemented();
    //}

    //public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source) {
    //    throw NotImplemented();
    //}

    //public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector) {
    //    throw NotImplemented();
    //}

    //static NotImplementedException NotImplemented() => new NotImplementedException();
}

public static class MetaEnumerable_Generated {
    public readonly struct SelectManyResult<TSource, TSourceEnumerable, TResult, TResultEnumerable> : IEnumerable<TResult>
        where TResultEnumerable : IEnumerable<TResult>
        where TSourceEnumerable : IEnumerable<TSource> {
        internal readonly IEnumerable<TSource> source;
        internal readonly Func<TSource, TResultEnumerable> selector;

        public SelectManyResult(IEnumerable<TSource> source, Func<TSource, TResultEnumerable> selector) {
            this.source = source;
            this.selector = selector;
        }

        IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator() {
            throw new NotImplementedException();
        }
        IEnumerator IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }
    }
    public static SelectManyResult<TSource, TSource[], TResult, TResult[]> SelectMany_Meta<TSource, TResult>(this TSource[] source, Func<TSource, TResult[]> selector) {
        return new SelectManyResult<TSource, TSource[], TResult, TResult[]>(source, selector);
    }
    public static SelectManyResult<TSource, List<TSource>, TResult, TResult[]> SelectMany_Meta<TSource, TResult>(this List<TSource> source, Func<TSource, TResult[]> selector) {
        return new SelectManyResult<TSource, List<TSource>, TResult, TResult[]>(source, selector);
    }
    public static SelectManyResult<TSource, TSource[], TResult, List<TResult>> SelectMany_Meta<TSource, TResult>(this TSource[] source, Func<TSource, List<TResult>> selector) {
        return new SelectManyResult<TSource, TSource[], TResult, List<TResult>>(source, selector);
    }



    public static List<TResult> ToLis_Meta<TSource, TSourceEnumerable, TResult, TResultEnumerable>(this SelectManyResult<TSource, TSourceEnumerable, TResult, TResultEnumerable> source)
        where TResultEnumerable : IEnumerable<TResult>
        where TSourceEnumerable : IEnumerable<TSource> {
        //var result = new List<TResult>();
        using var result = new LargeArrayBuilder<TResult>(ArrayPool<TResult>.Shared, false);
        if(source.source is TSource[] array) {
            var len = array.Length;
            for(int i = 0; i < len; i++) {
                var item = array[i];
                var selectResult = source.selector(item);
                if(selectResult is TResult[] resultArray) {
                    var len2 = resultArray.Length;
                    for(int j = 0; j < len2; j++) {
                        var item2 = resultArray[j];
                        result.Add(item2);
                    }
                } else {
                    throw new NotImplementedException();
                }
            }
        } else if(source.source is List<TSource> list) {
            foreach(var item in list) {
                var selectResult = source.selector(item);
                if(selectResult is TResult[] resultArray) {
                    var len2 = resultArray.Length;
                    for(int j = 0; j < len2; j++) {
                        var item2 = resultArray[j];
                        result.Add(item2);
                    }
                } else {
                    throw new NotImplementedException();
                }
            }
        } else {
            throw new NotImplementedException();
        }
        //return result;
        return result.ToArray().AsList();
    }
}
