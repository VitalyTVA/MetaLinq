using static MetaLinq.Generator.ChainElement;

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
    public void OrderBy_ThenBy() {
        AssertPieces(new[] { OrderBy, ThenBy}, new[] {
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy, ThenBy]"
        });
    }

    [Test]
    public void Where_OrderBy_ThenBy() {
        AssertPieces(new[] { Where, OrderBy, ThenBy }, new[] {
"SameType: True, SameSize: False, ResultType: ToInstance, Nodes: [Where]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy, ThenBy]"
        });
    }

    [Test]
    public void TakeWhile_OrderBy_ThenBy() {
        AssertPieces(new[] { TakeWhile, OrderBy, ThenBy }, new[] {
"SameType: True, SameSize: False, ResultType: ToInstance, Nodes: [TakeWhile]",
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
"SameType: False, SameSize: True, ResultType: ToInstance, Nodes: [Select]"
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
"SameType: False, SameSize: True, ResultType: ToInstance, Nodes: [Select]"
        });
    }

    [Test]
    public void Select_OrderBy() {
        AssertPieces(new[] { Select, OrderBy }, new[] {
"SameType: False, SameSize: True, ResultType: OrderBy, Nodes: [Select, OrderBy]"
        });
    }

    [Test]
    public void SelectMany_() {
        AssertPieces(new[] { SelectMany(SourceType.List) }, new[] {
"SameType: False, SameSize: False, ResultType: ToInstance, Nodes: [SelectMany List]"
        });
    }

    [Test]
    public void Where_OrderBy() {
        AssertPieces(new[] { Where, OrderBy }, new[] {
"SameType: True, SameSize: False, ResultType: ToInstance, Nodes: [Where]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]"
        });
    }

    [Test]
    public void SkipWhile_OrderBy() {
        AssertPieces(new[] { SkipWhile, OrderBy }, new[] {
"SameType: True, SameSize: False, ResultType: ToInstance, Nodes: [SkipWhile]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]"
        });
    }

    [Test]
    public void Where_Select_OrderBy() {
        AssertPieces(new[] { Where, Select, OrderBy }, new[] {
"SameType: False, SameSize: False, ResultType: ToInstance, Nodes: [Where, Select]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]"
        });
    }

    [Test]
    public void TakeWhile_Select_OrderBy() {
        AssertPieces(new[] { TakeWhile, Select, OrderBy }, new[] {
"SameType: False, SameSize: False, ResultType: ToInstance, Nodes: [TakeWhile, Select]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]"
        });
    }

    [Test]
    public void Where_Select_OrderBy_SelectMany_Where() {
        AssertPieces(new[] { Where, Select, OrderBy, SelectMany(SourceType.List), Where}, new[] {
"SameType: False, SameSize: False, ResultType: ToInstance, Nodes: [Where, Select]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: False, ResultType: ToInstance, Nodes: [SelectMany List, Where]",
        });
    }

    [Test]
    public void TakeWhile_Select_OrderBy_SelectMany_Where() {
        AssertPieces(new[] { TakeWhile, Select, OrderBy, SelectMany(SourceType.List), Where }, new[] {
"SameType: False, SameSize: False, ResultType: ToInstance, Nodes: [TakeWhile, Select]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: False, ResultType: ToInstance, Nodes: [SelectMany List, Where]",
        });
    }

    [Test]
    public void SkipWhile_Select_OrderBy_SelectMany_TakeWhile() {
        AssertPieces(new[] { SkipWhile, Select, OrderBy, SelectMany(SourceType.List), TakeWhile }, new[] {
"SameType: False, SameSize: False, ResultType: ToInstance, Nodes: [SkipWhile, Select]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: False, ResultType: ToInstance, Nodes: [SelectMany List, TakeWhile]",
        });
    }

    [Test]
    public void Where_OrderBy_Select() {
        AssertPieces(new[] { Where, OrderBy, Select }, new[] {
"SameType: True, SameSize: False, ResultType: ToInstance, Nodes: [Where]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: True, ResultType: ToInstance, Nodes: [Select]",
        });
    }

    [Test]
    public void TakeWhile_OrderBy_Select() {
        AssertPieces(new[] { TakeWhile, OrderBy, Select }, new[] {
"SameType: True, SameSize: False, ResultType: ToInstance, Nodes: [TakeWhile]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: True, ResultType: ToInstance, Nodes: [Select]",
        });
    }

    [Test]
    public void SkipWhile_OrderBy_Select() {
        AssertPieces(new[] { SkipWhile, OrderBy, Select }, new[] {
"SameType: True, SameSize: False, ResultType: ToInstance, Nodes: [SkipWhile]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: True, ResultType: ToInstance, Nodes: [Select]",
        });
    }

    [Test]
    public void Select_OrderBy_Where() {
        AssertPieces(new[] { Select, OrderBy, Where }, new[] {
"SameType: False, SameSize: True, ResultType: OrderBy, Nodes: [Select, OrderBy]",
"SameType: True, SameSize: False, ResultType: ToInstance, Nodes: [Where]",
        });
    }

    [Test]
    public void Select_OrderBy_TakeWhile() {
        AssertPieces(new[] { Select, OrderBy, TakeWhile }, new[] {
"SameType: False, SameSize: True, ResultType: OrderBy, Nodes: [Select, OrderBy]",
"SameType: True, SameSize: False, ResultType: ToInstance, Nodes: [TakeWhile]",
        });
    }

    [Test]
    public void Select_OrderBy_SkipWhile() {
        AssertPieces(new[] { Select, OrderBy, SkipWhile }, new[] {
"SameType: False, SameSize: True, ResultType: OrderBy, Nodes: [Select, OrderBy]",
"SameType: True, SameSize: False, ResultType: ToInstance, Nodes: [SkipWhile]",
        });
    }

    [Test]
    public void Where_() {
        AssertPieces(new[] { Where }, new[] {
"SameType: True, SameSize: False, ResultType: ToInstance, Nodes: [Where]"
        });
    }

    [Test]
    public void SkipWhile_() {
        AssertPieces(new[] { SkipWhile }, new[] {
"SameType: True, SameSize: False, ResultType: ToInstance, Nodes: [SkipWhile]"
        });
    }

    [Test]
    public void TakeWhile_() {
        AssertPieces(new[] { TakeWhile }, new[] {
"SameType: True, SameSize: False, ResultType: ToInstance, Nodes: [TakeWhile]"
        });
    }

    [Test]
    public void Select_Where() {
        AssertPieces(new[] { Select, Where }, new[] {
"SameType: False, SameSize: False, ResultType: ToInstance, Nodes: [Select, Where]"
        });
        AssertPieces(new[] { Where, Select }, new[] {
"SameType: False, SameSize: False, ResultType: ToInstance, Nodes: [Where, Select]"
        });
    }

    [Test]
    public void Select_TakeWhile() {
        AssertPieces(new[] { Select, TakeWhile }, new[] {
"SameType: False, SameSize: False, ResultType: ToInstance, Nodes: [Select, TakeWhile]"
        });
        AssertPieces(new[] { TakeWhile, Select }, new[] {
"SameType: False, SameSize: False, ResultType: ToInstance, Nodes: [TakeWhile, Select]"
        });
    }

    [Test]
    public void Select_SkipWhile() {
        AssertPieces(new[] { Select, SkipWhile }, new[] {
"SameType: False, SameSize: False, ResultType: ToInstance, Nodes: [Select, SkipWhile]"
        });
        AssertPieces(new[] { SkipWhile, Select }, new[] {
"SameType: False, SameSize: False, ResultType: ToInstance, Nodes: [SkipWhile, Select]"
        });
    }

    static void AssertPieces(ChainElement[] chain, string[] expected) {
        var model = new LinqModel();
        model.AddChain(SourceType.List, chain);
        var firstNode = (IntermediateNode)model.GetTrees().Single().Item2.GetNodes().Single();
        var lastContext = Extensions.Unfold(
            EmitContext.Root(SourceType.List, firstNode),
            context => {
                var node = context.Node.GetNodes().OfType<IntermediateNode>().SingleOrDefault();
                return node != null ? context.Next(node) : null;
            }).Last();
        var result = lastContext.GetPieces().ToArray().Select(x => x.ToString()).ToArray();
        CollectionAssert.AreEqual(expected, result);
    }
}
