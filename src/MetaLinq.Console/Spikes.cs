using MetaLinq;
using MetaLinq.Tests;
using System.Buffers;

namespace Spike {
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

        public static Array<TSource>.SelectManyEn_Arrray<TResult> SelectMany<TSource, TResult>(this TSource[] source, Func<TSource, TResult[]> selector)
            => new Array<TSource>.SelectManyEn_Arrray<TResult>(source, selector);
    }
    static partial class Array<TSource> {
        public readonly struct SelectManyEn_Arrray<T0_Result> {
            readonly TSource[] source;
            readonly Func<TSource, T0_Result[]> selector;
            public SelectManyEn_Arrray(TSource[] source, Func<TSource, T0_Result[]> selector) {
                this.source = source;
                this.selector = selector;
            }
            public T0_Result[] ToArray() {
                using var result = new LargeArrayBuilder<T0_Result>(ArrayPool<T0_Result>.Shared, false);
                var source = this;
                var source0 = this.source;
                var len0 = source0.Length;
                for(int i0 = 0; i0 < len0; i0++) {
                    var item0 = source0[i0];
                    var source1 = source.selector(item0);
                    var len1 = source1.Length;
                    for(int i1 = 0; i1 < len1; i1++) {
                        var item1 = source1[i1];
                        result.Add(item1);
                    }
                }
                return result.ToArray();
            }
            //#nullable disable
            //                public struct Enumerator : IEnumerator<T0_Result> {
            //                    WhereEn source;
            //                    int index;
            //                    T0_Result current;
            //                    public Enumerator(WhereEn source) {
            //                        this.source = source;
            //                        index = -1;
            //                        current = default;
            //                    }
            //                    public T0_Result Current => current;
            //                    public bool MoveNext() {
            //                        var rootSource = this.source.source.source;
            //                        var len = rootSource.Length;
            //                        while(true) {
            //                            index++;
            //                            if(index >= len)
            //                                break;
            //                            var item0 = rootSource[index];
            //                            var item1 = source.source.selector(item0);
            //                            var item2 = item1;
            //                            if(source.predicate(item2)) {
            //                                current = item2;
            //                                return true;
            //                            }
            //                        }
            //                        return false;
            //                    }
            //                    public void Dispose() { }
            //                    public void Reset() { }
            //                    object IEnumerator.Current => throw new NotImplementedException();
            //                }
            //#nullable restore
            //                public Enumerator GetEnumerator() => new Enumerator(this);
            //                IEnumerator<T0_Result> IEnumerable<T0_Result>.GetEnumerator() {
            //                    return new Enumerator(this);
            //                }
            //                IEnumerator IEnumerable.GetEnumerator() {
            //                    throw new NotImplementedException();
            //                }
        }

        public readonly struct SelectEn<T0_Result> {
            readonly TSource[] source;
            readonly Func<TSource, T0_Result> selector;
            public SelectEn(TSource[] source, Func<TSource, T0_Result> selector) {
                this.source = source;
                this.selector = selector;
            }
            public WhereEn Where(Func<T0_Result, bool> predicate) => new WhereEn(this, predicate);
            public readonly struct WhereEn : IEnumerable<T0_Result> {
                readonly SelectEn<T0_Result> source;
                readonly Func<T0_Result, bool> predicate;
                public WhereEn(SelectEn<T0_Result> source, Func<T0_Result, bool> predicate) {
                    this.source = source;
                    this.predicate = predicate;
                }
                public T0_Result[] ToArray() {
                    using var result = new LargeArrayBuilder<T0_Result>(ArrayPool<T0_Result>.Shared, false);
                    var rootSource = this.source.source;
                    var len = rootSource.Length;
                    var source = this;
                    for(int i = 0; i < len; i++) {
                        var item0 = rootSource[i];
                        var item1 = source.source.selector(item0);
                        var item2 = item1;
                        if(source.predicate(item2)) {
                            result.Add(item2);
                        }
                    }
                    return result.ToArray();
                }
#nullable disable
                public struct Enumerator : IEnumerator<T0_Result> {
                    WhereEn source;
                    int index;
                    T0_Result current;
                    public Enumerator(WhereEn source) {
                        this.source = source;
                        index = -1;
                        current = default;
                    }
                    public T0_Result Current => current;
                    public bool MoveNext() {
                        var rootSource = this.source.source.source;
                        var len = rootSource.Length;
                        while(true) {
                            index++;
                            if(index >= len)
                                break;
                            var item0 = rootSource[index];
                            var item1 = source.source.selector(item0);
                            var item2 = item1;
                            if(source.predicate(item2)) {
                                current = item2;
                                return true;
                            }
                        }
                        return false;
                    }
                    public void Dispose() { }
                    public void Reset() { }
                    object IEnumerator.Current => throw new NotImplementedException();
                }
#nullable restore
                public Enumerator GetEnumerator() => new Enumerator(this);
                IEnumerator<T0_Result> IEnumerable<T0_Result>.GetEnumerator() {
                    return new Enumerator(this);
                }
                IEnumerator IEnumerable.GetEnumerator() {
                    throw new NotImplementedException();
                }
            }
        }
        public readonly struct WhereEn {
            readonly TSource[] source;
            readonly Func<TSource, bool> predicate;
            public WhereEn(TSource[] source, Func<TSource, bool> predicate) {
                this.source = source;
                this.predicate = predicate;
            }

            public SelectEn<TResult> Select<TResult>(Func<TSource, TResult> selector) 
                => new SelectEn<TResult>(this, selector);
            public readonly struct SelectEn<T2_Result> {
                readonly WhereEn source;
                readonly Func<TSource, T2_Result> selector;
                public SelectEn(WhereEn source, Func<TSource, T2_Result> selector) {
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
