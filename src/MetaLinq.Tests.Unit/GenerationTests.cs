using MetaLinq.Internal;
using MetaLinq.Tests;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MetaLinqTests.Unit;

[TestFixture]
public class GenerationTests : BaseFixture {
    #region order by
    [Test]
    public void Array_OrderBy_ToArray() {
            AssertGeneration(
@"Data[] __() {{
    var source = Data.Array(10).Shuffle();
    var result = source.OrderBy(x => x.Int).ToArray();
    source.AssertAll(x => Assert.AreEqual(1, x.Int_GetCount));
    return result;
}}",
            Get0ToNDataArrayAssert(9),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "OrderBy", new[] {
                    new StructMethod("ToArray")
                })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void Array_OrderBy_ThenBy_ToArray() {
        AssertGeneration(
@"(Data[] source, Data[] result) __() {{
    var source = Data.Array(10).Shuffle(longMaxValue: 3);
    return (source, source.OrderBy(x => x.Long).ThenBy(x => x.Int).ToArray());
}}",
        ((Data[] source, Data[] result) x) => CollectionAssert.AreEqual(x.source.OrderBy(x => x.Long).ThenBy(x => x.Int).ToArray(), x.result),
        new[] {
                new MetaLinqMethodInfo(SourceType.Array, "OrderBy", new[] {
                    new StructMethod("ThenBy", new[] {
                        new StructMethod("ToArray"),
                    })
                })
        }
    );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
        Assert.AreEqual(2, TestTrace.ArrayCreatedCount);
    }

    [Test]
    public void Array_OrderBy_ThenBy_FirstAndFirstOrDefault() {
        AssertGeneration(
@"Data __() {{
    var source = Data.Array(10).Shuffle(longMaxValue: 3);
    Assert.Null(source.OrderBy(x => x.Long).ThenBy(x => x.Int).FirstOrDefault(x => x.Int == 1 && x.Long == 3));
    return source.OrderBy(x => x.Long).ThenBy(x => x.Int).First(x => x.Int == 1 && x.Long == 2);
}}",
        (Data x) => {
            Assert.AreEqual(1, x.Int);
            Assert.AreEqual(2, x.Long);
        },
        new[] {
                new MetaLinqMethodInfo(SourceType.Array, "OrderBy", new[] {
                    new StructMethod("ThenBy", new[] {
                        new StructMethod("First"),
                        new StructMethod("FirstOrDefault"),
                    })
                })
        }
    );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
        Assert.AreEqual(4, TestTrace.ArrayCreatedCount);
    }

    [Test]
    public void Array_OrderBy_ThenBy_ToHashSet() {
        AssertGeneration(
@"(Data[] source, HashSet<Data> result) __() {{
    var source = Data.Array(10).Shuffle(longMaxValue: 3);
    return (source, source.OrderBy(x => x.Long).ThenBy(x => x.Int).ToHashSet());
}}",
        ((Data[] source, HashSet<Data> result) x) => CollectionAssert.AreEquivalent(x.source, x.result),
        new[] {
            new MetaLinqMethodInfo(SourceType.Array, "OrderBy", new[] {
                new StructMethod("ThenBy", new[] {
                    new StructMethod("ToHashSet"),
                })
            })
        });
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void Array_OrderBy_ThenBy_ToDictionary() {
        AssertGeneration(
@"(Data[] source, Dictionary<int, Data> result) __() {{
    var source = Data.Array(10).Shuffle(longMaxValue: 3);
    return (source, source.OrderBy(x => x.Long).ThenBy(x => x.Int).ToDictionary(x => x.Int));
}}",
        ((Data[] source, Dictionary<int, Data> result) x) => CollectionAssert.AreEquivalent(x.source.ToDictionary(x => x.Int), x.result),
        new[] {
            new MetaLinqMethodInfo(SourceType.Array, "OrderBy", new[] {
                new StructMethod("ThenBy", new[] {
                    new StructMethod("ToDictionary"),
                })
            })
        });
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void Array_Select_OrderBy_ThenBy_ToArray() {
        AssertGeneration(
@"(Data[] source, Data[] result) __() {{
    var source = Data.Array(10).Shuffle(longMaxValue: 3);
    return (source, source.Select(x => x.Self).OrderBy(x => x.Long).ThenBy(x => x.Int).ToArray());
}}",
        ((Data[] source, Data[] result) x) => CollectionAssert.AreEqual(x.source.Select(x => x.Self).OrderBy(x => x.Long).ThenBy(x => x.Int).ToArray(), x.result),
        new[] {
                new MetaLinqMethodInfo(SourceType.Array, "Select", new[] {
                    new StructMethod("OrderBy", new[] {
                        new StructMethod("ThenBy", new[] {
                            new StructMethod("ToArray"),
                        })

                    })
                })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
        Assert.AreEqual(3, TestTrace.ArrayCreatedCount);
    }

    [Test]
    public void Array_Where_OrderBy_ThenBy_ToArray() {
        AssertGeneration(
@"(Data[] source, Data[] result) __() {{
    var source = Data.Array(10).Shuffle(longMaxValue: 3);
    return (source, source.Where(x => x.Int > 1).OrderBy(x => x.Long).ThenBy(x => x.Int).ToArray());
}}",
        ((Data[] source, Data[] result) x) => CollectionAssert.AreEqual(x.source.Where(x => x.Int > 1).OrderBy(x => x.Long).ThenBy(x => x.Int).ToArray(), x.result),
        new[] {
                new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                    new StructMethod("OrderBy", new[] {
                        new StructMethod("ThenBy", new[] {
                            new StructMethod("ToArray"),
                        })
                    })
                })
        }
    );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }


    [Test]
    public void Array_OrderBy_ThenByDescending_ToArray() {
        AssertGeneration(
@"(Data[] source, Data[] result) __() {{
    var source = Data.Array(10).Shuffle(longMaxValue: 3);
    return (source, source.OrderBy(x => x.Long).ThenByDescending(x => -x.Int).ToArray());
}}",
        ((Data[] source, Data[] result) x) => CollectionAssert.AreEqual(x.source.OrderBy(x => x.Long).ThenByDescending(x => -x.Int).ToArray(), x.result),
        new[] {
                new MetaLinqMethodInfo(SourceType.Array, "OrderBy", new[] {
                    new StructMethod("ThenByDescending", new[] {
                        new StructMethod("ToArray"),
                    })
                })
        }
    );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void Array_OrderByDescending_ThenByDescending_ThenBy_ToArray() {
        AssertGeneration(
@"(Data[] source, Data[] result) __() {{
    var source = Data.Array(20).Shuffle(longMaxValue: 4, shortMaxValue: 2);
    return (source, source.OrderByDescending(x => -x.Short).ThenByDescending(x => -x.Long).ThenBy(x => x.Int).ToArray());
}}",
        ((Data[] source, Data[] result) x) => CollectionAssert.AreEqual(x.source.OrderByDescending(x => -x.Short).ThenByDescending(x => -x.Long).ThenBy(x => x.Int).ToArray(), x.result),
        new[] {
                new MetaLinqMethodInfo(SourceType.Array, "OrderByDescending", new[] {
                    new StructMethod("ThenByDescending", new[] {
                        new StructMethod("ThenBy", new[] {
                            new StructMethod("ToArray"),
                        })
                    })
                })
        }
    );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void Array_Where_OrderBy_Select_OrderBy_ThenBy_ToArray() {
        AssertGeneration(
@"(int, long)[] __() {{
    var source = Data.Array(10).Shuffle(longMaxValue: 4);
    return source.Where(x => x.Int < 7).OrderBy(x => x.Int).Select(x => (x.Int, x.Long)).OrderBy(x => x.Int).ThenBy(x => x.Long).ToArray();
}}",
        ((int, long)[] x) => { },
        new[] {
            new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                new StructMethod("OrderBy", new[] {
                    new StructMethod("Select", new[] {
                        new StructMethod("OrderBy", new[] {
                            new StructMethod("ThenBy", new[] {
                                new StructMethod("ToArray"),
                            })
                        })
                    })
                })
            })
        }
    );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void List_OrderBy_ToArray() {
        AssertGeneration(
@"Data[] __() {{
    var source = Data.List(10).Shuffle();
    var result = source.OrderBy(x => x.Int).ToArray();
    source.AssertAll(x => Assert.AreEqual(1, x.Int_GetCount));
    return result;
}}",
        Get0ToNDataArrayAssert(9),
        new[] {
                new MetaLinqMethodInfo(SourceType.List, "OrderBy", new[] {
                    new StructMethod("ToArray")
                })
        }
    );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }


    [Test]
    public void CustomCollection_OrderBy_ToArray() {
        AssertGeneration(
@"Data[] __() {{
    var source = new CustomCollection<Data>(Data.Array(10).Shuffle());
    var result = source.OrderBy(x => x.Int).ToArray();
    source.AssertAll(x => Assert.AreEqual(1, x.Int_GetCount));
    return result;
}}",
        Get0ToNDataArrayAssert(9),
        new[] {
                new MetaLinqMethodInfo(SourceType.CustomCollection, "OrderBy", new[] {
                    new StructMethod("ToArray")
                })
        }
    );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void CustomEnumerable_OrderBy_ToArray() {
        AssertGeneration(
@"Data[] __() {{
    var source = new CustomEnumerable<Data>(Data.Array(10).Shuffle());
    var result = source.OrderBy(x => x.Int).ToArray();
    source.AssertAll(x => Assert.AreEqual(1, x.Int_GetCount));
    return result;
}}",
        Get0ToNDataArrayAssert(9),
        new[] {
                new MetaLinqMethodInfo(SourceType.CustomEnumerable, "OrderBy", new[] {
                    new StructMethod("ToArray")
                })
        }
    );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
        Assert.AreEqual(1, TestTrace.ArrayCreatedCount);
    }

    [Test]
    public void Array_Select_OrderByDescending_ToArray() {
        AssertGeneration(
@"int[] __() {{
    var source = Data.Array(10).Shuffle();
    var result = source.Select(x => new { Value = -x.Int }).OrderByDescending(x => x.Value).ToArray();
    source.AssertAll(x => Assert.AreEqual(1, x.Int_GetCount));
    return Enumerable.ToArray(Enumerable.Select(result, x => -x.Value));
}}",
        Get0ToNIntArrayAssert(9),
        new[] {
            new MetaLinqMethodInfo(SourceType.Array, "Select", new[] {
                new StructMethod("OrderByDescending", new[] {
                    new StructMethod("ToArray"),
                })
            })
        }
    );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void List_Select_OrderBy_ToArray() {
        AssertGeneration(
@"int[] __() {{
    var source = Data.List(10).Shuffle();
    var result = source.Select(x => new { Value = x.Int }).OrderBy(x => x.Value).ToArray();
    source.AssertAll(x => Assert.AreEqual(1, x.Int_GetCount));
    return Enumerable.ToArray(Enumerable.Select(result, x => x.Value));
}}",
        Get0ToNIntArrayAssert(9),
        new[] {
            new MetaLinqMethodInfo(SourceType.List, "Select", new[] {
                new StructMethod("OrderBy", new[] {
                    new StructMethod("ToArray"),
                })
            })
        }
    );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void CustomCollection_Select_OrderBy_ToArray() {
        AssertGeneration(
@"int[] __() {{
    var source = new CustomCollection<Data>(Data.Array(10).Shuffle());
    var result = source.Select(x => new { Value = x.Int }).OrderBy(x => x.Value).ToArray();
    source.AssertAll(x => Assert.AreEqual(1, x.Int_GetCount));
    return Enumerable.ToArray(Enumerable.Select(result, x => x.Value));
}}",
        Get0ToNIntArrayAssert(9),
        new[] {
            new MetaLinqMethodInfo(SourceType.CustomCollection, "Select", new[] {
                new StructMethod("OrderBy", new[] {
                    new StructMethod("ToArray"),
                })
            })
        }
    );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void CustomEnumerable_Select_OrderBy_ToArray() {
        AssertGeneration(
@"int[] __() {{
    var source = new CustomEnumerable<Data>(Data.Array(10).Shuffle());
    var result = source.Select(x => new { Value = x.Int }).OrderBy(x => x.Value).ToArray();
    source.AssertAll(x => Assert.AreEqual(1, x.Int_GetCount));
    return Enumerable.ToArray(Enumerable.Select(result, x => x.Value));
}}",
        Get0ToNIntArrayAssert(9),
        new[] {
            new MetaLinqMethodInfo(SourceType.CustomEnumerable, "Select", new[] {
                new StructMethod("OrderBy", new[] {
                    new StructMethod("ToArray"),
                })
            })
        }
    );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void Array_OrderByDescending_ToArray() {
        AssertGeneration(
@"Data[] __() {{
    var source = Data.Array(5).Shuffle();
    var result = source.OrderByDescending(x => x.Int).ToArray();
    source.AssertAll(x => Assert.AreEqual(1, x.Int_GetCount));
    return result;
}}",
        GetDataArrayAssert(4, 3, 2, 1, 0),
        new[] {
                new MetaLinqMethodInfo(SourceType.Array, "OrderByDescending", new[] {
                    new StructMethod("ToArray")
                })
        }
    );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_OrderBy_ToArray_AssertSortMethod() {
        AssertGeneration(
            "object? __() { DataExtensions.AssertSortMethod(x => x.OrderBy(x => x.Int).ToArray(), isStable: true, CollectionAssert.AreEqual); return null; }",
            (object x) => Assert.Null(x),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "OrderBy", new[] {
                    new StructMethod("ToArray")
                })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void Array_Where_OrderByDescending_ToArray() {
        AssertGeneration(
@"Data[] __() {{
    var source = Data.Array(10).Shuffle();
    return source.Where(x => x.Int < 5).OrderByDescending(x => x.Int).ToArray();
}}",
        GetDataArrayAssert(4, 3, 2, 1, 0),
        new[] {
            new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                new StructMethod("OrderByDescending", new[] {
                    new StructMethod("ToArray"),
                })
            })
        }
    );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void List_Select_Where_OrderBy_ToArray() {
        AssertGeneration(
@"int[] __() {{
    var source = Data.List(10).Shuffle();
    var result = source.Select(x => new { Value = x.Int }).Where(x => x.Value < 6).OrderBy(x => x.Value).ToArray();
    source.AssertAll(x => Assert.AreEqual(1, x.Int_GetCount));
    return Enumerable.ToArray(Enumerable.Select(result, x => x.Value));
}}",
        Get0ToNIntArrayAssert(5),
        new[] {
            new MetaLinqMethodInfo(SourceType.List, "Select", new[] {
                new StructMethod("Where", new[] {
                    new StructMethod("OrderBy", new[] {
                        new StructMethod("ToArray"),
                    })
                })
            })
        }
    );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void CustomCollection_Select_Where_OrderBy_ToArray() {
        AssertGeneration(
@"int[] __() {{
    var source = new CustomCollection<Data>(Data.Array(10).Shuffle());
    var result = source.Select(x => new { Value = x.Int }).Where(x => x.Value < 6).OrderBy(x => x.Value).ToArray();
    source.AssertAll(x => Assert.AreEqual(1, x.Int_GetCount));
    return Enumerable.ToArray(Enumerable.Select(result, x => x.Value));
}}",
        Get0ToNIntArrayAssert(5),
        new[] {
            new MetaLinqMethodInfo(SourceType.CustomCollection, "Select", new[] {
                new StructMethod("Where", new[] {
                    new StructMethod("OrderBy", new[] {
                        new StructMethod("ToArray"),
                    })
                })
            })
        }
    );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void CustomEnumerable_Select_Where_OrderBy_ToArray() {
        AssertGeneration(
@"int[] __() {{
    var source = new CustomEnumerable<Data>(Data.Array(10).Shuffle());
    var result = source.Select(x => new { Value = x.Int }).Where(x => x.Value < 6).OrderBy(x => x.Value).ToArray();
    source.AssertAll(x => Assert.AreEqual(1, x.Int_GetCount));
    return Enumerable.ToArray(Enumerable.Select(result, x => x.Value));
}}",
        Get0ToNIntArrayAssert(5),
        new[] {
            new MetaLinqMethodInfo(SourceType.CustomEnumerable, "Select", new[] {
                new StructMethod("Where", new[] {
                    new StructMethod("OrderBy", new[] {
                        new StructMethod("ToArray"),
                    })
                })
            })
        }
    );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void Array_Where_SelectMany_Select_Where_OrderByDescending_ToArray() {
        AssertGeneration(
@"int[] __() {{
        var source = Data.Array(10).Shuffle();
        var result = source.Where(x => x.Int < 8).SelectMany(x => x.DataList).Select(x => new { Value = -x.Int }).Where(x => x.Value > -8).OrderByDescending(x => x.Value).ToArray();
        return Enumerable.ToArray(Enumerable.Select(result, x => -x.Value));
    }}",
        Get0ToNIntArrayAssert(7),
        new[] {
            new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                new StructMethod("SelectMany", new[] {
                    new StructMethod("Select", new[] {
                        new StructMethod("Where", new[] {
                            new StructMethod("OrderByDescending", new[] {
                                new StructMethod("ToArray"),
                            })
                        })
                    })
                })
            })
        }
    );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void Array_Where_OrderBy_Select_ToArray() {
        AssertGeneration(
@"int[] __() {{
    var source = Data.Array(10).Shuffle();
    return source.Where(x => x.Int < 7).OrderBy(x => x.Int).Select(x => x.Int).ToArray();
}}",
        Get0ToNIntArrayAssert(6),
        new[] {
            new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                new StructMethod("OrderBy", new[] {
                    new StructMethod("Select", new[] {
                        new StructMethod("ToArray"),
                    })  
                })
            })
        }
    );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void Array_Where_OrderBy_Select_ToHashSet() {
        AssertGeneration(
@"HashSet<int> __() {{
    var source = Data.Array(10).Shuffle();
    return source.Where(x => x.Int < 7).OrderBy(x => x.Int).Select(x => x.Int).ToHashSet();
}}",
        (HashSet<int> x) => CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3, 4, 5, 6 }, x),
        new[] {
            new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                new StructMethod("OrderBy", new[] {
                    new StructMethod("Select", new[] {
                        new StructMethod("ToHashSet"),
                    })
                })
            })
        },
        assertGeneratedCode: x => StringAssert.Contains("new HashSet<T3_Result>(result_2.Length)", x.Single())
    );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void Array_Where_OrderBy_Select_ToDictionary() {
        AssertGeneration(
@"Dictionary<int, int> __() {{
    var source = Data.Array(10).Shuffle();
    return source.Where(x => x.Int < 7).OrderBy(x => x.Int).Select(x => x.Int).ToDictionary(x => x * 10);
}}",
        (Dictionary<int, int> x) => CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3, 4, 5, 6 }.ToDictionary(x => x * 10), x),
        new[] {
            new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                new StructMethod("OrderBy", new[] {
                    new StructMethod("Select", new[] {
                        new StructMethod("ToDictionary"),
                    })
                })
            })
        }
    );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
        Assert.AreEqual(1, TestTrace.DictionaryWithCapacityCreatedCount);
        Assert.AreEqual(0, TestTrace.DictionaryCreatedCount);
    }

    [Test]
    public void Array_Where_OrderBy_Select_OrderByDescending_ToArray() {
        AssertGeneration(
@"int[] __() {{
    var source = Data.Array(10).Shuffle();
    return source.Where(x => x.Int < 7).OrderBy(x => x.Int).Select(x => x.Int).OrderByDescending(x => 2 * x).ToArray();
}}",
        GetIntArrayAssert(new[] { 6, 5, 4, 3, 2, 1, 0 }),
        new[] {
            new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                new StructMethod("OrderBy", new[] {
                    new StructMethod("Select", new[] {
                        new StructMethod("OrderByDescending", new[] {
                            new StructMethod("ToArray"),
                        })
                    })
                })
            })
        }
    );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void Array_Where_OrderBy_Select_OrderByDescending_First() {
        AssertGeneration(
@"int __() {{
    var source = Data.Array(10).Shuffle();
    return source.Where(x => x.Int < 7).OrderBy(x => x.Int).Select(x => x.Int).OrderByDescending(x => 2 * x).First(x => x < 5);
}}",
        (int x) => Assert.AreEqual(4, x),
        new[] {
            new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                new StructMethod("OrderBy", new[] {
                    new StructMethod("Select", new[] {
                        new StructMethod("OrderByDescending", new[] {
                            new StructMethod("First"),
                        })
                    })
                })
            })
        }
    );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
        Assert.AreEqual(3, TestTrace.ArrayCreatedCount);
    }


    [Test]
    public void Array_Where_OrderBy_Select_Where_OrderByDescending_ToArray() {
        AssertGeneration(
@"int[] __() {{
    var source = Data.Array(10).Shuffle();
    return source.Where(x => x.Int < 7).OrderBy(x => x.Int).Select(x => x.Int).Where(x => x > 2).OrderByDescending(x => 2 * x).ToArray();
}}",
        GetIntArrayAssert(new[] { 6, 5, 4, 3 }),
        new[] {
            new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                new StructMethod("OrderBy", new[] {
                    new StructMethod("Select", new[] {
                        new StructMethod("Where", new[] {
                            new StructMethod("OrderByDescending", new[] {
                                new StructMethod("ToArray"),
                            })
                        })
                    })
                })
            })
        }
    );
        Assert.AreEqual(2, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void Array_Where_OrderBy_Select_Where_OrderByDescending_ToHashSet() {
        AssertGeneration(
@"HashSet<int> __() {{
    var source = Data.Array(10).Shuffle();
    return source.Where(x => x.Int < 7).OrderBy(x => x.Int).Select(x => x.Int).Where(x => x > 2).OrderByDescending(x => 2 * x).ToHashSet();
}}",
        (HashSet<int> x) => CollectionAssert.AreEquivalent(new[] { 6, 5, 4, 3 }, x),
        new[] {
            new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                new StructMethod("OrderBy", new[] {
                    new StructMethod("Select", new[] {
                        new StructMethod("Where", new[] {
                            new StructMethod("OrderByDescending", new[] {
                                new StructMethod("ToHashSet"),
                            })
                        })
                    })
                })
            })
        }
    );
        Assert.AreEqual(2, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void Array_Where_OrderBy_Select_Where_OrderByDescending_ToDictionary() {
        AssertGeneration(
@"Dictionary<int, int> __() {{
    var source = Data.Array(10).Shuffle();
    return source.Where(x => x.Int < 7).OrderBy(x => x.Int).Select(x => x.Int).Where(x => x > 2).OrderByDescending(x => 2 * x).ToDictionary(x => x * 10);
}}",
        (Dictionary<int, int> x) => CollectionAssert.AreEquivalent(new[] { 6, 5, 4, 3 }.ToDictionary(x => x * 10), x),
        new[] {
            new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                new StructMethod("OrderBy", new[] {
                    new StructMethod("Select", new[] {
                        new StructMethod("Where", new[] {
                            new StructMethod("OrderByDescending", new[] {
                                new StructMethod("ToDictionary"),
                            })
                        })
                    })
                })
            })
        }
    );
        Assert.AreEqual(2, TestTrace.LargeArrayBuilderCreatedCount);
    }
    #endregion

    #region skip/take while
    [Test]
    public void Array_TakeWhile_ToArray() {
        AssertGeneration(
            "Data[] __() => Data.Array(20).TakeWhile(x => x.Int < 10).ToArray();",
            Get0ToNDataArrayAssert(9),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "TakeWhile", new[] {
                        new StructMethod("ToArray")
                    })
            }
        );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_TakeWhile_FirstOrDefault() {
        AssertGeneration(
@"Data? __() { 
    Assert.Null(Data.Array(20).TakeWhile(x => x.Int < 10).FirstOrDefault(x => x.Int % 15 == 12));
    return Data.Array(20).TakeWhile(x => x.Int < 10).FirstOrDefault(x => x.Int == 8);
}",
            (Data x) => Assert.AreEqual(8, x.Int),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "TakeWhile", new[] {
                        new StructMethod("FirstOrDefault")
                    })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_SkipWhile_ToArray() {
        AssertGeneration(
            "Data[] __() => Data.Array(10).SkipWhile(x => x.Int < 5).ToArray();",
            GetDataArrayAssert(5, 6, 7, 8, 9),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "SkipWhile", new[] {
                        new StructMethod("ToArray")
                    })
            }
        );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_SkipWhile_First() {
        AssertGeneration(
            "Data __() => Data.Array(10).SkipWhile(x => x.Int < 5).First(x => x.Int % 4 == 0);",
            (Data x) => Assert.AreEqual(8, x.Int),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "SkipWhile", new[] {
                        new StructMethod("First")
                    })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_Select_TakeWhile_ToArray() {
        AssertGeneration(
            "int[] __() => Data.Array(20).Select(x => x.Int % 10).TakeWhile(x => x < 5).ToArray();",
            Get0ToNIntArrayAssert(4),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Select", new[] {
                        new StructMethod("TakeWhile", new[] {
                            new StructMethod("ToArray")
                        })
                    })
            }
        );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_Select_SkipWhile_ToArray() {
        AssertGeneration(
            "int[] __() => Data.Array(20).Select(x => x.Int % 10).SkipWhile(x => x < 5).ToArray();",
            GetIntArrayAssert(Enumerable.Range(5, 15).Select(x => x % 10).ToArray()),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Select", new[] {
                        new StructMethod("SkipWhile", new[] {
                            new StructMethod("ToArray")
                        })
                    })
            }
        );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_SelectMany_TakeWhile_ToArray() {
        AssertGeneration(
            "int[] __() => Data.Array(5).SelectMany(x => new[] { 2 * x.Int, 2 * x.Int + 1 }).TakeWhile(x => x % 2 == 0).ToArray();",
            Get0ToNIntArrayAssert(0),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "SelectMany", new[] {
                        new StructMethod("TakeWhile", new[] {
                            new StructMethod("ToArray")
                        })
                    })
            }
        );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_SelectMany_SkipWhile_ToArray() {
        AssertGeneration(
            "int[] __() => Data.Array(5).SelectMany(x => new[] { 2 * x.Int, 2 * x.Int + 1 }).SkipWhile(x => x % 2 == 0).ToArray();",
            GetIntArrayAssert(Enumerable.Range(1, 9).ToArray()),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "SelectMany", new[] {
                        new StructMethod("SkipWhile", new[] {
                            new StructMethod("ToArray")
                        })
                    })
            }
        );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_SelectMany_SkipWhile_First() {
        AssertGeneration(
            "int __() => Data.Array(5).SelectMany(x => new[] { 2 * x.Int, 2 * x.Int + 1 }).SkipWhile(x => x < 5).First(x => x % 4 == 0);",
            (int x) => Assert.AreEqual(8, x),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "SelectMany", new[] {
                        new StructMethod("SkipWhile", new[] {
                            new StructMethod("First")
                        })
                    })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
        Assert.AreEqual(0, TestTrace.ArrayCreatedCount);
    }
    [Test]
    public void Array_SkipWhile_OrderBy_Select_SkipWhile_OrderByDescending_ToArray() {
        AssertGeneration(
@"int[] __() {{
    var source = Enumerable.ToArray(Enumerable.Reverse(Data.Array(10)));
    return source.SkipWhile(x => x.Int >= 7).OrderBy(x => x.Int).Select(x => x.Int).SkipWhile(x => x < 3).OrderByDescending(x => 2 * x).ToArray();
}}",
        GetIntArrayAssert(new[] { 6, 5, 4, 3 }),
        new[] {
            new MetaLinqMethodInfo(SourceType.Array, "SkipWhile", new[] {
                new StructMethod("OrderBy", new[] {
                    new StructMethod("Select", new[] {
                        new StructMethod("SkipWhile", new[] {
                            new StructMethod("OrderByDescending", new[] {
                                new StructMethod("ToArray"),
                            })
                        })
                    })
                })
            })
        }
    );
        Assert.AreEqual(2, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_TakeWhile_OrderBy_Select_TakeWhile_OrderByDescending_ToArray() {
        AssertGeneration(
@"int[] __() {{
    var source = Enumerable.ToArray(Enumerable.Reverse(Data.Array(10)));
    return source.TakeWhile(x => x.Int >= 3).OrderBy(x => x.Int).Select(x => x.Int).TakeWhile(x => x < 7).OrderByDescending(x => 2 * x).ToArray();
}}",
        GetIntArrayAssert(new[] { 6, 5, 4, 3 }),
        new[] {
            new MetaLinqMethodInfo(SourceType.Array, "TakeWhile", new[] {
                new StructMethod("OrderBy", new[] {
                    new StructMethod("Select", new[] {
                        new StructMethod("TakeWhile", new[] {
                            new StructMethod("OrderByDescending", new[] {
                                new StructMethod("ToArray"),
                            })
                        })
                    })
                })
            })
        }
    );
        Assert.AreEqual(2, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_TakeWhile_OrderBy_Select_TakeWhile_OrderByDescending_First() {
        AssertGeneration(
@"int __() {{
    var source = Enumerable.ToArray(Enumerable.Reverse(Data.Array(10)));
    return source.TakeWhile(x => x.Int >= 3).OrderBy(x => x.Int).Select(x => x.Int).TakeWhile(x => x < 7).OrderByDescending(x => 2 * x).First(x => x < 6);
}}",
        (int x) => Assert.AreEqual(5, x),
        new[] {
            new MetaLinqMethodInfo(SourceType.Array, "TakeWhile", new[] {
                new StructMethod("OrderBy", new[] {
                    new StructMethod("Select", new[] {
                        new StructMethod("TakeWhile", new[] {
                            new StructMethod("OrderByDescending", new[] {
                                new StructMethod("First"),
                            })
                        })
                    })
                })
            })
        }
    );
        Assert.AreEqual(2, TestTrace.LargeArrayBuilderCreatedCount);
        Assert.AreEqual(2, TestTrace.ArrayCreatedCount);
    }
    #endregion

    #region where
    [Test]
    public void Array_Where_ToArray() {
        AssertGeneration(
            "Data[] __() => Data.Array(10).Where(x => x.Int < 5).ToArray();",
            Get0To4DataArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                        new StructMethod("ToArray")
                    })
            }
        );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
        Assert.AreEqual(0, TestTrace.ArrayCreatedCount);
    }
    [Test]
    public void Array_Where_ToHashSet() {
        AssertGeneration(
            "HashSet<Data> __() => Data.Array(10).Where(x => x.Int < 5).ToHashSet();",
            (HashSet<Data> x) => CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3, 4 }, x.Select(x => x.Int)),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                    new StructMethod("ToHashSet")
                })
            },
            assertGeneratedCode: x => StringAssert.Contains("new HashSet<TSource>()", x.Single())
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_Where_ToDictionary() {
        AssertGeneration(
            "(Data[], Dictionary<int, Data>) __() { var source = Data.Array(10); return (source, source.Where(x => x.Int < 5).ToDictionary(x => x.Int)); }",
            ((Data[], Dictionary<int, Data>) x) => CollectionAssert.AreEquivalent(x.Item1.Where(x => x.Int < 5).ToDictionary(x => x.Int), x.Item2),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                    new StructMethod("ToDictionary")
                })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
        Assert.AreEqual(0, TestTrace.DictionaryWithCapacityCreatedCount);
        Assert.AreEqual(1, TestTrace.DictionaryCreatedCount);
    }
    [Test]
    public void Array_Where_ToList() {
        AssertGeneration(
            "List<Data> __() => Data.Array(10).Where(x => x.Int < 5).ToList();",
            (List<Data> result) => {
                Assert.AreEqual(5, result.Count);
                Assert.AreEqual(5, result.Capacity);
                Get0To4DataListAssert()(result);
            },
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                        new StructMethod("ToArray"),
                        new StructMethod("ToList")
                    })
            }
        );
    }
    [Test]
    public void Array_Where_ToListAndToArray() {
        AssertGeneration(
            "List<Data> __() { var array = Data.Array(10).Where(x => x.Int < 8).ToArray(); return array.Where(x => x.Int < 5).ToList(); }",
            (List<Data> result) => {
                Assert.AreEqual(5, result.Count);
                Assert.AreEqual(5, result.Capacity);
                Get0To4DataListAssert()(result);
            },
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                        new StructMethod("ToArray"),
                        new StructMethod("ToList")
                    })
            }
        );
    }
    [Test]
    public void ArrayNewExpression_Where_ToArray() {
        AssertGeneration(
            "int[] __() => new [] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }.Where(x => x < 5).ToArray();",
            Get0ToNIntArrayAssert(4),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                        new StructMethod("ToArray")
                    })
            }
        );
    }
    [Test]
    public void ArrayVariable_Where_ToArray() {
        AssertGeneration(
            "Data[] __() { var data = Data.Array(10); return data.Where(x => x.Int < 5).ToArray(); }",
            Get0To4DataArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                        new StructMethod("ToArray")
                    })
            }
        );
    }
    [Test]
    public void ListField_Where_ToArray() {
        AssertGeneration(
            "Data[] __() => dataField.Where(x => x.Int < 5).ToArray();",
            Get0To4DataArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.List, "Where", new[] {
                        new StructMethod("ToArray")
                    })
            },
            additionalClassCode: "static List<Data> dataField = Data.List(10);"
        );
    }
    [Test]
    public void ListParameter_Where_ToArray() {
        AssertGeneration(
            "Data[] __() => GetData(Data.List(10));",
            Get0To4DataArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.List, "Where", new[] {
                        new StructMethod("ToArray")
                    })
            },
            additionalClassCode: "static Data[] GetData(List<Data> list) => list.Where(x => x.Int < 5).ToArray();"
        );
    }
    [Test]
    public void ArrayProperty_Where_ToArray() {
        AssertGeneration(
            "Data[] __() => DataProperty.Where(x => x.Int < 5).ToArray();",
            Get0To4DataArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                        new StructMethod("ToArray")
                    })
            },
            additionalClassCode: "static Data[] DataProperty => Data.Array(10);"
        );
    }

    [Test]
    public void List_Where_ToArray() {
        AssertGeneration(
            "Data[] __() => Data.List(10).Where(x => x.Int < 5).ToArray();",
            Get0To4DataArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.List, "Where", new[] {
                        new StructMethod("ToArray")
                    })
            }
        );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void CustomCollection_Where_ToArray() {
        AssertGeneration(
            "Data[] __() => new CustomCollection<Data>(Data.Array(10)).Where(x => x.Int < 5).ToArray();",
            Get0To4DataArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.CustomCollection, "Where", new[] {
                        new StructMethod("ToArray")
                    })
            }
        );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void CustomEnumerable_Where_ToArray() {
        AssertGeneration(
            "Data[] __() => new CustomEnumerable<Data>(Data.Array(10)).Where(x => x.Int < 5).ToArray();",
            Get0To4DataArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.CustomEnumerable, "Where", new[] {
                        new StructMethod("ToArray")
                    })
            }
        );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_Where_StandardToArray() {
        AssertGeneration(
            "Data[] __() => System.Linq.Enumerable.ToArray(Data.Array(10).Where(x => x.Int < 5));",
            Get0To4DataArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Where", new StructMethod[] {
                        new StructMethod("GetEnumerator")
                    }, implementsIEnumerable: true)
            }
        );
    }
    [Test]
    public void List_Where_StandardToArray() {
        AssertGeneration(
            "Data[] __() => System.Linq.Enumerable.ToArray(Data.List(10).Where(x => x.Int < 5));",
            Get0To4DataArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.List, "Where", new StructMethod[] {
                        new StructMethod("GetEnumerator")
                    }, implementsIEnumerable: true)
            }
        );
    }
    [Test]
    public void ArrayAndList_Where_ToArray_And_StandardToArray_AndForeach() {
        AssertGeneration(
            new (string code, Action<Data[]> assert)[] {
                    (
                        "Data[] __() => Data.Array(10).Where(x => x.Int < 5).ToArray();",
                        Get0To4DataArrayAssert()
                    ),
                    (
                        "Data[] __() => System.Linq.Enumerable.ToArray(Data.Array(10).Where(x => x.Int < 5));",
                        Get0To4DataArrayAssert()
                    ),
                     (
                        "Data[] __()  { List<Data> result = new(); foreach(var item in Data.Array(10).Where(x => x.Int < 5)) result.Add(item); return result.ToArray(); }",
                        Get0To4DataArrayAssert()
                    ),
                    (
                        "Data[] __() => Data.List(10).Where(x => x.Int < 5).ToArray();",
                        Get0To4DataArrayAssert()
                    ),
                    (
                        "Data[] __() => System.Linq.Enumerable.ToArray(Data.List(10).Where(x => x.Int < 5));",
                        Get0To4DataArrayAssert()
                    ),
                     (
                        "Data[] __()  { List<Data> result = new(); foreach(var item in Data.List(10).Where(x => x.Int < 5)) result.Add(item); return result.ToArray(); }",
                        Get0To4DataArrayAssert()
                    ),
            },
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                        new StructMethod("GetEnumerator"),
                        new StructMethod("ToArray"),
                    }, implementsIEnumerable: true),
                    new MetaLinqMethodInfo(SourceType.List, "Where", new[] {
                        new StructMethod("GetEnumerator"),
                        new StructMethod("ToArray"),
                    }, implementsIEnumerable: true)

            }
        );
    }
    [Test]
    public void Array_Where_ToArray_Standard() {
        AssertGeneration(
            "Data[] __() => Data.Array(10).Where(x => x.Int < 5).ToArray();",
            Get0To4DataArrayAssert(),
            new MetaLinqMethodInfo[0],
            addMetaLinqUsing: false,
            addStadardLinqUsing: true
        );
    }
    [Test]
    public void List_Where_ToArray_Standard() {
        AssertGeneration(
            "Data[] __() => Data.List(10).Where(x => x.Int < 5).ToArray();",
            Get0To4DataArrayAssert(),
            new MetaLinqMethodInfo[0],
            addMetaLinqUsing: false,
            addStadardLinqUsing: true
        );
    }

    [Test]
    public void Array_Where_First() {
        var expectedMessage = Assert.Throws<InvalidOperationException>(() => new[] { 1 }.First(x => x == 0))!.Message;
        AssertGeneration(
$@"Data __() {{
    var ex = Assert.Throws<System.InvalidOperationException>(() => Data.Array(10).Where(x => x.Int > 5).First(x => x.Int == 0));
    Assert.AreEqual(""{expectedMessage}"", ex!.Message);
    var source = Data.Array(10);
    var result = source.Where(x => x.Int > 5).First(x => x.Int % 4 == 0);
    Assert.AreEqual(0, source[9].Int_GetCount);
    return result;
}}",
            (Data x) => Assert.AreEqual(8, x.Int),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                    new StructMethod("First")
                })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_Where_FirstOrDefault() {
        AssertGeneration(
$@"Data __() {{
    Assert.Null(Data.Array(10).Where(x => x.Int > 5).FirstOrDefault(x => x.Int == 0));
    var source = Data.Array(10);
    var result = source.Where(x => x.Int > 5).FirstOrDefault(x => x.Int % 4 == 0);
    Assert.AreEqual(0, source[9].Int_GetCount);
    return result!;
}}",
            (Data x) => Assert.AreEqual(8, x.Int),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                    new StructMethod("FirstOrDefault")
                })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void CustomEnumerable_Where_First() {
        var expectedMessage = Assert.Throws<InvalidOperationException>(() => new[] { 1 }.First(x => x == 0))!.Message;
        AssertGeneration(
$@"Data __() {{
    var source = Data.Array(10);
    var result = new CustomEnumerable<Data>(source).Where(x => x.Int > 5).First(x => x.Int % 4 == 0);
    Assert.AreEqual(0, source[9].Int_GetCount);
    return result;
}}",
            (Data x) => Assert.AreEqual(8, x.Int),
            new[] {
                new MetaLinqMethodInfo(SourceType.CustomEnumerable, "Where", new[] {
                    new StructMethod("First")
                })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }
    #endregion

    #region select
    [Test]
    public void Array_Select_ToArray() {
        AssertGeneration(
            "int[] __() => Data.Array(5).Select(x => x.Int).ToArray();",
            Get0ToNIntArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Select", new[] {
                        new StructMethod("ToArray")
                    })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
        Assert.AreEqual(1, TestTrace.ArrayCreatedCount);
    }
    [Test]
    public void Array_Select_ToHashSet() {
        AssertGeneration(
            "HashSet<int> __() => Data.Array(5).Select(x => x.Int).ToHashSet();",
            (HashSet<int> x) => CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3, 4 }, x),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "Select", new[] {
                    new StructMethod("ToHashSet")
                })
            },
            assertGeneratedCode: x => StringAssert.Contains("new HashSet<T1_Result>(this.source.Length)", x.Single())
        );
    }
    [Test]
    public void Array_Select_ToDictionay() {
        AssertGeneration(
            "(Data[], Dictionary<int, Data>) __() { var source = Data.Array(5); return (source, source.Select(x => x.Self).ToDictionary(x => x.Int)); }",
            ((Data[], Dictionary<int, Data>) x) => CollectionAssert.AreEquivalent(x.Item1.ToDictionary(x => x.Int), x.Item2),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "Select", new[] {
                    new StructMethod("ToDictionary")
                })
            }
        );
        Assert.AreEqual(1, TestTrace.DictionaryWithCapacityCreatedCount);
        Assert.AreEqual(0, TestTrace.DictionaryCreatedCount);
    }
    [Test]
    public void List_Select_ToHashSet() {
        AssertGeneration(
            "HashSet<int> __() => Data.List(5).Select(x => x.Int).ToHashSet();",
            (HashSet<int> x) => CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3, 4 }, x),
            new[] {
                new MetaLinqMethodInfo(SourceType.List, "Select", new[] {
                    new StructMethod("ToHashSet")
                })
            }
        );
    }
    [Test]
    public void List_Select_ToDictionay() {
        AssertGeneration(
            "(List<Data>, Dictionary<int, Data>) __() { var source = Data.List(5); return (source, source.Select(x => x.Self).ToDictionary(x => x.Int)); }",
            ((List<Data>, Dictionary<int, Data>) x) => CollectionAssert.AreEquivalent(x.Item1.ToDictionary(x => x.Int), x.Item2),
            new[] {
                new MetaLinqMethodInfo(SourceType.List, "Select", new[] {
                    new StructMethod("ToDictionary")
                })
            }
        );
        Assert.AreEqual(1, TestTrace.DictionaryWithCapacityCreatedCount);
        Assert.AreEqual(0, TestTrace.DictionaryCreatedCount);
    }
    [Test]
    public void Array_Select_Select_ToArray() {
        AssertGeneration(
            "int[] __() => Data.Array(5).Select(x => x.Int - 1).Select(x => x + 1).ToArray();",
            Get0ToNIntArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Select", new[] {
                        new StructMethod("Select", new[] { 
                            new StructMethod("ToArray") 
                        })
                    })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_Select_Select_ToHashSet() {
        AssertGeneration(
            "HashSet<int> __() => Data.Array(5).Select(x => x.Int - 1).Select(x => x + 1).ToHashSet();",
            (HashSet<int> x) => CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3, 4 }, x),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Select", new[] {
                        new StructMethod("Select", new[] {
                            new StructMethod("ToHashSet")
                        })
                    })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_Select_Select_ToDictionary() {
        AssertGeneration(
            "Dictionary<int, long> __() => Data.Array(5).Select(x => x.Int - 2).Select(x => (long)x * 10).ToDictionary(x => (int)x / 10 + 2);",
            (Dictionary<int, long> x) => CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3, 4 }.ToDictionary(x => x, x => (long)(x - 2) * 10), x),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "Select", new[] {
                    new StructMethod("Select", new[] {
                        new StructMethod("ToDictionary")
                    })
                })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void List_Select_ToArray() {
        AssertGeneration(
            "int[] __() => Data.List(5).Select(x => x.Int).ToArray();",
            Get0ToNIntArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.List, "Select", new[] {
                        new StructMethod("ToArray")
                    })
            }
        );
    }
    [Test]
    public void CustomCollection_Select_ToArray() {
        AssertGeneration(
            "int[] __() => new CustomCollection<Data>(Data.Array(5)).Select(x => x.Int).ToArray();",
            Get0ToNIntArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.CustomCollection, "Select", new[] {
                        new StructMethod("ToArray")
                    })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void CustomEnumerable_Select_ToArray() {
        AssertGeneration(
            "int[] __() => new CustomEnumerable<Data>(Data.Array(5)).Select(x => x.Int).ToArray();",
            Get0ToNIntArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.CustomEnumerable, "Select", new[] {
                        new StructMethod("ToArray")
                    })
            }
        );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_Select_StandardToArray() {
        AssertGeneration(
            "int[] __() => System.Linq.Enumerable.ToArray(Data.Array(5).Select(x => x.Int));",
            Get0ToNIntArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Select", new StructMethod[] {
                        new StructMethod("GetEnumerator")
                    }, implementsIEnumerable: true)
            }
        );
    }
    [Test]
    public void List_Select_StandardToArray() {
        AssertGeneration(
            "int[] __() => System.Linq.Enumerable.ToArray(Data.List(5).Select(x => x.Int));",
            Get0ToNIntArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.List, "Select", new StructMethod[] {
                        new StructMethod("GetEnumerator")
                    }, implementsIEnumerable: true)
            }
        );
    }
    [Test]
    public void Array_Select_Foreach() {
        AssertGeneration(
            "int[] __()  { List<int> result = new(); foreach(var item in Data.Array(5).Select(x => x.Int)) result.Add(item); return result.ToArray(); }",
            Get0ToNIntArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Select", new StructMethod[] {
                        new StructMethod("GetEnumerator")
                    }, implementsIEnumerable: true)
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }

    [Test]
    public void Array_SelectAndWhere_ToArray() {
        AssertGeneration(
            new (string code, Action<Data[]> assert)[] {
                    (
                        "Data[] __() => Data.Array(5).Select(x => x.Self).ToArray();",
                        Get0To4DataArrayAssert()
                    ),
                    (
                        "Data[] __() => Data.Array(10).Where(x => x.Int < 5).ToArray();",
                        Get0To4DataArrayAssert()
                    )
            },
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Select", new[] {
                        new StructMethod("ToArray")
                    }),
                    new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                        new StructMethod("ToArray")
                    }),
            }
        );
    }

    [Test]
    public void Array_Select_First() {
        AssertGeneration(
$@"Data __() {{
    Assert.Throws<System.InvalidOperationException>(() => Data.Array(10).Select(x => x.Self).First(x => x.Int == -1));
    var source = Data.Array(10);
    var result = source.Select(x => x.Self).First(x => x.Int > 0 && x.Int % 4 == 0);
    Assert.AreEqual(0, source[5].Int_GetCount);
    return result;
}}",
            (Data x) => Assert.AreEqual(4, x.Int),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "Select", new[] {
                    new StructMethod("First")
                })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
        Assert.AreEqual(0, TestTrace.ArrayCreatedCount);
    }
    [Test]
    public void Array_Select_FirstOrDefault() {
        AssertGeneration(
$@"Data __() {{
    Assert.Null(Data.Array(10).Select(x => x.Self).FirstOrDefault(x => x.Int == -1));
    var source = Data.Array(10);
    var result = source.Select(x => x.Self).FirstOrDefault(x => x.Int > 0 && x.Int % 4 == 0);
    Assert.AreEqual(0, source[5].Int_GetCount);
    return result!;
}}",
            (Data x) => Assert.AreEqual(4, x.Int),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "Select", new[] {
                    new StructMethod("FirstOrDefault")
                })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void CustomEnumerable_Select_First() {
        AssertGeneration(
$@"Data __() {{
    var source = Data.Array(10);
    var result = new CustomEnumerable<Data>(source).Select(x => x.Self).First(x => x.Int > 0 && x.Int % 4 == 0);
    Assert.AreEqual(0, source[5].Int_GetCount);
    return result;
}}",
            (Data x) => Assert.AreEqual(4, x.Int),
            new[] {
                new MetaLinqMethodInfo(SourceType.CustomEnumerable, "Select", new[] {
                    new StructMethod("First")
                })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }
    #endregion

    #region select many
    [Test]
    public void Array_SelectManyArray_ToArray() {
        AssertGeneration(
@"int[] __() {{
    var source = Data.Array(3);
    var result = source.SelectMany(x => x.IntArray).ToArray();
    source.AssertAll(x => Assert.AreEqual(1, x.IntArray_GetCount));
    return result;
}}",
            Get0ToNIntArrayAssert(5),
            new [] {
                new MetaLinqMethodInfo(SourceType.Array, "SelectMany", new[] {
                    new StructMethod("ToArray")
                })
            }
        );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_SelectManyArray_ToHashSet() {
        AssertGeneration(
@"HashSet<int> __() {{
    var source = Data.Array(3);
    var result = source.SelectMany(x => x.IntArray).ToHashSet();
    source.AssertAll(x => Assert.AreEqual(1, x.IntArray_GetCount));
    return result;
}}",
            (HashSet<int> x) => CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4, 5 }, x),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "SelectMany", new[] {
                    new StructMethod("ToHashSet")
                })
            },
            assertGeneratedCode: x => StringAssert.Contains("new HashSet<T1_Result>()", x.Single())
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_SelectManyArray_ToDictionary() {
        AssertGeneration(
@"Dictionary<int, int> __() {{
    var source = Data.Array(3);
    var result = source.SelectMany(x => x.IntArray).ToDictionary(x => x * 10);
    source.AssertAll(x => Assert.AreEqual(1, x.IntArray_GetCount));
    return result;
}}",
            (Dictionary<int, int> x) => CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4, 5 }.ToDictionary(x => x * 10), x),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "SelectMany", new[] {
                    new StructMethod("ToDictionary")
                })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
        Assert.AreEqual(0, TestTrace.DictionaryWithCapacityCreatedCount);
        Assert.AreEqual(1, TestTrace.DictionaryCreatedCount);
    }
    [Test]
    public void Array_SelectManyArrayNewArrayExpression_ToArray() {
        AssertGeneration(
            @"int[] __() => Data.Array(3).SelectMany(x => new[] { 2 * x.Int, 2 * x.Int + 1 }).ToArray();",
            Get0ToNIntArrayAssert(5),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "SelectMany", new[] {
                    new StructMethod("ToArray")
                })
            }
        );
    }
    [Test]
    public void List_SelectManyList_ToArray() {
        AssertGeneration(
            "int[] __() => Data.List(3).SelectMany(x => x.IntList).ToArray();",
            Get0ToNIntArrayAssert(5),
            new[] {
                new MetaLinqMethodInfo(SourceType.List, "SelectMany", new[] {
                    new StructMethod("ToArray")
                })
            }
        );
    }
    [Test]
    public void List_SelectManyCustomCollection_ToArray() {
        AssertGeneration(
            "int[] __() => Data.List(3).SelectMany(x => new CustomCollection<int>(x.IntArray)).ToArray();",
            Get0ToNIntArrayAssert(5),
            new[] {
                new MetaLinqMethodInfo(SourceType.List, "SelectMany", new[] {
                    new StructMethod("ToArray")
                })
            }
        );
    }
    [Test]
    public void List_SelectManyCustomEnumerable_ToArray() {
        AssertGeneration(
            "int[] __() => Data.List(3).SelectMany(x => new CustomEnumerable<int>(x.IntArray)).ToArray();",
            Get0ToNIntArrayAssert(5),
            new[] {
                new MetaLinqMethodInfo(SourceType.List, "SelectMany", new[] {
                    new StructMethod("ToArray")
                })
            }
        );
    }
    [Test]
    public void Array_SelectManyArrayAndList_ToArray() {
        AssertGeneration(
            new (string code, Action<int[]> assert)[] {
                (
                    "int[] __() => Data.Array(3).SelectMany(x => x.IntArray).ToArray();",
                    Get0ToNIntArrayAssert(5)
                ),
                (
                    "int[] __() => Data.Array(3).SelectMany(x => x.IntList).ToArray();",
                    Get0ToNIntArrayAssert(5)
                )
            },
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "SelectMany", new[] {
                    new StructMethod("ToArray")
                }),
                new MetaLinqMethodInfo(SourceType.Array, "SelectMany", new[] {
                    new StructMethod("ToArray")
                })
            }
        );
    }
    [Test]
    public void Array_SelectManyArray_StandardToArray() {
        AssertGeneration(
@"int[] __() {{
    var source = Data.Array(3);
    var result = Enumerable.ToArray(source.SelectMany(x => x.IntArray));
    source.AssertAll(x => Assert.AreEqual(1, x.IntArray_GetCount));
    return result;
}}",
            Get0ToNIntArrayAssert(5),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "SelectMany", new[] {
                    new StructMethod("GetEnumerator")
                }, implementsIEnumerable: true)
            }
        );
    }
    [Test]
    public void Array_SelectManyList_StandardToArray() {
        AssertGeneration(
@"int[] __() {{
    var source = Data.Array(3);
    var result = Enumerable.ToArray(source.SelectMany(x => x.IntList));
    source.AssertAll(x => Assert.AreEqual(1, x.IntList_GetCount));
    return result;
}}",
            Get0ToNIntArrayAssert(5),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "SelectMany", new[] {
                    new StructMethod("GetEnumerator")
                }, implementsIEnumerable: true)
            }
        );
    }
    [Test]
    public void List_SelectManyArray_StandardToArray() {
        AssertGeneration(
            "int[] __() => Enumerable.ToArray(Data.List(3).SelectMany(x => x.IntArray));",
            Get0ToNIntArrayAssert(5),
            new[] {
                new MetaLinqMethodInfo(SourceType.List, "SelectMany", new[] {
                    new StructMethod("GetEnumerator")
                }, implementsIEnumerable: true)
            }
        );
    }
    [Test]
    public void SelectMany_EnumeratorTests() {
        AssertGeneration(
            new (string code, Action<int[]> assert)[] {
                ("int[] __() => SelectMany(new[] { new[] { 0, 1 }, new[] { 2, 3, 4 } });",
                 Get0ToNIntArrayAssert(4)),
                ("int[] __() => SelectMany(new int[][] { new int[] { } });",
                 Get0ToNIntArrayAssert(-1)),
                ("int[] __() => SelectMany(new[] {  new int[0], new[] { 0, 1, 2 } });",
                 Get0ToNIntArrayAssert(2)),
                ("int[] __() => SelectMany(new[] { new[] { 0, 1 }, new int[0] });",
                 Get0ToNIntArrayAssert(1)),
                ("int[] __() => SelectMany(new[] { new[] { 0 }, new[] { 1 } });",
                 Get0ToNIntArrayAssert(1)),
            },
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "SelectMany", new[] {
                    new StructMethod("GetEnumerator")
                }, implementsIEnumerable: true)
            },
            additionalClassCode: "static int[] SelectMany(int[][] ints) => Enumerable.ToArray(ints.SelectMany(static x => x));"
        );
    }
    [Test]
    public void Array_SelectManyList_SelectManyArray_ToArray() {
        AssertGeneration(
@"int[] __() {{
    var source = Data.Array(3);
    var result = source.SelectMany(x => x.DataList).SelectMany(x => x.IntArray).ToArray();
    source.AssertAll(x => {
        Assert.AreEqual(1, x.DataList_GetCount);
        x.DataList.AssertAll(y => Assert.AreEqual(1, y.IntArray_GetCount));
    });
    return result;
}}",
            Get0ToNIntArrayAssert(11),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "SelectMany", new[] {
                    new StructMethod("SelectMany", new[] {
                        new StructMethod("ToArray")
                    })
                })
            }
        );
    }
    [Test]
    public void Array_SelectManyList_SelectManyArray_StandardToArray() {
        AssertGeneration(
@"int[] __() {{
    var result = Enumerable.ToArray(source.SelectMany(x => x.DataList).SelectMany(x => x.IntArray));
    source.AssertAll(x => {
        Assert.AreEqual(1, x.DataList_GetCount);
        x.DataList.AssertAll(y => Assert.AreEqual(1, y.IntArray_GetCount));
    });
    return result;
}}
static Data[] source = Data.Array(3);",
            Get0ToNIntArrayAssert(11),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "SelectMany", new[] {
                    new StructMethod("SelectMany", new[] {
                        new StructMethod("GetEnumerator")
                    }, implementsIEnumerable: true)
                })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void SelectMany_SelectMany_EnumeratorTests() {
        AssertGeneration(
            new (string code, Action<int[]> assert)[] {
                ("int[] __() => SelectMany(new int[][][] { } );",
                 Get0ToNIntArrayAssert(-1)),
                ("int[] __() => SelectMany(new [] { new int[][] { } });",
                 Get0ToNIntArrayAssert(-1)),
                ("int[] __() => SelectMany(new[] { new [] { new int[] { } } });",
                 Get0ToNIntArrayAssert(-1)),
                ("int[] __() => SelectMany(new[] { new[] { new [] { 0 } } });",
                 Get0ToNIntArrayAssert(0)),
                ("int[] __() => SelectMany(new[] { new[] { new[] { 0 } }, new[] { new[] { 1 } } });",
                 Get0ToNIntArrayAssert(1)),
                ("int[] __() => SelectMany(new[] { new[] { new[] { 0, 1 }, new[] { 2 } }, new[] { new[] { 3, 4, 5 }, new int[] {}, new[] { 6, 7 } } });",
                 Get0ToNIntArrayAssert(7)),
            },
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "SelectMany", new[] {
                    new StructMethod("SelectMany", new[] {
                        new StructMethod("GetEnumerator")
                    }, implementsIEnumerable: true)
                })
            },
            additionalClassCode: "static int[] SelectMany(int[][][] ints) => Enumerable.ToArray(ints.SelectMany(static x => x).SelectMany(static x => x));"
        );
    }

    [Test]
    public void SelectMany_LongChains() {
        AssertGeneration(
            new (string code, Action<int[]> assert)[] {
                ("int[] __() => Enumerable.ToArray(Data.List(5).Where(x => x.Int < 3).SelectMany(x => x.DataList).SelectMany(x => x.IntArray));",
                 Get0ToNIntArrayAssert(11)),
                ("int[] __() => Data.List(5).Where(x => x.Int < 3).SelectMany(x => x.DataList).SelectMany(x => x.IntArray).ToArray();",
                 Get0ToNIntArrayAssert(11)),

                ("int[] __() => Enumerable.ToArray(Data.List(6).SelectMany(x => x.DataList).Where(x => x.Int % 2 == 1).Where(x => x.Int < 5).SelectMany(x => x.IntArray).Select(x => x + 1));",
                 GetIntArrayAssert(new[] { 3, 4, 7, 8 })),
                ("int[] __() => Data.List(6).SelectMany(x => x.DataList).Where(x => x.Int % 2 == 1).Where(x => x.Int < 5).SelectMany(x => x.IntArray).Select(x => x + 1).ToArray();",
                 GetIntArrayAssert(new[] { 3, 4, 7, 8 })),

                ("int[] __() => Enumerable.ToArray(Data.List(6).SelectMany(x => x.DataList).Where(x => x.Int % 2 == 1).SelectMany(x => x.IntArray));",
                 GetIntArrayAssert(new[] { 2, 3, 6, 7, 10, 11, 14, 15, 18, 19, 22, 23 })),
                ("int[] __() => Data.List(6).SelectMany(x => x.DataList).Where(x => x.Int % 2 == 1).SelectMany(x => x.IntArray).ToArray();",
                 GetIntArrayAssert(new[] { 2, 3, 6, 7, 10, 11, 14, 15, 18, 19, 22, 23 })),
            },
            new[] {
                new MetaLinqMethodInfo(SourceType.List, "SelectMany", new[] {
                    new StructMethod("Where", new[] {
                        new StructMethod("SelectMany", new[] {
                            new StructMethod("GetEnumerator"),
                            new StructMethod("ToArray"),
                        }, implementsIEnumerable: true),
                        new StructMethod("Where", new[] {
                            new StructMethod("SelectMany", new[] {
                                new StructMethod("Select", new[] {
                                    new StructMethod("GetEnumerator"),
                                    new StructMethod("ToArray"),
                                }, implementsIEnumerable: true),
                            })
                        }),
                    })
                }),
                new MetaLinqMethodInfo(SourceType.List, "Where", new[] {
                    new StructMethod("SelectMany", new[] {
                        new StructMethod("SelectMany", new[] {
                            new StructMethod("GetEnumerator"),
                            new StructMethod("ToArray"),
                        }, implementsIEnumerable: true)
                    })
                }),
            }
        );
    }

    [Test]
    public void Array_SelectManyList_First() {
        AssertGeneration(
$@"Data __() {{
    var source = Data.Array(5);
    var result = source.SelectMany(x => x.DataList).First(x => x.Int > 0 && x.Int % 4 == 0);
    Assert.AreEqual(2, source[2].DataList[0].Int_GetCount);
    Assert.AreEqual(0, source[2].DataList[1].Int_GetCount);
    return result;
}}",
            (Data x) => Assert.AreEqual(4, x.Int),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "SelectMany", new[] {
                    new StructMethod("First")
                })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_SelectManyList_FirstOrDefault() {
        AssertGeneration(
$@"Data __() {{
    Assert.Null(Data.Array(5).SelectMany(x => x.DataList).FirstOrDefault(x => x.Int < 0));
    var source = Data.Array(5);
    var result = source.SelectMany(x => x.DataList).FirstOrDefault(x => x.Int > 0 && x.Int % 4 == 0);
    Assert.AreEqual(2, source[2].DataList[0].Int_GetCount);
    Assert.AreEqual(0, source[2].DataList[1].Int_GetCount);
    return result!;
}}",
            (Data x) => Assert.AreEqual(4, x.Int),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "SelectMany", new[] {
                    new StructMethod("FirstOrDefault")
                })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void CustomEnumerable_SelectManyCustomEnumerable_First() {
        AssertGeneration(
$@"Data __() {{
    var source = Data.Array(5);
    var result = new CustomEnumerable<Data>(source).SelectMany(x => new CustomEnumerable<Data>(x.DataList)).FirstOrDefault(x => x.Int > 0 && x.Int % 4 == 0);
    Assert.AreEqual(2, source[2].DataList[0].Int_GetCount);
    Assert.AreEqual(0, source[2].DataList[1].Int_GetCount);
    return result!;
}}",
            (Data x) => Assert.AreEqual(4, x.Int),
            new[] {
                new MetaLinqMethodInfo(SourceType.CustomEnumerable, "SelectMany", new[] {
                    new StructMethod("FirstOrDefault")
                })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }

    #endregion

    #region select and where
    [Test]
    public void Array_Select_Where_ToArray() {
        AssertGeneration(
            "int[] __() => Data.Array(10).Select(x => x.Int).Where(x => x < 5).ToArray();",
            Get0ToNIntArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Select", new[] {
                        new StructMethod("Where", new[] {
                            new StructMethod("ToArray")
                        })
                    })
            }
        );
        Assert.AreEqual(1, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void Array_Select_Where_ToHashSet() {
        AssertGeneration(
            "HashSet<int> __() => Data.Array(10).Select(x => x.Int).Where(x => x < 5).ToHashSet();",
            (HashSet<int> x) =>CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, x),
            new[] {
                new MetaLinqMethodInfo(SourceType.Array, "Select", new[] {
                    new StructMethod("Where", new[] {
                        new StructMethod("ToHashSet")
                    })
                })
            },
            assertGeneratedCode: x => StringAssert.Contains("new HashSet<T1_Result>()", x.Single())
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void List_Select_Where_ToArray() {
        AssertGeneration(
            "int[] __() => Data.List(10).Select(x => x.Int).Where(x => x < 5).ToArray();",
            Get0ToNIntArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.List, "Select", new[] {
                        new StructMethod("Where", new[] {
                            new StructMethod("ToArray")
                        })
                    })
            }
        );
    }
    [Test]
    public void Array_Where_Select_ToArray() {
        AssertGeneration(
            "int[] __() => Data.Array(10).Where(x => x.Int < 5).Select(x => x.Int).ToArray();",
            Get0ToNIntArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                        new StructMethod("Select", new[] {
                            new StructMethod("ToArray")
                        })
                    })
            }
        );
    }

    [Test]
    public void Array_Select_Where_StandardToArray() {
        AssertGeneration(
            "int[] __() => System.Linq.Enumerable.ToArray(Data.Array(10).Select(x => x.Int).Where(x => x < 5));",
            Get0ToNIntArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Select", new[] {
                        new StructMethod("Where", new[] {
                            new StructMethod("GetEnumerator")
                        }, implementsIEnumerable: true)
                    })
            }
        );
    }
    [Test]
    public void List_Where_Select_ForEach() {
        AssertGeneration(
            "int[] __()  { List<int> result = new(); foreach(var item in Data.List(10).Where(x => x.Int < 5).Select(x => x.Int)) result.Add(item); return result.ToArray(); }",
            Get0ToNIntArrayAssert(),
            new[] {
                    new MetaLinqMethodInfo(SourceType.List, "Where", new[] {
                        new StructMethod("Select", new[] {
                            new StructMethod("GetEnumerator")
                        }, implementsIEnumerable: true)
                    })
            }
        );
        Assert.AreEqual(0, TestTrace.LargeArrayBuilderCreatedCount);
    }
    [Test]
    public void SelectAndWhere_LongMixedChains() {
        AssertGeneration(
            new (string code, Action<int[]> assert)[] {
                (
                    "int[] __() => System.Linq.Enumerable.ToArray(Data.Array(10).Where(x => x.Int < 7).Select(x => x.Int - 2).Where(x => x >= 0));",
                    Get0ToNIntArrayAssert()
                ),
                (
                    "int[] __() => System.Linq.Enumerable.ToArray(Data.Array(10).Where(x => x.Int < 5).Select(x => x.Int));",
                    Get0ToNIntArrayAssert()
                ),
                (
                    "int[] __() => Data.List(10).Where(x => x.Int < 7).Select(x => x.Int - 2).Where(x => x >= 0).ToArray();",
                    Get0ToNIntArrayAssert()
                ),
            },
            new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                        new StructMethod("Select", new[] {
                            new StructMethod("GetEnumerator"),
                            new StructMethod("Where", new[] {
                                new StructMethod("GetEnumerator")
                            }, implementsIEnumerable: true),
                        }, implementsIEnumerable: true)
                    }),
                    new MetaLinqMethodInfo(SourceType.List, "Where", new[] {
                        new StructMethod("Select", new[] {
                            new StructMethod("Where", new[] {
                                new StructMethod("ToArray")
                            })
                        })
                    })
            }
        );
    }

    [Test]
    public void List_Select_Where_First() {
        AssertGeneration(
            "int __() => Data.List(10).Select(x => x.Int).Where(x => x > 4).First(x => x % 4 == 3);",
            (int x) => Assert.AreEqual(7, x),
            new[] {
                new MetaLinqMethodInfo(SourceType.List, "Select", new[] {
                    new StructMethod("Where", new[] {
                        new StructMethod("First")
                    })
                })
            }
        );
    }
    [Test]
    public void List_Where_Select_FirstOrDefault() {
        AssertGeneration(
@"int? __() { 
    Assert.AreEqual(0, Data.List(10).Where(x => x.Int > 4).Select(x => x.Int).FirstOrDefault(x => x == -1));
    return Data.List(10).Where(x => x.Int > 4).Select(x => x.Int).FirstOrDefault(x => x % 4 == 3); 
}",
            (int? x) => Assert.AreEqual(7, x),
            new[] {
                new MetaLinqMethodInfo(SourceType.List, "Where", new[] {
                    new StructMethod("Select", new[] {
                        new StructMethod("FirstOrDefault")
                    })
                })
            }
        );
    }
    #endregion

    #region skip
    [Test]
    public void Array() {
        AssertGeneration(
            "Data[] __() => Data.Array(5);",
            Get0To4DataArrayAssert(),
            new MetaLinqMethodInfo[0]
        );
    }
    [Test]
    public void EnumerableToArray() {
        AssertGeneration(
            "Data[] __() => Enumerable.ToArray(Data.Array(5));",
            Get0To4DataArrayAssert(),
            new MetaLinqMethodInfo[0]
        );
    }
    #endregion

    static Action<Data[]> Get0To4DataArrayAssert() {
        return Get0ToNDataArrayAssert(4);
    }
    static Action<Data[]> Get0ToNDataArrayAssert(int n = 4) {
        return GetDataArrayAssert(Enumerable.Range(0, n + 1).ToArray());
    }
    static Action<Data[]> GetDataArrayAssert(params int[] array) {
        return (Data[] result) => {
            CollectionAssert.AreEqual(array, result.Select(x => x.Int).ToArray());
        };
    }
    static Action<int[]> Get0ToNIntArrayAssert(int n = 4) {
        return GetIntArrayAssert(Enumerable.Range(0, n + 1).ToArray());
    }
    static Action<int[]> GetIntArrayAssert(int[] expected) {
        return (int[] result) => {
            CollectionAssert.AreEqual(expected, result);
        };
    }
    static Action<List<Data>> Get0To4DataListAssert() {
        return (List<Data> result) => {
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, result.Select(x => x.Int).ToArray());
        };
    }
    static Action<List<int>> Get0ToNIntListAssert(int n = 4) {
        return GetIntListAssert(Enumerable.Range(0, n + 1).ToList());
    }
    static Action<List<int>> GetIntListAssert(List<int> expected) {
        return (List<int> result) => {
            CollectionAssert.AreEqual(expected, result);
        };
    }
    class StructMethod {
        public readonly string Name;
        public readonly StructMethod[] ResultMethods;
        public readonly bool ImplementsIEnumerable;
        public StructMethod(string name, StructMethod[]? resultMethods = null, bool implementsIEnumerable = false) {
            Name = name;
            ResultMethods = resultMethods ?? new StructMethod[0];
            ImplementsIEnumerable = implementsIEnumerable;
        }
        public override bool Equals(object? obj) {
            return obj is StructMethod method &&
                   Name == method.Name &&
                   ImplementsIEnumerable == method.ImplementsIEnumerable &&
                   StructuralComparisons.StructuralEqualityComparer.Equals(ResultMethods, method.ResultMethods);
        }
        public override int GetHashCode() {
            throw new NotImplementedException();
        }
        public override string ToString() {
            var methods = string.Join(", ", ResultMethods.Select(x => x.ToString()));
            return $"Name: {Name}, IEnumerable: {ImplementsIEnumerable}, Methods: [ {methods} ]";
        }
    }
    enum SourceType { Array, List, CustomCollection, CustomEnumerable }
    sealed class MetaLinqMethodInfo : StructMethod {
        public readonly SourceType SourceType;

        public MetaLinqMethodInfo(SourceType sourceType, string name, StructMethod[] resultMethods, bool implementsIEnumerable = false)
            : base(name, resultMethods, implementsIEnumerable) {
            SourceType = sourceType;
        }
        public override bool Equals(object? obj) {
            return base.Equals(obj) && obj is MetaLinqMethodInfo info &&
                   SourceType == info.SourceType;
        }
        public override string ToString() {
            return $"SourceType: {SourceType}, {base.ToString()}";
        }
        public override int GetHashCode() {
            throw new NotImplementedException();
        }
    }


    static void AssertGeneration<T>(string code, Action<T> assert, MetaLinqMethodInfo[] methods, bool addMetaLinqUsing = true, bool addStadardLinqUsing = true, string? additionalClassCode = null, Action<IEnumerable<string>>? assertGeneratedCode = null) {
        AssertGeneration(new[] { (code, assert) }, methods, addMetaLinqUsing, addStadardLinqUsing, additionalClassCode, assertGeneratedCode);
    }
    static void AssertGeneration<T>((string code, Action<T> assert)[] cases, MetaLinqMethodInfo[] methods, bool addMetaLinqUsing = true, bool addStadardLinqUsing = true, string? additionalClassCode = null, Action<IEnumerable<string>>? assertGeneratedCode = null) {
        var refLocation = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var references = new[] {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(refLocation, "netstandard.dll")),
                MetadataReference.CreateFromFile(Path.Combine(refLocation, "System.Linq.dll")),
                MetadataReference.CreateFromFile(Path.Combine(refLocation, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(refLocation, "System.Buffers.dll")),
                MetadataReference.CreateFromFile(Path.Combine(refLocation, "System.Collections.dll")),
                MetadataReference.CreateFromFile(typeof(Data).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Assert).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(MetaLinq.MetaEnumerable).Assembly.Location),
            };

        var executeMethodsCode = string.Join(Environment.NewLine, cases.Select((x, i) => "static " + x.code.Replace("__", "Execute" + i)));

        var source =
$@"
{(addMetaLinqUsing ? "using MetaLinq;" : null)}
{(addStadardLinqUsing ? "using System.Linq;" : null)}
using MetaLinq.Tests;
using NUnit.Framework;
using System.Collections.Generic;
public static class Executor {{
{additionalClassCode}
{executeMethodsCode}
}}
";


        var location = Path.Combine(Path.GetDirectoryName(typeof(GenerationTests).Assembly.Location)!, "Generated");
        if(!Directory.Exists(location))
            Directory.CreateDirectory(location);
        var filesPath = Path.Combine(location, NUnit.Framework.TestContext.CurrentContext.Test.Name);
        if(!Directory.Exists(filesPath))
            Directory.CreateDirectory(filesPath);
        var dllPath = filesPath + ".dll";

        Compilation inputCompilation = CSharpCompilation.Create(
            "MyCompilation",
            new[] { CSharpSyntaxTree.ParseText(source, path: "Source.cs", encoding: Encoding.UTF8) },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable)
        );
        MetaLinqGenerator generator = new();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

        bool DebugMode = true;

        foreach(var tree in outputCompilation.SyntaxTrees.ToArray()) {
            if(!File.Exists(tree.FilePath)) {
                var newPath = Path.Combine(filesPath, Path.GetFileName(tree.FilePath));
                
                if(DebugMode) {
                    outputCompilation = outputCompilation.ReplaceSyntaxTree(tree, tree.WithFilePath(newPath));
                    File.WriteAllText(newPath, tree.GetText().ToString(), Encoding.UTF8);
                } else {
                    if(File.Exists(newPath))
                        File.Delete(newPath);
                }

            }
        }
        GeneratorDriverRunResult runResult = driver.GetRunResult();
        CollectionAssert.IsEmpty(runResult.Diagnostics);
        GeneratorRunResult generatorResult = runResult.Results[0];
        var generatedCode = generatorResult.GeneratedSources.Select(x => x.SourceText.ToString());
        assertGeneratedCode?.Invoke(generatedCode);

        var emitResult = outputCompilation.Emit(dllPath, pdbPath: DebugMode ? Path.ChangeExtension(dllPath, "pdb") : null);
        var severeDiagnostics = emitResult.Diagnostics.Where(x => x.Severity != DiagnosticSeverity.Hidden).ToArray();
        if(!emitResult.Success || severeDiagnostics.Any()) {
            foreach(var code in generatedCode) {
                var split = code.Split(Environment.NewLine);
                int line = 1;
                foreach(var item in split) {
                    Debug.Write(line);
                    Debug.WriteLine(item);
                    line++;
                }
            }
            foreach(var item in emitResult.Diagnostics) {
                Debug.WriteLine(item);
            }
        }
        CollectionAssert.IsEmpty(severeDiagnostics);
        Assert.True(emitResult.Success);

        var assembly = Assembly.LoadFile(dllPath);
        var executorType = assembly.GetType("Executor")!;
        var executeMethods = executorType
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Where(x => x.Name.StartsWith("Execute"))
            .OrderBy(x => x.Name)
            .ToArray();
        for(int i = 0; i < executeMethods.Length; i++) {
            var result = (T)executeMethods[i].Invoke(null, null)!;
            cases[i].assert(result);
        }

        AssertGeneratedClasses(methods, assembly, executorType);
    }

    static void AssertGeneratedClasses(MetaLinqMethodInfo[] methods, Assembly assembly, Type executorType) {
        var extensionsType = assembly.GetType("MetaLinq.MetaEnumerable")!;

        if(!methods.Any()) {
            Assert.Null(extensionsType);
            return;
        }

        var expectedGeneratedTypes = new HashSet<Type>();
        Assert.False(extensionsType.IsPublic);
        var allGeneratedTypes = assembly.GetTypes()
            .Where(x => x != extensionsType && x != executorType && !x.IsNested && !typeof(Attribute).IsAssignableFrom(x) && x.GetCustomAttribute<CompilerGeneratedAttribute>() == null)
            .SelectMany(x => {
                var nested = Extensions.Flatten(x.GetNestedTypes(), x => x.GetNestedTypes().Where(x => x.Name != CodeGenerationTraits.EnumeratorTypeName));
                CollectionAssert.IsNotEmpty(nested);
                return nested;
            })
            .ToArray();
        var actualMethods = extensionsType
            .GetMethods()
            .Where(x => x.DeclaringType == extensionsType)
            .Select(x => {
                Assert.False(x.ReturnType.IsPublic);
                bool implementsIEnumerable = ImplementsIEnumerable(x);
                expectedGeneratedTypes.Add(x.ReturnType.GetGenericTypeDefinition());
                var sourceType = x.GetParameters()[0].ParameterType.Name switch {
                    "TSource[]" => SourceType.Array,
                    "List`1" => SourceType.List,
                    "CustomCollection`1" => SourceType.CustomCollection,
                    "CustomEnumerable`1" => SourceType.CustomEnumerable,
                    _ => throw new InvalidOperationException()
                };
                Assert.AreEqual(CodeGenerationTraits.RootStaticTypePrefix + sourceType.ToString() + "`1", x.ReturnType.DeclaringType!.Name);
                Assert.NotNull(x.ReturnType.GetCustomAttribute(typeof(IsReadOnlyAttribute)));
                return new MetaLinqMethodInfo(
                    sourceType,
                    x.Name,
                    x.ReturnType
                        .GetMethods()
                        .Where(y => y.DeclaringType == x.ReturnType)
                        .Select(y => CollectMethods(expectedGeneratedTypes, y))
                        .ToArray(),
                    implementsIEnumerable: implementsIEnumerable
                );
            })
            .OrderBy(x => x.SourceType)
            .ToArray();
        CollectionAssert.AreEqual(methods, actualMethods);
        CollectionAssert.AreEquivalent(expectedGeneratedTypes.ToArray(), allGeneratedTypes);
    }
    static StructMethod CollectMethods(HashSet<Type> expectedGeneratedTypes, MethodInfo method) {
        //TODO uncomment all here
        bool implementsIEnumerable = ImplementsIEnumerable(method);
        if(method is { ReturnType: { IsValueType: true, IsNested: true } } && method.ReturnType.Name != CodeGenerationTraits.EnumeratorTypeName) {
            expectedGeneratedTypes.Add(method.ReturnType.IsGenericType ? method.ReturnType.GetGenericTypeDefinition() : method.ReturnType);
            Assert.True(method.ReturnType.IsNestedPublic);
            Assert.NotNull(method.ReturnType.GetCustomAttribute(typeof(IsReadOnlyAttribute)));
            return new StructMethod(
                method.Name,
                method.ReturnType
                    .GetMethods()
                    .Where(y => y.DeclaringType == method.ReturnType)
                    .Select(y => CollectMethods(expectedGeneratedTypes, y))
                    .ToArray(),
                implementsIEnumerable: implementsIEnumerable
            );
        } else {
            return new StructMethod(method.Name);
        }
    }

    static bool ImplementsIEnumerable(MethodInfo method) {
        return method.ReturnType.GetInterfaces().Where(x => x.Name.Contains("IEnumerable")).Count() == 2;
    }
}
