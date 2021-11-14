using System;
using System.Collections;
using System.Collections.Generic;
using System.Buffers;
namespace MetaLinq {
    static partial class MetaEnumerable {
        public static Meta_Array<TSource>.SelectManyEn_Meta_Array0<TResult> SelectMany<TSource, TResult>(this TSource[] source, Func<TSource, TResult[]> selector)
            => new Meta_Array<TSource>.SelectManyEn_Meta_Array0<TResult>(source, selector);
    }
    static partial class Meta_Array<TSource> {
        public readonly struct SelectManyEn_Meta_Array0<T0_Result> : IEnumerable<T0_Result> {
            readonly TSource[] source;
            readonly Func<TSource, T0_Result[]> selector;
            public SelectManyEn_Meta_Array0(TSource[] source, Func<TSource, T0_Result[]> selector) {
                this.source = source;
                this.selector = selector;
            }
            #nullable disable
            public struct Enumerator : IEnumerator<T0_Result> {
                TSource[] source;
                Func<TSource, T0_Result[]> selector;

                T0_Result[] nested;
                int i0;
                int i1;
                T0_Result current;
                public Enumerator(SelectManyEn_Meta_Array0<T0_Result> source) {
                    this.source = source.source;
                    this.selector = source.selector;
                    nested = null;
                    i0 = -1;
                    i1 = -1;
                    current = default;
                }
                public T0_Result Current => current;
                public bool MoveNext() {
                    while(true) {
                        if(i1 < 0) {
                            i0++;
                            if(i0 == source.Length)
                                break;
                            nested = selector(source[i0]);
                            i1 = 0;
                        } else {
                            i1++;
                        }
                        if(i1 == nested.Length) {
                            i1 = -1;
                            break;
                        }
                        current = nested[i1];
                        return true;
                    }
                    return false;
                }
                public void Dispose() { }
                public void Reset() { }
                object IEnumerator.Current => throw new NotImplementedException();
            }
            public Enumerator GetEnumerator() => new Enumerator(this);
            IEnumerator<T0_Result> IEnumerable<T0_Result>.GetEnumerator() {
                for(int i0 = 0; i0 < source.Length; i0++) {
                    var source1 = selector(source[i0]);
                    for(int i1 = 0; i1 < source1.Length; i1++) {
                        yield return source1[i1];
                    }
                }

            }
            IEnumerator IEnumerable.GetEnumerator() {
                throw new NotImplementedException();
            }

            private sealed class En__ {
                private int state;

                private T0_Result int2;

                public SelectManyEn_Meta_Array0<T0_Result> source;

                private int i0;

                private T0_Result[] nested;

                private int i1;

                public T0_Result Current {
                    get {
                        return int2;
                    }
                }

                public En__(int state, SelectManyEn_Meta_Array0<T0_Result> source) {
                    this.state = state;
                    this.source = source;
                }


                private bool MoveNext() {
                    int num = state;
                    if(num != 0) {
                        if(num != 1) {
                            return false;
                        }
                        state = -1;
                        i1++;
                        goto nextNested;
                    }
                    state = -1;
                    i0 = 0;
                    goto nextSource;
                nextNested:
                    if(i1 < nested.Length) {
                        int2 = nested[i1];
                        state = 1;
                        return true;
                    }
                    nested = null;
                    i0++;
                    goto nextSource;
                nextSource:
                    if(i0 < source.source.Length) {
                        nested = source.selector(source.source[i0]);
                        i1 = 0;
                        goto nextNested;
                    }
                    return false;
                }
            }
        }
#nullable restore
    }
}
