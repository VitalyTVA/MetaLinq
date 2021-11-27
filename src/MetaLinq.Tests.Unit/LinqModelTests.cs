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
        model.AddChain(SourceType.List, new[] { ChainElement.OrderBy, ChainElement.ThenBy });
        AssertModel(model,
@"List
    Root
        OrderBy
            -Enumerable
            ThenBy
                -Enumerable
            -ToArray
        Select
            -Enumerable");
    }
    [Test]
    public void OrderByDescending() {
        var model = new LinqModel();
        model.AddChain(SourceType.List, new[] { ChainElement.Select });
        model.AddChain(SourceType.List, new[] { ChainElement.OrderByDescending });
        model.AddChain(SourceType.List, new[] { ChainElement.OrderByDescending, ChainElement.ToArray });
        model.AddChain(SourceType.List, new[] { ChainElement.OrderByDescending, ChainElement.ThenByDescending });
        AssertModel(model,
@"List
    Root
        OrderByDescending
            -Enumerable
            ThenByDescending
                -Enumerable
            -ToArray
        Select
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
        SelectMany Array
            -Enumerable
        SelectMany List
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
        SelectMany Array
            -Enumerable
        SelectMany List
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
            SelectMany Array
                -Enumerable
            SelectMany List
                -Enumerable
        SelectMany Array
            -Enumerable
            Where
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
        Select
            -Enumerable
        Where
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
        Select
            -Enumerable
            -ToArray
            -ToList
        Where
            Select
                -ToArray
                -ToList
            Where
                -Enumerable
                -ToArray
                -ToList");
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
            -Enumerable
            -ToArray");
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
            -Enumerable
            -ToArray");
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
        OrderBy
            -Enumerable
            -ToArray
        OrderByDescending
            -Enumerable
            -ToArray
        Select
            -Enumerable
            -ToArray
        Where
            -Enumerable
            -ToArray
Array
    Root
        Select
            -Enumerable
            -ToArray
            Where
                -ToArray
        Where
            -Enumerable
            Select
                -ToArray
            -ToArray");
    }

    static void AssertModel(LinqModel model, string expected) {
        Assert.AreEqual(expected, model.ToString());
    }

    [Test]
    public void NodeKeyTests() { 
        void AssertNodeComparison(int expected, ChainElement key1, ChainElement key2) { 
            Assert.AreEqual(expected, ChainElement.Comparer.Compare(key1, key2));
        }
        AssertNodeComparison(0, ChainElement.Select, ChainElement.Select);
        AssertNodeComparison(-1, ChainElement.Select, ChainElement.Where);
        AssertNodeComparison(1, ChainElement.Where, ChainElement.Select);

        AssertNodeComparison(0, ChainElement.ToList, ChainElement.ToList);
        AssertNodeComparison(1, ChainElement.ToList, ChainElement.ToArray);
        AssertNodeComparison(-1, ChainElement.ToArray, ChainElement.ToList);

        AssertNodeComparison(0, ChainElement.SelectMany(SourceType.Array), ChainElement.SelectMany(SourceType.Array));
        AssertNodeComparison(-1, ChainElement.SelectMany(SourceType.Array), ChainElement.SelectMany(SourceType.List));
        AssertNodeComparison(1, ChainElement.SelectMany(SourceType.List), ChainElement.SelectMany(SourceType.Array));

        AssertNodeComparison(1, ChainElement.ToList, ChainElement.Select);
        AssertNodeComparison(-1, ChainElement.Select, ChainElement.ToList);

        AssertNodeComparison(1, ChainElement.ToList, ChainElement.SelectMany(SourceType.Array));
        AssertNodeComparison(-1, ChainElement.SelectMany(SourceType.Array), ChainElement.ToList);

        AssertNodeComparison(1, ChainElement.SelectMany(SourceType.Array), ChainElement.Select);
        AssertNodeComparison(-1, ChainElement.Select, ChainElement.SelectMany(SourceType.Array));

        AssertNodeComparison(0, ChainElement.OrderBy, ChainElement.OrderBy);
        AssertNodeComparison(1, ChainElement.Select, ChainElement.OrderBy);
        AssertNodeComparison(-1, ChainElement.OrderBy, ChainElement.Select);

        AssertNodeComparison(0, ChainElement.OrderByDescending, ChainElement.OrderByDescending);
        AssertNodeComparison(1, ChainElement.Select, ChainElement.OrderByDescending);
        AssertNodeComparison(-1, ChainElement.OrderByDescending, ChainElement.Select);
    }
}
