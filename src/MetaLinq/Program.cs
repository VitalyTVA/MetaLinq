using System;
using System.Collections;
using System.Collections.Generic;

namespace MetaLinq {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            var res = new[] { 1, 2, 3 }.Select(x => x * 10).Where(x => x < 15).ToList();
        }
    }
    static class MetaEnumerable {
        public struct SelectResult<TSource, TResult> : IEnumerable<TResult> {
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
            return new SelectResult<TSource, TResult>();
        }

        public static WhereResult<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            return new WhereResult<TSource>();
        }

        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source) {
            throw NotImplemented();
        }

        static NotImplementedException NotImplemented() => new NotImplementedException();
    }

    static class MetaEnumerable_Generated {
        public struct SelectWhereResult<TSource, TResult> : IEnumerable<TResult> {
            IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator() {
                throw new NotImplementedException();
            }
            IEnumerator IEnumerable.GetEnumerator() {
                throw new NotImplementedException();
            }
        }
        public static SelectWhereResult<T1, TSource> Where<T1, TSource>(this MetaEnumerable.SelectResult<T1, TSource> source, Func<TSource, bool> predicate) {
            return new SelectWhereResult<T1, TSource>();
        }

        public static List<TSource> ToList<T1, TSource>(this SelectWhereResult<T1, TSource> source) {
            return null;
        }
    }
}
