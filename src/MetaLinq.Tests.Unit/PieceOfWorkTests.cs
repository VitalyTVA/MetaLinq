using static MetaLinq.Generator.LinqNode;

namespace MetaLinqTests.Unit;

[TestFixture]
public class PieceOfWorkTests {
    [Test]
    public void OrderBy_() {
        AssertPieces(new[] { OrderBy }, new[] {
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]"
        });
    }

    [Test]
    public void CustomEnumerable_OrderBy(
        [Values(ToValueType.ToHashSet, ToValueType.ToArray, ToValueType.ToDictionary)] ToValueType toValueType
    ) {
        AssertPieces(new[] { OrderBy }, new[] {
"SameType: True, SameSize: False, ResultType: ToValue, Nodes: []",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]"
        }, SourceType.CustomEnumerable, toValueType);
    }

    [Test]
    public void CustomEnumerable_OrderBy_First(
        [Values(ToValueType.First, ToValueType.FirstOrDefault)] ToValueType toValueType
    ) {
        AssertPieces(new[] { OrderBy }, new[] {
"SameType: True, SameSize: False, ResultType: OrderBy, Nodes: [OrderBy]"
        }, SourceType.CustomEnumerable, toValueType);
    }

    [Test]
    public void CustomEnumerable_Where_OrderBy_First(
        [Values(ToValueType.First, ToValueType.FirstOrDefault)] ToValueType toValueType
    ) {
        AssertPieces(new[] { Where, OrderBy }, new[] {
"SameType: True, SameSize: False, ResultType: ToValue, Nodes: [Where]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]"
        }, SourceType.CustomEnumerable, toValueType);
    }

    [Test]
    public void CustomEnumerable_OfType_OrderBy_First(
    [Values(ToValueType.First, ToValueType.FirstOrDefault)] ToValueType toValueType
) {
        AssertPieces(new[] { OfType, OrderBy }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [OfType]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]"
        }, SourceType.CustomEnumerable, toValueType);
    }

    [Test]
    public void CustomEnumerable_Cast_OrderBy_First(
[Values(ToValueType.First, ToValueType.FirstOrDefault)] ToValueType toValueType
) {
        AssertPieces(new[] { Cast, OrderBy }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [Cast]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]"
        }, SourceType.CustomEnumerable, toValueType);
    }

    [Test]
    public void Cast_OrderBy_First(
[Values(ToValueType.First, ToValueType.FirstOrDefault)] ToValueType toValueType
) {
        AssertPieces(new[] { Cast, OrderBy }, new[] {
"SameType: False, SameSize: True, ResultType: OrderBy, Nodes: [Cast, OrderBy]"
        }, SourceType.List, toValueType);
    }

    [Test]
    public void Cast_OrderBy() {
        AssertPieces(new[] { Cast, OrderBy }, new[] {
"SameType: False, SameSize: True, ResultType: OrderBy, Nodes: [Cast, OrderBy]"
        }, SourceType.List);
    }

    [Test]
    public void OrderBy_ThenBy() {
        AssertPieces(new[] { OrderBy, ThenBy}, new[] {
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy, ThenBy]"
        });
    }

    [Test]
    public void Where_OrderBy_ThenBy() {
        AssertPieces(new[] { Where, OrderBy, ThenBy }, new[] {
"SameType: True, SameSize: False, ResultType: ToValue, Nodes: [Where]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy, ThenBy]"
        });
    }

    [Test]
    public void TakeWhile_OrderBy_ThenBy() {
        AssertPieces(new[] { TakeWhile, OrderBy, ThenBy }, new[] {
"SameType: True, SameSize: False, ResultType: ToValue, Nodes: [TakeWhile]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy, ThenBy]"
        });
    }

    [Test]
    public void OrderBy_ThenByDescending() {
        AssertPieces(new[] { OrderBy, ThenByDescending }, new[] {
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy, ThenByDescending]"
        });
    }

    [Test]
    public void OrderBy_Select() {
        AssertPieces(new[] { OrderBy, Select }, new[] {
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: True, ResultType: ToValue, Nodes: [Select]"
        });
    }

    [Test]
    public void OrderBy_Select_OrderBy() {
        AssertPieces(new[] { OrderBy, Select, OrderBy }, new[] {
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: True, ResultType: OrderBy, Nodes: [Select, OrderBy]"
        });
    }

    [Test]
    public void OrderBy_Select_OrderByDescending_ThenBy() {
        AssertPieces(new[] { OrderBy, Select, OrderByDescending, ThenBy }, new[] {
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: True, ResultType: OrderBy, Nodes: [Select, OrderByDescending, ThenBy]"
        });
    }

    [Test]
    public void Select_() {
        AssertPieces(new[] { Select }, new[] {
"SameType: False, SameSize: True, ResultType: ToValue, Nodes: [Select]"
        });
    }

    [Test]
    public void Select_CustomEnumerable() {
        AssertPieces(new[] { Select }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [Select]"
        }, SourceType.CustomEnumerable);
    }

    [Test]
    public void Select_OrderBy() {
        AssertPieces(new[] { Select, OrderBy }, new[] {
"SameType: False, SameSize: True, ResultType: OrderBy, Nodes: [Select, OrderBy]"
        });
    }

    [Test]
    public void Select_Select_OrderBy() {
        AssertPieces(new[] { Select, Select, OrderBy }, new[] {
"SameType: False, SameSize: True, ResultType: OrderBy, Nodes: [Select, Select, OrderBy]"
        });
    }

    [Test]
    public void SelectMany_() {
        AssertPieces(new[] { SelectMany(SourceType.List) }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [SelectMany List]"
        });
    }

    [Test]
    public void Where_OrderBy() {
        AssertPieces(new[] { Where, OrderBy }, new[] {
"SameType: True, SameSize: False, ResultType: ToValue, Nodes: [Where]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]"
        });
    }

    [Test]
    public void SkipWhile_OrderBy() {
        AssertPieces(new[] { SkipWhile, OrderBy }, new[] {
"SameType: True, SameSize: False, ResultType: ToValue, Nodes: [SkipWhile]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]"
        });
    }

    [Test]
    public void Where_Select_OrderBy() {
        AssertPieces(new[] { Where, Select, OrderBy }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [Where, Select]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]"
        });
    }

    [Test]
    public void TakeWhile_Select_OrderBy() {
        AssertPieces(new[] { TakeWhile, Select, OrderBy }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [TakeWhile, Select]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]"
        });
    }

    [Test]
    public void Where_Select_OrderBy_SelectMany_Where() {
        AssertPieces(new[] { Where, Select, OrderBy, SelectMany(SourceType.List), Where}, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [Where, Select]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [SelectMany List, Where]",
        });
    }

    [Test]
    public void Where_Select_OrderBy_OfType_Where() {
        AssertPieces(new[] { Where, Select, OrderBy, OfType, Where }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [Where, Select]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [OfType, Where]",
        });
    }

    [Test]
    public void Where_Select_OrderBy_Cast_Where() {
        AssertPieces(new[] { Where, Select, OrderBy, Cast, Where }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [Where, Select]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [Cast, Where]",
        });
    }

    [Test]
    public void TakeWhile_Select_OrderBy_SelectMany_Where() {
        AssertPieces(new[] { TakeWhile, Select, OrderBy, SelectMany(SourceType.List), Where }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [TakeWhile, Select]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [SelectMany List, Where]",
        });
    }

    [Test]
    public void SkipWhile_Select_OrderBy_SelectMany_TakeWhile() {
        AssertPieces(new[] { SkipWhile, Select, OrderBy, SelectMany(SourceType.List), TakeWhile }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [SkipWhile, Select]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [SelectMany List, TakeWhile]",
        });
    }

    [Test]
    public void Where_OrderBy_Select() {
        AssertPieces(new[] { Where, OrderBy, Select }, new[] {
"SameType: True, SameSize: False, ResultType: ToValue, Nodes: [Where]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: True, ResultType: ToValue, Nodes: [Select]",
        });
    }

    [Test]
    public void TakeWhile_OrderBy_Select() {
        AssertPieces(new[] { TakeWhile, OrderBy, Select }, new[] {
"SameType: True, SameSize: False, ResultType: ToValue, Nodes: [TakeWhile]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: True, ResultType: ToValue, Nodes: [Select]",
        });
    }

    [Test]
    public void SkipWhile_OrderBy_Select() {
        AssertPieces(new[] { SkipWhile, OrderBy, Select }, new[] {
"SameType: True, SameSize: False, ResultType: ToValue, Nodes: [SkipWhile]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: True, ResultType: ToValue, Nodes: [Select]",
        });
    }

    [Test]
    public void Select_OrderBy_Where() {
        AssertPieces(new[] { Select, OrderBy, Where }, new[] {
"SameType: False, SameSize: True, ResultType: OrderBy, Nodes: [Select, OrderBy]",
"SameType: True, SameSize: False, ResultType: ToValue, Nodes: [Where]",
        });
    }

    [Test]
    public void Select_OrderBy_TakeWhile() {
        AssertPieces(new[] { Select, OrderBy, TakeWhile }, new[] {
"SameType: False, SameSize: True, ResultType: OrderBy, Nodes: [Select, OrderBy]",
"SameType: True, SameSize: False, ResultType: ToValue, Nodes: [TakeWhile]",
        });
    }

    [Test]
    public void Select_OrderBy_SkipWhile() {
        AssertPieces(new[] { Select, OrderBy, SkipWhile }, new[] {
"SameType: False, SameSize: True, ResultType: OrderBy, Nodes: [Select, OrderBy]",
"SameType: True, SameSize: False, ResultType: ToValue, Nodes: [SkipWhile]",
        });
    }

    [Test]
    public void Where_() {
        AssertPieces(new[] { Where }, new[] {
"SameType: True, SameSize: False, ResultType: ToValue, Nodes: [Where]"
        });
    }

    [Test]
    public void OfType_() {
        AssertPieces(new[] { OfType }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [OfType]"
        });
    }

    [Test]
    public void Cast_() {
        AssertPieces(new[] { Cast }, new[] {
"SameType: False, SameSize: True, ResultType: ToValue, Nodes: [Cast]"
        });
    }

    [Test]
    public void SkipWhile_() {
        AssertPieces(new[] { SkipWhile }, new[] {
"SameType: True, SameSize: False, ResultType: ToValue, Nodes: [SkipWhile]"
        });
    }

    [Test]
    public void TakeWhile_() {
        AssertPieces(new[] { TakeWhile }, new[] {
"SameType: True, SameSize: False, ResultType: ToValue, Nodes: [TakeWhile]"
        });
    }

    [Test]
    public void Select_Where() {
        AssertPieces(new[] { Select, Where }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [Select, Where]"
        });
        AssertPieces(new[] { Where, Select }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [Where, Select]"
        });
    }

    [Test]
    public void Select_OfType() {
        AssertPieces(new[] { Select, OfType }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [Select, OfType]"
        });
        AssertPieces(new[] { OfType, Select }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [OfType, Select]"
        });
    }

    [Test]
    public void Where_Cast() {
        AssertPieces(new[] { Cast, Where }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [Cast, Where]"
        });
        AssertPieces(new[] { Where, Cast }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [Where, Cast]"
        });
    }

    [Test]
    public void Select_TakeWhile() {
        AssertPieces(new[] { Select, TakeWhile }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [Select, TakeWhile]"
        });
        AssertPieces(new[] { TakeWhile, Select }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [TakeWhile, Select]"
        });
    }

    [Test]
    public void Select_SkipWhile() {
        AssertPieces(new[] { Select, SkipWhile }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [Select, SkipWhile]"
        });
        AssertPieces(new[] { SkipWhile, Select }, new[] {
"SameType: False, SameSize: False, ResultType: ToValue, Nodes: [SkipWhile, Select]"
        });
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
