namespace MetaLinqTests.Unit.Spikes;

[TestFixture]
public class SpikesTests {
    [Test]
    public void Plain() {
        void Assert(int[] expected, int[]source) {
            var result = new List<int>();
            var en = new Enumerator0<int>(source);
            while(en.MoveNext()) {
                result.Add(en.Current);
            }
            CollectionAssert.AreEqual(expected, result);
        }
        Assert(new[] { 0, 1, 2 }, new[] { 0, 1, 2 });
        Assert(new[] { 1 }, new[] { 1 });
        Assert(new int[] {  }, new int[] { });
    }
    [Test]
    public void SelectMany() {
        void Assert(int[] expected, int[][] source) {
            var result = new List<int>();
            var en = new Enumerator1<int[], int>(source, x => x);
            while(en.MoveNext()) {
                result.Add(en.Current);
            }
            CollectionAssert.AreEqual(expected, result);
        }
        Assert(new[] { 0, 1, 2, 3, 4 }, new[] { new[] { 0, 1 }, new[] { 2, 3, 4 } });
        Assert(new int[] {  }, new int[][] { new int[] { } });
        Assert(new[] { 2, 3, 4 }, new[] {  new int[0], new[] { 2, 3, 4 } });
        Assert(new[] { 0, 1 }, new[] { new[] { 0, 1 }, new int[0] } );
        Assert(new[] { 0, 1 }, new[] { new[] { 0 }, new[] { 1 } });
    }
    [Test]
    public void SelectMany_SelectMany() {
        void Assert(int[] expected, int[][][] source) {
            var result = new List<int>();
            var en = new Enumerator2<int[][], int[], int>(source, x => x, x => x);
            while(en.MoveNext()) {
                result.Add(en.Current);
            }
            CollectionAssert.AreEqual(expected, result);
        }

        Assert(new int[] {  }, new int[][][] { } );
        Assert(new int[] { }, new [] { new int[][] { } });
        Assert(new int[] { }, new[] { new [] { new int[] { } } });
        Assert(new int[] { 0 }, new[] { new[] { new [] { 0 } } });
        Assert(new int[] { 0, 1 }, new[] {
            new[] { new[] { 0 } },
            new[] { new[] { 1 } }
        });
        Assert(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, new[] { 
            new[] { new[] { 0, 1 }, new[] { 2 } },
            new[] { new[] { 3, 4, 5 }, new int[] {}, new[] { 6, 7 } }
        });
    }

    public class Data {
        public int Id { get; set; }

    }
    [Test]
    public void OrderBy() {
        var en = new[] { 4, 3, 6, 2 }.Select(x => new Data { Id = x }) .OrderBy(x => x.Id);
        en.ToArray();
    }
}

#nullable disable
public struct Enumerator2<TSource, TResult1, TResult2> {
    TSource[] source;
    Func<TSource, TResult1[]> selector1;
    Func<TResult1, TResult2[]> selector2;

    TResult1[] nested1;
    TResult2[] nested2;
    int i0;
    int i1;
    int i2;
    TResult2 current;
    int state;

    public Enumerator2(TSource[] source, Func<TSource, TResult1[]> selector1, Func<TResult1, TResult2[]> selector2) : this() {
        this.source = source;
        this.selector1 = selector1;
        this.selector2 = selector2;
        nested1 = null;
        nested2 = null;
        i0 = -1;
        i1 = -1;
        i2 = -1;
        current = default;
        state = -1;
    }

    public TResult2 Current => current;
    public bool MoveNext() {
        if(state == 0) //in progress
            goto next2;
        if(state == -1) //start
            goto next0;
        return false; //finished
    next0:
        i0++;
        if(i0 == source.Length) {
            state = 1;
            return false;
        }
        nested1 = selector1(source[i0]);
        i1 = -1;
    next1:
        i1++;
        if(i1 == nested1.Length)
            goto next0;
        nested2 = selector2(nested1[i1]);
        i2 = -1;
    next2:
        i2++;
        if(i2 == nested2.Length)
            goto next1;
        current = nested2[i2];
        state = 0;
        return true;
    }
}
public struct Enumerator1<TSource, TResult> {
    TSource[] source;
    Func<TSource, TResult[]> source_selector;

    TResult[] source1;
    int i0;
    int i1;
    TResult current;
    int state;

    public Enumerator1(TSource[] source, Func<TSource, TResult[]> selector) : this() {
        this.source = source;
        this.source_selector = selector;
        source1 = null;
        i0 = -1;
        i1 = 0;
        state = -1;
        current = default;
    }

    public TResult Current => current;
    public bool MoveNext() {
        if(state == 0) //in progress
            goto next1;
        if(state == -1) //start
            goto next0;
        return false; //finished
    next0:
        var source0 = source;
        i0++;
        if(i0 == source0.Length) {
            state = 1;
            return false;
        }
        var item0 = source0[i0];

        source1 = source_selector(item0);
        i1 = -1;
    next1:
        i1++;
        if(i1 == source1.Length)
            goto next0;
        current = source1[i1];

        state = 0;
        return true;
    }
}

public struct Enumerator0<TSource> {
    TSource[] source;
    //Func<TSource, TResult[]> selector;
    int i0;
    //int i1;
    TSource current;
    int state;

    public Enumerator0(TSource[] source) : this() {
        this.source = source;
        //this.selector = selector;
        i0 = -1;
        state = -1;
        current = default;
    }

    public TSource Current => current;
    public bool MoveNext() {
        if(state == 0) //in progress
            goto next0;
        if(state == -1) //start
            goto next0;
        return false; //finished
    next0:
        i0++;
        if(i0 == source.Length) {
            state = 1;
            return false;
        }
        current = source[i0];
        state = 0;
        return true;
    }
}


public struct Enumerator0_WhereSelectWhere<TSource, TResult> {
    readonly TSource[] source0;
    readonly Func<TSource, bool> predicate1;
    readonly Func<TSource, TResult> selector2;
    readonly Func<TResult, bool> predicate3;

    int i0;
    TResult current;
    int state;

    public Enumerator0_WhereSelectWhere(
        TSource[] source0, 
        Func<TSource, bool> predicate1, 
        Func<TSource, TResult> selector2,
        Func<TResult, bool> predicate3
    ) : this() {
        this.source0 = source0;
        this.predicate1 = predicate1;
        this.selector2 = selector2;
        this.predicate3 = predicate3;
        i0 = -1;
        state = -1;
        current = default;
    }

    public TResult Current => current;
    public bool MoveNext() {
        if(state == 0) //in progress
            goto next0;
        if(state == -1) //start
            goto next0;
        return false; //finished
    next0:
        i0++;
        if(i0 == source0.Length) {
            state = 1;
            return false;
        }
        var item0 = source0[i0];

        var item1 = item0;
        if(!predicate1(item1))
            goto next0;

        var item2 = selector2(item1);

        var item3 = item2;
        if(!predicate3(item3))
            goto next0;

        current = item3;
        state = 0;
        return true;
    }
}
#nullable restore