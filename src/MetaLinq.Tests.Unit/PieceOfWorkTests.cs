namespace MetaLinqTests.Unit;

[TestFixture]
public class PieceOfWorkTests {
    [Test]
    public void OrderBy() {
        AssertPieces(new[] { ChainElement.OrderBy }, new[] {
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]"
        });
    }

    [Test]
    public void OrderBy_Select() {
        AssertPieces(new[] { ChainElement.OrderBy, ChainElement.Select }, new[] {
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: True, ResultType: ToArray, Nodes: [Select]"
        });
    }

    [Test]
    public void OrderBy_Select_OrderBy() {
        AssertPieces(new[] { ChainElement.OrderBy, ChainElement.Select, ChainElement.OrderBy }, new[] {
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: True, ResultType: OrderBy, Nodes: [Select, OrderBy]"
        });
    }

    [Test]
    public void Select() {
        AssertPieces(new[] { ChainElement.Select }, new[] {
"SameType: False, SameSize: True, ResultType: ToArray, Nodes: [Select]"
        });
    }

    [Test]
    public void Select_OrderBy() {
        AssertPieces(new[] { ChainElement.Select, ChainElement.OrderBy }, new[] {
"SameType: False, SameSize: True, ResultType: OrderBy, Nodes: [Select, OrderBy]"
        });
    }

    [Test]
    public void SelectMany() {
        AssertPieces(new[] { ChainElement.SelectMany(SourceType.List) }, new[] {
"SameType: False, SameSize: False, ResultType: ToArray, Nodes: [SelectMany List]"
        });
    }

    [Test]
    public void Where_OrderBy() {
        AssertPieces(new[] { ChainElement.Where, ChainElement.OrderBy }, new[] {
"SameType: True, SameSize: False, ResultType: ToArray, Nodes: [Where]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]"
        });
    }

    [Test]
    public void Where_Select_OrderBy() {
        AssertPieces(new[] { ChainElement.Where, ChainElement.Select, ChainElement.OrderBy }, new[] {
"SameType: False, SameSize: False, ResultType: ToArray, Nodes: [Where, Select]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]"
        });
    }

    [Test]
    public void Where_Select_OrderBy_SelectMany_Where() {
        AssertPieces(new[] { ChainElement.Where, ChainElement.Select, ChainElement.OrderBy, ChainElement.SelectMany(SourceType.List), ChainElement.Where}, new[] {
"SameType: False, SameSize: False, ResultType: ToArray, Nodes: [Where, Select]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: False, ResultType: ToArray, Nodes: [SelectMany List, Where]",
        });
    }

    [Test]
    public void Where_OrderBy_Select() {
        AssertPieces(new[] { ChainElement.Where, ChainElement.OrderBy, ChainElement.Select }, new[] {
"SameType: True, SameSize: False, ResultType: ToArray, Nodes: [Where]",
"SameType: True, SameSize: True, ResultType: OrderBy, Nodes: [OrderBy]",
"SameType: False, SameSize: True, ResultType: ToArray, Nodes: [Select]",
        });
    }

    [Test]
    public void Select_OrderBy_Where() {
        AssertPieces(new[] { ChainElement.Select, ChainElement.OrderBy, ChainElement.Where }, new[] {
"SameType: False, SameSize: True, ResultType: OrderBy, Nodes: [Select, OrderBy]",
"SameType: True, SameSize: False, ResultType: ToArray, Nodes: [Where]",
        });
    }


    [Test]
    public void Where() {
        AssertPieces(new[] { ChainElement.Where }, new[] {
"SameType: True, SameSize: False, ResultType: ToArray, Nodes: [Where]"
        });
    }

    [Test]
    public void Select_Where() {
        AssertPieces(new[] { ChainElement.Select, ChainElement.Where }, new[] {
"SameType: False, SameSize: False, ResultType: ToArray, Nodes: [Select, Where]"
        });
        AssertPieces(new[] { ChainElement.Where, ChainElement.Select }, new[] {
"SameType: False, SameSize: False, ResultType: ToArray, Nodes: [Where, Select]"
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
        var result = lastContext.GetPieces().Select(x => x.ToString()).ToArray();
        CollectionAssert.AreEqual(expected, result);
    }
}
