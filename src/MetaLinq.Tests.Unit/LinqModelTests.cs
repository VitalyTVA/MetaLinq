namespace MetaLinqTests.Unit;

[TestFixture]
public class LinqModelTests {
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
    public void DuplicateChains() {
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

    static void AssertModel(LinqModel model, string expected) {
        Assert.AreEqual(expected, model.ToString());
    }
}
