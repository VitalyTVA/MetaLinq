using static MetaLinq.Generator.LinqNode;

namespace MetaLinqTests.Unit;

[TestFixture]
public class PieceOfWorkTests {
    static readonly ToValueType[] OrderDependentValueTypes = new[] {
        ToValueType.First, ToValueType.FirstOrDefault, ToValueType.Last, ToValueType.LastOrDefault
    };
    public static readonly ToValueType[] OrderIndependentValueTypes = new[] {
        ToValueType.All, ToValueType.Any, 
        ToValueType.Single, ToValueType.SingleOrDefault,

        ToValueType.Sum_Int, ToValueType.Sum_IntN, 
        ToValueType.Sum_Long, ToValueType.Sum_LongN,
        ToValueType.Sum_Float, ToValueType.Sum_FloatN,
        ToValueType.Sum_Double, ToValueType.Sum_DoubleN,
        ToValueType.Sum_Decimal, ToValueType.Sum_DecimalN,

        ToValueType.Average_Int, ToValueType.Average_IntN,
        ToValueType.Average_Long, ToValueType.Average_LongN,
        ToValueType.Average_Float, ToValueType.Average_FloatN,
        ToValueType.Average_Double, ToValueType.Average_DoubleN,
        ToValueType.Average_Decimal, ToValueType.Average_DecimalN,

        ToValueType.Min_Int, ToValueType.Min_IntN,
        ToValueType.Min_Long, ToValueType.Min_LongN,
        ToValueType.Min_Float, ToValueType.Min_FloatN,
        ToValueType.Min_Double, ToValueType.Min_DoubleN,
        ToValueType.Min_Decimal, ToValueType.Min_DecimalN,

        ToValueType.Max_Int, ToValueType.Max_IntN,
        ToValueType.Max_Long, ToValueType.Max_LongN,
        ToValueType.Max_Float, ToValueType.Max_FloatN,
        ToValueType.Max_Double, ToValueType.Max_DoubleN,
        ToValueType.Max_Decimal, ToValueType.Max_DecimalN,
    };
    static readonly ToValueType[] NoSortValueTypes = 
        System.Linq.Enumerable.Concat(OrderIndependentValueTypes, OrderDependentValueTypes).ToArray();
    static readonly ToValueType[] NoSortForwardValueTypes =
        System.Linq.Enumerable.Concat(OrderIndependentValueTypes, new[] { ToValueType.First, ToValueType.FirstOrDefault }).ToArray();

    [Test]
    public void OrderBy_() {
        AssertPieces(new[] { OrderBy }, new[] {
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]"
        });
    }

    [Test]
    public void CustomEnumerable_OrderBy_ToArray(
        [Values(ToValueType.ToArray)] ToValueType toValueType
    ) {
        AssertPieces(new[] { OrderBy }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: []",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]"
        }, SourceType.CustomEnumerable, toValueType);
    }
    [Test]
    public void CustomEnumerable_OrderBy_ToDictionaryAndHashSet(
    [Values(ToValueType.ToHashSet, ToValueType.ToDictionary)] ToValueType toValueType
) {
        AssertPieces(new[] { OrderBy }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [Identity]"
        }, SourceType.CustomEnumerable, toValueType);
    }

    [TestCaseSource(nameof(OrderDependentValueTypes))]
    public void CustomEnumerable_OrderBy_First(ToValueType toValueType) {
        AssertPieces(new[] { OrderBy }, new[] {
"KnownType: True, KnownSize: False, LoopType: Sort, Nodes: [OrderBy]"
        }, SourceType.CustomEnumerable, toValueType);
    }

    [TestCaseSource(nameof(OrderIndependentValueTypes))]
    public void CustomEnumerable_OrderBy_Any(ToValueType toValueType) {
        AssertPieces(new[] { OrderBy }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [Identity]"
        }, SourceType.CustomEnumerable, toValueType);
    }

    [TestCaseSource(nameof(OrderIndependentValueTypes))]
    public void CustomEnumerable_OrderBy_ThenBy_Any(ToValueType toValueType) {
        AssertPieces(new[] { OrderBy, ThenBy }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [Identity, Identity]"
        }, SourceType.CustomEnumerable, toValueType);
    }


    [TestCaseSource(nameof(OrderIndependentValueTypes))]
    public void CustomEnumerable_OrderByDescending_ThenByDescending_Any(ToValueType toValueType) {
        AssertPieces(new[] { OrderByDescending, ThenByDescending }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [Identity, Identity]"
        }, SourceType.CustomEnumerable, toValueType);
    }

    [TestCaseSource(nameof(OrderDependentValueTypes))]
    public void CustomEnumerable_Where_OrderBy_First(ToValueType toValueType) {
        AssertPieces(new[] { Where, OrderBy }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [Where]",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]"
        }, SourceType.CustomEnumerable, toValueType);
    }

    [TestCaseSource(nameof(OrderIndependentValueTypes))]
    public void CustomEnumerable_Where_OrderBy_Any(ToValueType toValueType) {
        AssertPieces(new[] { Where, OrderBy }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [Where, Identity]"
        }, SourceType.CustomEnumerable, toValueType);
    }

    [TestCaseSource(nameof(OrderDependentValueTypes))]
    public void CustomEnumerable_OfType_OrderBy_First(ToValueType toValueType) {
        AssertPieces(new[] { OfType, OrderBy }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [OfType]",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]"
        }, SourceType.CustomEnumerable, toValueType);
    }

    [TestCaseSource(nameof(OrderIndependentValueTypes))]
    public void CustomEnumerable_OfType_OrderBy_Any(ToValueType toValueType) {
        AssertPieces(new[] { OfType, OrderBy }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [OfType, Identity]"
        }, SourceType.CustomEnumerable, toValueType);
    }

    [TestCaseSource(nameof(OrderDependentValueTypes))]
    public void CustomEnumerable_Cast_OrderBy_First(ToValueType toValueType) {
        AssertPieces(new[] { Cast, OrderBy }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [Cast]",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]"
        }, SourceType.CustomEnumerable, toValueType);
    }

    [TestCaseSource(nameof(OrderIndependentValueTypes))]
    public void CustomEnumerable_Cast_OrderBy_Any(ToValueType toValueType) {
        AssertPieces(new[] { Cast, OrderBy }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [Cast, Identity]"
        }, SourceType.CustomEnumerable, toValueType);
    }

    [TestCaseSource(nameof(OrderDependentValueTypes))]
    public void CustomEnumerable_Where_First(ToValueType toValueType
) {
        AssertPieces(new[] { Where }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [Where]",
        }, SourceType.CustomEnumerable, toValueType);
    }

    [Test]
    public void CustomEnumerable_Where_Last(
[Values(ToValueType.Last, ToValueType.LastOrDefault)] ToValueType toValueType
) {
        AssertPieces(new[] { Where }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [Where]",
        }, SourceType.CustomEnumerable, toValueType);
    }


    [TestCaseSource(nameof(NoSortForwardValueTypes))]
    public void CustomEnumerable_Select_First(ToValueType toValueType) {
        AssertPieces(new[] { Select }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [Select]",
        }, SourceType.CustomEnumerable, toValueType);
    }

    [Test]
    public void CustomEnumerable_Select_Last(
[Values(ToValueType.Last, ToValueType.LastOrDefault)] ToValueType toValueType
) {
        AssertPieces(new[] { Select }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [Select]",
        }, SourceType.CustomEnumerable, toValueType);
    }

    [TestCaseSource(nameof(NoSortValueTypes))]
    public void CustomEnumerable_SelectManyArray_First(ToValueType toValueType) {
        AssertPieces(new[] { SelectMany(SourceType.Array) }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [SelectMany Array]",
        }, SourceType.CustomEnumerable, toValueType);
    }

    [TestCaseSource(nameof(NoSortValueTypes))]
    public void CustomEnumerable_SelectManyCustomEnumerable_First(ToValueType toValueType) {
        AssertPieces(new[] { SelectMany(SourceType.CustomEnumerable) }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [SelectMany CustomEnumerable]",
        }, SourceType.CustomEnumerable, toValueType);
    }

    [TestCaseSource(nameof(NoSortValueTypes))]
    public void Array_SelectManyCustomEnumerable_First(ToValueType toValueType) {
        AssertPieces(new[] { SelectMany(SourceType.CustomEnumerable) }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [SelectMany CustomEnumerable]",
        }, SourceType.Array, toValueType);
    }

    [TestCaseSource(nameof(NoSortForwardValueTypes))]
    public void Where_First(ToValueType toValueType) {
        AssertPieces(new[] { Where }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [Where]",
        }, toValueType: toValueType);
    }

    [Test]
    public void Where_Last(
[Values(ToValueType.Last, ToValueType.LastOrDefault)] ToValueType toValueType
) {
        AssertPieces(new[] { Where }, new[] {
"KnownType: True, KnownSize: False, LoopType: Backward, Nodes: [Where]",
        }, toValueType: toValueType);
    }

    [TestCaseSource(nameof(NoSortForwardValueTypes))]
    public void Select_First(ToValueType toValueType) {
        AssertPieces(new[] { Select }, new[] {
"KnownType: False, KnownSize: True, LoopType: Forward, Nodes: [Select]",
        }, toValueType: toValueType);
    }

    [Test]
    public void Select_Last(
[Values(ToValueType.Last, ToValueType.LastOrDefault)] ToValueType toValueType
) {
        AssertPieces(new[] { Select }, new[] {
"KnownType: False, KnownSize: True, LoopType: Backward, Nodes: [Select]",
        }, toValueType: toValueType);
    }

    [TestCaseSource(nameof(OrderDependentValueTypes))]
    public void Cast_OrderBy_First(ToValueType toValueType) {
        AssertPieces(new[] { Cast, OrderBy }, new[] {
"KnownType: False, KnownSize: True, LoopType: Sort, Nodes: [Cast, OrderBy]"
        }, SourceType.List, toValueType);
    }

    [TestCaseSource(nameof(OrderIndependentValueTypes))]
    public void Cast_OrderBy_Any(ToValueType toValueType) {
        AssertPieces(new[] { Cast, OrderBy }, new[] {
"KnownType: False, KnownSize: True, LoopType: Forward, Nodes: [Cast, Identity]"
        }, SourceType.List, toValueType);
    }

    [Test]
    public void Cast_OrderBy() {
        AssertPieces(new[] { Cast, OrderBy }, new[] {
"KnownType: False, KnownSize: True, LoopType: Sort, Nodes: [Cast, OrderBy]"
        }, SourceType.List);
    }

    [Test]
    public void OrderBy_ThenBy() {
        AssertPieces(new[] { OrderBy, ThenBy}, new[] {
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy, ThenBy]"
        });
    }

    [Test]
    public void Where_OrderBy_ThenBy() {
        AssertPieces(new[] { Where, OrderBy, ThenBy }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [Where]",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy, ThenBy]"
        });
    }

    [Test]
    public void TakeWhile_OrderBy_ThenBy() {
        AssertPieces(new[] { TakeWhile, OrderBy, ThenBy }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [TakeWhile]",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy, ThenBy]"
        });
    }

    [Test]
    public void OrderBy_ThenByDescending() {
        AssertPieces(new[] { OrderBy, ThenByDescending }, new[] {
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy, ThenByDescending]"
        });
    }

    [Test]
    public void OrderBy_Select() {
        AssertPieces(new[] { OrderBy, Select }, new[] {
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]",
"KnownType: False, KnownSize: True, LoopType: Forward, Nodes: [Select]"
        });
    }

    [Test]
    public void OrderBy_Select_OrderBy() {
        AssertPieces(new[] { OrderBy, Select, OrderBy }, new[] {
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]",
"KnownType: False, KnownSize: True, LoopType: Sort, Nodes: [Select, OrderBy]"
        });
    }

    [Test]
    public void OrderBy_Select_OrderByDescending_ThenBy() {
        AssertPieces(new[] { OrderBy, Select, OrderByDescending, ThenBy }, new[] {
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]",
"KnownType: False, KnownSize: True, LoopType: Sort, Nodes: [Select, OrderByDescending, ThenBy]"
        });
    }

    [Test]
    public void Select_() {
        AssertPieces(new[] { Select }, new[] {
"KnownType: False, KnownSize: True, LoopType: Forward, Nodes: [Select]"
        });
    }

    [Test]
    public void Select_CustomEnumerable() {
        AssertPieces(new[] { Select }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [Select]"
        }, SourceType.CustomEnumerable);
    }

    [Test]
    public void Select_OrderBy() {
        AssertPieces(new[] { Select, OrderBy }, new[] {
"KnownType: False, KnownSize: True, LoopType: Sort, Nodes: [Select, OrderBy]"
        });
    }

    [Test]
    public void Select_Select_OrderBy() {
        AssertPieces(new[] { Select, Select, OrderBy }, new[] {
"KnownType: False, KnownSize: True, LoopType: Sort, Nodes: [Select, Select, OrderBy]"
        });
    }

    [Test]
    public void SelectMany_() {
        AssertPieces(new[] { SelectMany(SourceType.List) }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [SelectMany List]"
        });
    }

    [Test]
    public void Where_OrderBy() {
        AssertPieces(new[] { Where, OrderBy }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [Where]",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]"
        });
    }

    [Test]
    public void SkipWhile_OrderBy() {
        AssertPieces(new[] { SkipWhile, OrderBy }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [SkipWhile]",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]"
        });
    }

    [Test]
    public void Where_Select_OrderBy() {
        AssertPieces(new[] { Where, Select, OrderBy }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [Where, Select]",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]"
        });
    }

    [Test]
    public void TakeWhile_Select_OrderBy() {
        AssertPieces(new[] { TakeWhile, Select, OrderBy }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [TakeWhile, Select]",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]"
        });
    }

    [Test]
    public void Where_Select_OrderBy_SelectMany_Where() {
        AssertPieces(new[] { Where, Select, OrderBy, SelectMany(SourceType.List), Where}, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [Where, Select]",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]",
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [SelectMany List, Where]",
        });
    }

    [Test]
    public void Where_Select_OrderBy_OfType_Where() {
        AssertPieces(new[] { Where, Select, OrderBy, OfType, Where }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [Where, Select]",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]",
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [OfType, Where]",
        });
    }

    [Test]
    public void Where_Select_OrderBy_Cast_Where() {
        AssertPieces(new[] { Where, Select, OrderBy, Cast, Where }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [Where, Select]",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]",
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [Cast, Where]",
        });
    }

    [Test]
    public void TakeWhile_Select_OrderBy_SelectMany_Where() {
        AssertPieces(new[] { TakeWhile, Select, OrderBy, SelectMany(SourceType.List), Where }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [TakeWhile, Select]",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]",
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [SelectMany List, Where]",
        });
    }

    [Test]
    public void SkipWhile_Select_OrderBy_SelectMany_TakeWhile() {
        AssertPieces(new[] { SkipWhile, Select, OrderBy, SelectMany(SourceType.List), TakeWhile }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [SkipWhile, Select]",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]",
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [SelectMany List, TakeWhile]",
        });
    }

    [Test]
    public void Where_OrderBy_Select() {
        AssertPieces(new[] { Where, OrderBy, Select }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [Where]",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]",
"KnownType: False, KnownSize: True, LoopType: Forward, Nodes: [Select]",
        });
    }

    [Test]
    public void TakeWhile_OrderBy_Select() {
        AssertPieces(new[] { TakeWhile, OrderBy, Select }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [TakeWhile]",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]",
"KnownType: False, KnownSize: True, LoopType: Forward, Nodes: [Select]",
        });
    }

    [Test]
    public void SkipWhile_OrderBy_Select() {
        AssertPieces(new[] { SkipWhile, OrderBy, Select }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [SkipWhile]",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]",
"KnownType: False, KnownSize: True, LoopType: Forward, Nodes: [Select]",
        });
    }

    [Test]
    public void Select_OrderBy_Where() {
        AssertPieces(new[] { Select, OrderBy, Where }, new[] {
"KnownType: False, KnownSize: True, LoopType: Sort, Nodes: [Select, OrderBy]",
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [Where]",
        });
    }

    [Test]
    public void Select_OrderBy_TakeWhile() {
        AssertPieces(new[] { Select, OrderBy, TakeWhile }, new[] {
"KnownType: False, KnownSize: True, LoopType: Sort, Nodes: [Select, OrderBy]",
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [TakeWhile]",
        });
    }

    [Test]
    public void Select_OrderBy_SkipWhile() {
        AssertPieces(new[] { Select, OrderBy, SkipWhile }, new[] {
"KnownType: False, KnownSize: True, LoopType: Sort, Nodes: [Select, OrderBy]",
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [SkipWhile]",
        });
    }

    [Test]
    public void Where_() {
        AssertPieces(new[] { Where }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [Where]"
        });
    }

    [Test]
    public void OfType_() {
        AssertPieces(new[] { OfType }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [OfType]"
        });
    }

    [Test]
    public void Cast_() {
        AssertPieces(new[] { Cast }, new[] {
"KnownType: False, KnownSize: True, LoopType: Forward, Nodes: [Cast]"
        });
    }

    [Test]
    public void SkipWhile_() {
        AssertPieces(new[] { SkipWhile }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [SkipWhile]"
        });
    }

    [Test]
    public void TakeWhile_() {
        AssertPieces(new[] { TakeWhile }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [TakeWhile]"
        });
    }

    [Test]
    public void Select_Where() {
        AssertPieces(new[] { Select, Where }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [Select, Where]"
        });
        AssertPieces(new[] { Where, Select }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [Where, Select]"
        });
    }

    [Test]
    public void Select_OfType() {
        AssertPieces(new[] { Select, OfType }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [Select, OfType]"
        });
        AssertPieces(new[] { OfType, Select }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [OfType, Select]"
        });
    }

    [Test]
    public void Where_Cast() {
        AssertPieces(new[] { Cast, Where }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [Cast, Where]"
        });
        AssertPieces(new[] { Where, Cast }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [Where, Cast]"
        });
    }

    [Test]
    public void Select_TakeWhile() {
        AssertPieces(new[] { Select, TakeWhile }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [Select, TakeWhile]"
        });
        AssertPieces(new[] { TakeWhile, Select }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [TakeWhile, Select]"
        });
    }

    [Test]
    public void Select_SkipWhile() {
        AssertPieces(new[] { Select, SkipWhile }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [Select, SkipWhile]"
        });
        AssertPieces(new[] { SkipWhile, Select }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [SkipWhile, Select]"
        });
    }

    [Test]
    public void Array_Where_OrderBy_Select_Where_OrderByDescending_ToDictionaryAndHashSet(
[Values(ToValueType.ToHashSet, ToValueType.ToDictionary)] ToValueType toValueType
) {
        AssertPieces(new[] { Where, OrderBy, Select, Where, OrderByDescending }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [Where]",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]",
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [Select, Where, Identity]"
        }, SourceType.Array, toValueType);
    }

    [Test]
    public void Array_OrderBy_Cast_OrderByDescending_ToDictionaryAndHashSet(
[Values(ToValueType.ToHashSet, ToValueType.ToDictionary)] ToValueType toValueType
) {
        AssertPieces(new[] { OrderBy, Cast, OrderByDescending }, new[] {
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]",
"KnownType: False, KnownSize: True, LoopType: Forward, Nodes: [Cast, Identity]",
        }, SourceType.Array, toValueType);
    }

    [Test]
    public void Array_Where_Cast_OrderByDescending_ToDictionaryAndHashSet(
[Values(ToValueType.ToHashSet, ToValueType.ToDictionary)] ToValueType toValueType
) {
        AssertPieces(new[] { Where, Cast, OrderByDescending }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [Where, Cast, Identity]",
        }, SourceType.Array, toValueType);
    }

    static ToValueType[] AggregateTypes = new[] { ToValueType.Aggregate_Seed };

    [TestCaseSource(nameof(AggregateTypes))]
    public void Where_Aggregate(ToValueType aggregateType) {
        AssertPieces(new[] { Where }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [Where]"
        }, toValueType: aggregateType);
    }

    [TestCaseSource(nameof(AggregateTypes))]
    public void OrderBy_Aggregate(ToValueType aggregateType) {
        AssertPieces(new[] { OrderBy }, new[] {
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]"
        }, toValueType: aggregateType);
    }

    [TestCaseSource(nameof(AggregateTypes))]
    public void CustomEnumerable_OrderBy_Aggregate(ToValueType aggregateType) {
        AssertPieces(new[] { OrderBy }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: []",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy]"
        }, SourceType.CustomEnumerable, toValueType: aggregateType);
    }

    [TestCaseSource(nameof(AggregateTypes))]
    public void SelectMany_SkipWhile_Aggregate(ToValueType aggregateType) {
        AssertPieces(new[] { SelectMany(SourceType.Array), SkipWhile }, new[] {
"KnownType: False, KnownSize: False, LoopType: Forward, Nodes: [SelectMany Array, SkipWhile]"
        }, toValueType: aggregateType);
    }

    [TestCaseSource(nameof(AggregateTypes))]
    public void Where_OrderBy_ThenBy_Aggregate(ToValueType aggregateType) {
        AssertPieces(new[] { Where, OrderBy, ThenBy }, new[] {
"KnownType: True, KnownSize: False, LoopType: Forward, Nodes: [Where]",
"KnownType: True, KnownSize: True, LoopType: Sort, Nodes: [OrderBy, ThenBy]"
        }, toValueType: aggregateType);
    }

    static void AssertPieces(LinqNode[] chain, string[] expected, SourceType sourceType = SourceType.List, ToValueType toValueType = ToValueType.ToArray) {
        var context = chain.Cast<IntermediateNode>().Skip(1).Aggregate(
            EmitContext.Root(sourceType, (IntermediateNode)chain.First()), 
            (acc, c) => acc.Next(c)
        );
        var result = context.GetPieces(sourceType, toValueType).ToArray().Select(x => x.ToString()).ToArray();
        CollectionAssert.AreEqual(expected, result);
    }
}
