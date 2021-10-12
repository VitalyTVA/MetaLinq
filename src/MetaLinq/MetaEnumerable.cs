using System;
using System.Collections;
using System.Collections.Generic;

namespace MetaLinq {
    public static class MetaEnumerable {
        public readonly struct SelectResult<TSource, TResult> : IEnumerable<TResult> {
            internal readonly IEnumerable<TSource> source;
            internal readonly Func<TSource, TResult> selector;
            public SelectResult(IEnumerable<TSource> source, Func<TSource, TResult> selector) {
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
        public struct WhereResult<TSource> : IEnumerable<TSource> {
            IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator() {
                throw new NotImplementedException();
            }
            IEnumerator IEnumerable.GetEnumerator() {
                throw new NotImplementedException();
            }
        }
        public static SelectResult<TSource, TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) {
            return new SelectResult<TSource, TResult>(source, selector);
        }

        public static WhereResult<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            throw NotImplemented();
        }

        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source) {
            throw NotImplemented();
        }

        static NotImplementedException NotImplemented() => new NotImplementedException();
    }

    public static class MetaEnumerable_Generated {
        public readonly struct SelectWhereResult<TSource, TResult> : IEnumerable<TResult> {
            internal readonly MetaEnumerable.SelectResult<TSource, TResult> selectResult;
            internal readonly Func<TResult, bool> predicate;
            public SelectWhereResult(MetaEnumerable.SelectResult<TSource, TResult> selectResult, Func<TResult, bool> predicate) {
                this.selectResult = selectResult;
                this.predicate = predicate;
            }

            IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator() {
                throw new NotImplementedException();
            }
            IEnumerator IEnumerable.GetEnumerator() {
                throw new NotImplementedException();
            }
        }
        public static SelectWhereResult<T1, TSource> Where<T1, TSource>(this MetaEnumerable.SelectResult<T1, TSource> source, Func<TSource, bool> predicate) {
            return new SelectWhereResult<T1, TSource>(source, predicate);
        }

        public static List<TSource> ToList<T1, TSource>(this SelectWhereResult<T1, TSource> source) {
            var result = new List<TSource>();
            foreach(var item in source.selectResult.source) {
                var selectResult = source.selectResult.selector(item);
                if(source.predicate(selectResult))
                    result.Add(selectResult);
            }
            return result;
        }
    }
}
