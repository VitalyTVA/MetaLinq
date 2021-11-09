using MetaLinq;
using MetaLinq.Tests;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace Spike1 {
    public static class MetaEnumerable {
        public static ArrayWhereEnumerable<TSource> Where<TSource>(this TSource[] source, Func<TSource, bool> predicate)
            => new ArrayWhereEnumerable<TSource>(source, predicate);
    }
    public struct ArrayWhereEnumerable<T> : IEnumerable<T> {
        public readonly T[] source;
        public readonly Func<T, bool> predicate;
        public ArrayWhereEnumerable(T[] source, Func<T, bool> predicate) {
            this.source = source;
            this.predicate = predicate;
        }
        public T[] ToArray() {
            using var result = new LargeArrayBuilder<T>(ArrayPool<T>.Shared, false);
            var len = source.Length;
            for(int i = 0; i < len; i++) {
                var item = source[i];
                if(predicate(item)) {
                    result.Add(item);
                }
            }
            return result.ToArray();
        }
        public struct Enumerator {
            ArrayWhereEnumerable<T> source;
            int index;
            public Enumerator(ArrayWhereEnumerable<T> source) {
                this.source = source;
                index = -1;
            }
            public T Current => source.source[index];
            public bool MoveNext() {
                var len = source.source.Length;
                do {
                    index++;
                    if(source.predicate(source.source[index]))
                        return true;
                } while(index < len);
                return false;
            }
        }
        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            var len = source.Length;
            for(int i = 0; i < len; i++) {
                var item = source[i];
                if(predicate(item)) {
                    yield return item;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }
    }
}

namespace Spike2 {
    static class MetaEnumerable {
        static void Test() {
            var result = Data.Array(10)
                .Where(x => x.Int < 5)
                .Select(x => x.Int)
                .SelectMany<int, List<int>>(x => new List<int> { x, x + 1 })
                .Where(x => x > 2)
                .Select(x => x.ToString());
        }
        public static Array<TSource>.WhereEn Where<TSource>(this TSource[] source, Func<TSource, bool> predicate)
            => new Array<TSource>.WhereEn(source, predicate);
    }
    static partial class Array<T0_Source> {
        public readonly struct SelectEn<T1_Result> {
            readonly T0_Source[] source;
            readonly Func<T0_Source, T1_Result> selector;
            public SelectEn(T0_Source[] source, Func<T0_Source, T1_Result> selector) {
                this.source = source;
                this.selector = selector;
            }
            public WhereEn Where(Func<T1_Result, bool> predicate) => new WhereEn(this, predicate);
            public readonly struct WhereEn {
                readonly SelectEn<T1_Result> source;
                readonly Func<T1_Result, bool> predicate;
                public WhereEn(SelectEn<T1_Result> source, Func<T1_Result, bool> predicate) {
                    this.source = source;
                    this.predicate = predicate;
                }
                public T1_Result[] ToArray() {
                    using var result = new LargeArrayBuilder<T1_Result>(ArrayPool<T1_Result>.Shared, false);
                    int length = source.source.Length;
                    for(int i = 0; i < length; i++) {
                        var itemT0 = source.source[i];
                        var itemT1 = source.selector(itemT0);
                        if(predicate(itemT1)) {
                            result.Add(itemT1);
                        }
                    }
                    return result.ToArray();
                }
            }
        }
        public readonly struct WhereEn {
            readonly T0_Source[] source;
            readonly Func<T0_Source, bool> predicate;
            public WhereEn(T0_Source[] source, Func<T0_Source, bool> predicate) {
                this.source = source;
                this.predicate = predicate;
            }

            public SelectEn<TResult> Select<TResult>(Func<T0_Source, TResult> selector) 
                => new SelectEn<TResult>(this, selector);
            public readonly struct SelectEn<T2_Result> {
                readonly WhereEn source;
                readonly Func<T0_Source, T2_Result> selector;
                public SelectEn(WhereEn source, Func<T0_Source, T2_Result> selector) {
                    this.source = source;
                    this.selector = selector;
                }


                public List.SelectManyEn<TResult> SelectMany<TResult, TResultEnumerable>(Func<T2_Result, List<TResult>> selector) 
                    => new List.SelectManyEn<TResult>(this, selector);

                public static class List {
                    public readonly struct SelectManyEn<T3_Result> {
                        readonly SelectEn<T2_Result> source;
                        readonly Func<T2_Result, List<T3_Result>> selector;
                        public SelectManyEn(SelectEn<T2_Result> source, Func<T2_Result, List<T3_Result>> selector) {
                            this.source = source;
                            this.selector = selector;
                        }

                        public WhereEn Where(Func<T3_Result, bool> predicate)
                            => new WhereEn(this, predicate);
                        public readonly struct WhereEn {
                            readonly SelectManyEn<T3_Result> source;
                            readonly Func<T3_Result, bool> predicate;
                            public WhereEn(SelectManyEn<T3_Result> source, Func<T3_Result, bool> predicate) {
                                this.source = source;
                                this.predicate = predicate;
                            }

                            public SelectEn<TResult> Select<TResult>(Func<T3_Result, TResult> selector)
                                => new SelectEn<TResult>(this, selector);
                            public readonly struct SelectEn<T5_Result> {
                                readonly WhereEn source;
                                readonly Func<T3_Result, T5_Result> selector;
                                public SelectEn(WhereEn source, Func<T3_Result, T5_Result> selector) {
                                    this.source = source;
                                    this.selector = selector;
                                }

                                public T5_Result[] ToArray() {
                                    using var result = new LargeArrayBuilder<T5_Result>(ArrayPool<T5_Result>.Shared, false);
                                    int length = source.source.source.source.source.Length;
                                    for(int i = 0; i < length; i++) {
                                        var itemT0 = source.source.source.source.source[i];
                                        if(source.source.source.source.predicate(itemT0)) {
                                            var itemT1 = source.source.source.selector(itemT0);
                                            var itemT2List = source.source.selector(itemT1);
                                            foreach(var itemT2 in itemT2List) {
                                                if(source.predicate(itemT2)) {
                                                    var itemT4 = selector(itemT2);
                                                    result.Add(itemT4);
                                                }
                                            }
                                        }
                                    }
                                    return result.ToArray();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
