namespace MetaLinqTests.Unit;

[TestFixture]
public class LinqModelTests : BaseFixture {
    [Test]
    public void Empty() {
        var model = new LinqModel();
        AssertModel(model, @"");
    }

    [Test]
    public void ToArray() {
        var model = new LinqModel();
        model.AddChain(SourceType.List, new[] { ChainElement.ToArray });
        AssertModel(model,
@"List
    Root
        -ToArray");
    }

    [Test]
    public void ToArrayAndToList() {
        var model = new LinqModel();
        model.AddChain(SourceType.List, new[] { ChainElement.ToArray });
        model.AddChain(SourceType.List, new[] { ChainElement.ToList });
        AssertModel(model,
@"List
    Root
        -ToArray
        -ToList");
    }

    [Test]
    public void Where() {
        var model = new LinqModel();
        model.AddChain(SourceType.List, new[] { ChainElement.Where });
        AssertModel(model,
@"List
    Root
        Where
            -Enumerable");
    }

    [Test]
    public void Select() {
        var model = new LinqModel();
        model.AddChain(SourceType.List, new[] { ChainElement.Select });
        AssertModel(model,
@"List
    Root
        Select
            -Enumerable");
    }

    [Test]
    public void OrderBy() {
        var model = new LinqModel();
        model.AddChain(SourceType.List, new[] { ChainElement.Select });
        model.AddChain(SourceType.List, new[] { ChainElement.OrderBy });
        model.AddChain(SourceType.List, new[] { ChainElement.OrderBy, ChainElement.ToArray });
        AssertModel(model,
@"List
    Root
        Select
            -Enumerable
        OrderBy
            -ToArray
            -Enumerable");
    }
    [Test]
    public void OrderByDescending() {
        var model = new LinqModel();
        model.AddChain(SourceType.List, new[] { ChainElement.Select });
        model.AddChain(SourceType.List, new[] { ChainElement.OrderByDescending });
        model.AddChain(SourceType.List, new[] { ChainElement.OrderByDescending, ChainElement.ToArray });
        AssertModel(model,
@"List
    Root
        Select
            -Enumerable
        OrderByDescending
            -ToArray
            -Enumerable");
    }

    [Test]
    public void SelectMany_Array() {
        var model = new LinqModel();
        model.AddChain(SourceType.List, new[] { ChainElement.SelectMany(SourceType.Array) });
        AssertModel(model,
@"List
    Root
        SelectMany Array
            -Enumerable");
    }

    [Test]
    public void SelectMany_List() {
        var model = new LinqModel();
        model.AddChain(SourceType.Array, new[] { ChainElement.SelectMany(SourceType.List) });
        AssertModel(model,
@"Array
    Root
        SelectMany List
            -Enumerable");
    }

    [Test]
    public void SelectMany_Mixed1() {
        var model = new LinqModel();
        model.AddChain(SourceType.List, new[] { ChainElement.SelectMany(SourceType.Array) });
        model.AddChain(SourceType.List, new[] { ChainElement.SelectMany(SourceType.List) });
        model.AddChain(SourceType.List, new[] { ChainElement.SelectMany(SourceType.List) });
        AssertModel(model,
@"List
    Root
        SelectMany List
            -Enumerable
        SelectMany Array
            -Enumerable");
    }

    [Test]
    public void SelectMany_Mixed2() {
        var model = new LinqModel();
        model.AddChain(SourceType.List, new[] { ChainElement.SelectMany(SourceType.List) });
        model.AddChain(SourceType.List, new[] { ChainElement.SelectMany(SourceType.Array) });
        AssertModel(model,
@"List
    Root
        SelectMany List
            -Enumerable
        SelectMany Array
            -Enumerable");
    }

    [Test]
    public void SelectMany_Mixed3() {
        var model = new LinqModel();
        model.AddChain(SourceType.List, new[] { ChainElement.SelectMany(SourceType.Array) });
        model.AddChain(SourceType.List, new[] { ChainElement.Select, ChainElement.SelectMany(SourceType.Array) });
        model.AddChain(SourceType.List, new[] { ChainElement.Select, ChainElement.SelectMany(SourceType.List) });
        model.AddChain(SourceType.List, new[] { ChainElement.SelectMany(SourceType.Array), ChainElement.Where });
        model.AddChain(SourceType.List, new[] { ChainElement.Select, ChainElement.SelectMany(SourceType.Array) });
        model.AddChain(SourceType.List, new[] { ChainElement.SelectMany(SourceType.Array), ChainElement.Where });
        AssertModel(model,
@"List
    Root
        Select
            SelectMany List
                -Enumerable
            SelectMany Array
                -Enumerable
        SelectMany Array
            Where
                -Enumerable
            -Enumerable");
    }

    [Test]
    public void WhereToArray() {
        var model = new LinqModel();
        model.AddChain(SourceType.Array, new[] { ChainElement.Where, ChainElement.ToArray });
        AssertModel(model,
@"Array
    Root
        Where
            -ToArray");
    }

    [Test]
    public void WhereToArrayAndToList() {
        var model = new LinqModel();
        model.AddChain(SourceType.Array, new[] { ChainElement.Where, ChainElement.ToList });
        model.AddChain(SourceType.Array, new[] { ChainElement.Where, ChainElement.ToArray });
        AssertModel(model,
@"Array
    Root
        Where
            -ToArray
            -ToList");
    }

    [Test]
    public void WhereAndSelect() {
        var model = new LinqModel();
        model.AddChain(SourceType.Array, new[] { ChainElement.Where });
        model.AddChain(SourceType.Array, new[] { ChainElement.Select });
        AssertModel(model,
@"Array
    Root
        Where
            -Enumerable
        Select
            -Enumerable");
    }

    [Test]
    public void SelectToArray() {
        var model = new LinqModel();
        model.AddChain(SourceType.Array, new[] { ChainElement.Select, ChainElement.ToArray });
        AssertModel(model,
@"Array
    Root
        Select
            -ToArray");
    }

    [Test]
    public void WhereSelectToList() {
        var model = new LinqModel();
        model.AddChain(SourceType.Array, new[] { ChainElement.Where, ChainElement.Select, ChainElement.ToList });
        AssertModel(model,
@"Array
    Root
        Where
            Select
                -ToArray
                -ToList");
    }

    [Test]
    public void MultipleTrunks() {
        var model = new LinqModel();
        model.AddChain(SourceType.Array, new[] { ChainElement.Where, ChainElement.Select, ChainElement.ToList });
        model.AddChain(SourceType.Array, new[] { ChainElement.Where, ChainElement.Select, ChainElement.ToArray });
        model.AddChain(SourceType.Array, new[] { ChainElement.Where, ChainElement.Where, ChainElement.ToList });
        model.AddChain(SourceType.Array, new[] { ChainElement.Where, ChainElement.Where });
        model.AddChain(SourceType.Array, new[] { ChainElement.Select });
        model.AddChain(SourceType.Array, new[] { ChainElement.Select, ChainElement.ToList });
        model.AddChain(SourceType.Array, new[] { ChainElement.Select, ChainElement.ToArray });
        AssertModel(model,
@"Array
    Root
        Where
            Where
                -ToArray
                -ToList
                -Enumerable
            Select
                -ToArray
                -ToList
        Select
            -ToArray
            -ToList
            -Enumerable");
    }

    [Test]
    public void MultipleSources() {
        var model = new LinqModel();
        model.AddChain(SourceType.Array, new[] { ChainElement.Where });
        model.AddChain(SourceType.List, new[] { ChainElement.Where, ChainElement.ToArray });
        AssertModel(model,
@"List
    Root
        Where
            -ToArray
Array
    Root
        Where
            -Enumerable");
    }

    [Test]
    public void Where_WhereToArray() {
        var model = new LinqModel();
        model.AddChain(SourceType.Array, new[] { ChainElement.Where });
        model.AddChain(SourceType.Array, new[] { ChainElement.Where, ChainElement.ToArray });
        AssertModel(model,
@"Array
    Root
        Where
            -ToArray
            -Enumerable");
    }

    [Test]
    public void DuplicateChains1() {
        var model = new LinqModel();
        model.AddChain(SourceType.Array, new[] { ChainElement.Where });
        model.AddChain(SourceType.Array, new[] { ChainElement.Where, ChainElement.ToArray });
        model.AddChain(SourceType.Array, new[] { ChainElement.Where });
        model.AddChain(SourceType.Array, new[] { ChainElement.Where, ChainElement.ToArray });
        AssertModel(model,
@"Array
    Root
        Where
            -ToArray
            -Enumerable");
    }

    [Test]
    public void DuplicateChains2() {
        var model = new LinqModel();
        model.AddChain(SourceType.Array, new[] { ChainElement.Select, ChainElement.Where, ChainElement.ToArray });
        model.AddChain(SourceType.Array, new[] { ChainElement.Where, ChainElement.Select, ChainElement.ToArray });
        model.AddChain(SourceType.Array, new[] { ChainElement.Where, ChainElement.ToArray });
        model.AddChain(SourceType.Array, new[] { ChainElement.Where });
        model.AddChain(SourceType.List, new[] { ChainElement.Where, ChainElement.ToArray });
        model.AddChain(SourceType.List, new[] { ChainElement.Where });
        model.AddChain(SourceType.Array, new[] { ChainElement.Select, ChainElement.ToArray });
        model.AddChain(SourceType.Array, new[] { ChainElement.Select });
        model.AddChain(SourceType.List, new[] { ChainElement.Select, ChainElement.ToArray});
        model.AddChain(SourceType.List, new[] { ChainElement.Select });
        model.AddChain(SourceType.List, new[] { ChainElement.OrderBy, ChainElement.ToArray });
        model.AddChain(SourceType.List, new[] { ChainElement.OrderBy });
        model.AddChain(SourceType.List, new[] { ChainElement.OrderByDescending, ChainElement.ToArray });
        model.AddChain(SourceType.List, new[] { ChainElement.OrderByDescending });

        AssertModel(model,
@"List
    Root
        Where
            -ToArray
            -Enumerable
        Select
            -ToArray
            -Enumerable
        OrderBy
            -ToArray
            -Enumerable
        OrderByDescending
            -ToArray
            -Enumerable
Array
    Root
        Where
            Select
                -ToArray
            -ToArray
            -Enumerable
        Select
            Where
                -ToArray
            -ToArray
            -Enumerable");
    }

    static void AssertModel(LinqModel model, string expected) {
        Assert.AreEqual(expected, model.ToString());
    }

    [Test]
    public void NodeKeyTests() { 
        void AssertNodeComparison(int expected, NodeKey key1, NodeKey key2) { 
            Assert.AreEqual(expected, Comparer<NodeKey>.Default.Compare(key1, key2));
        }
        AssertNodeComparison(0, NodeKey.Simple(NodeType.Select), NodeKey.Simple(NodeType.Select));
        AssertNodeComparison(1, NodeKey.Simple(NodeType.Select), NodeKey.Simple(NodeType.Where));
        AssertNodeComparison(-1, NodeKey.Simple(NodeType.Where), NodeKey.Simple(NodeType.Select));

        AssertNodeComparison(0, NodeKey.Terminal(TerminalNodeType.ToList), NodeKey.Terminal(TerminalNodeType.ToList));
        AssertNodeComparison(1, NodeKey.Terminal(TerminalNodeType.ToList), NodeKey.Terminal(TerminalNodeType.ToArray));
        AssertNodeComparison(-1, NodeKey.Terminal(TerminalNodeType.ToArray), NodeKey.Terminal(TerminalNodeType.ToList));

        AssertNodeComparison(0, NodeKey.SelectMany(SourceType.Array), NodeKey.SelectMany(SourceType.Array));
        AssertNodeComparison(1, NodeKey.SelectMany(SourceType.Array), NodeKey.SelectMany(SourceType.List));
        AssertNodeComparison(-1, NodeKey.SelectMany(SourceType.List), NodeKey.SelectMany(SourceType.Array));

        AssertNodeComparison(1, NodeKey.Terminal(TerminalNodeType.ToList), NodeKey.Simple(NodeType.Select));
        AssertNodeComparison(-1, NodeKey.Simple(NodeType.Select), NodeKey.Terminal(TerminalNodeType.ToList));

        AssertNodeComparison(1, NodeKey.Terminal(TerminalNodeType.ToList), NodeKey.SelectMany(SourceType.Array));
        AssertNodeComparison(-1, NodeKey.SelectMany(SourceType.Array), NodeKey.Terminal(TerminalNodeType.ToList));

        AssertNodeComparison(1, NodeKey.SelectMany(SourceType.Array), NodeKey.Simple(NodeType.Select));
        AssertNodeComparison(-1, NodeKey.Simple(NodeType.Select), NodeKey.SelectMany(SourceType.Array));

        AssertNodeComparison(0, NodeKey.Simple(NodeType.OrderBy), NodeKey.Simple(NodeType.OrderBy));
        AssertNodeComparison(-1, NodeKey.Simple(NodeType.Select), NodeKey.Simple(NodeType.OrderBy));
        AssertNodeComparison(1, NodeKey.Simple(NodeType.OrderBy), NodeKey.Simple(NodeType.Select));

        AssertNodeComparison(0, NodeKey.Simple(NodeType.OrderByDescending), NodeKey.Simple(NodeType.OrderByDescending));
        AssertNodeComparison(-1, NodeKey.Simple(NodeType.Select), NodeKey.Simple(NodeType.OrderByDescending));
        AssertNodeComparison(1, NodeKey.Simple(NodeType.OrderByDescending), NodeKey.Simple(NodeType.Select));
    }
}
