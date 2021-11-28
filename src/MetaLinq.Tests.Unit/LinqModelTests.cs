using static MetaLinq.Generator.ChainElement;

namespace MetaLinqTests.Unit;

[TestFixture]
public class LinqModelTests : BaseFixture {
    [Test]
    public void Empty() {
        var model = new LinqModel();
        AssertModel(model, @"");
    }

    [Test]
    public void ToArray_() {
        var model = new LinqModel();
        model.AddChain(SourceType.List, new[] { ToArray });
        AssertModel(model,
@"List
    Root
        -ToArray");
    }

    [Test]
    public void ToArrayAndToList() {
        var model = new LinqModel();
        model.AddChain(SourceType.List, new[] { ToArray });
        model.AddChain(SourceType.List, new[] { ToList });
        AssertModel(model,
@"List
    Root
        -ToArray
        -ToList");
    }

    [Test]
    public void Where_() {
        var model = new LinqModel();
        model.AddChain(SourceType.List, new[] { Where });
        AssertModel(model,
@"List
    Root
        Where
            -Enumerable");
    }

    [Test]
    public void Select_() {
        var model = new LinqModel();
        model.AddChain(SourceType.List, new[] { Select });
        AssertModel(model,
@"List
    Root
        Select
            -Enumerable");
    }

    [Test]
    public void OrderBy_() {
        var model = new LinqModel();
        model.AddChain(SourceType.List, new[] { Select });
        model.AddChain(SourceType.List, new[] { OrderBy });
        model.AddChain(SourceType.List, new[] { OrderBy, ToArray });
        model.AddChain(SourceType.List, new[] { OrderBy, ThenBy });
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
    public void OrderByDescending_() {
        var model = new LinqModel();
        model.AddChain(SourceType.List, new[] { Select });
        model.AddChain(SourceType.List, new[] { OrderByDescending });
        model.AddChain(SourceType.List, new[] { OrderByDescending, ToArray });
        model.AddChain(SourceType.List, new[] { OrderByDescending, ThenByDescending });
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
        model.AddChain(SourceType.List, new[] { SelectMany(SourceType.Array) });
        AssertModel(model,
@"List
    Root
        SelectMany Array
            -Enumerable");
    }

    [Test]
    public void SelectMany_List() {
        var model = new LinqModel();
        model.AddChain(SourceType.Array, new[] { SelectMany(SourceType.List) });
        AssertModel(model,
@"Array
    Root
        SelectMany List
            -Enumerable");
    }

    [Test]
    public void SelectMany_Mixed1() {
        var model = new LinqModel();
        model.AddChain(SourceType.List, new[] { SelectMany(SourceType.Array) });
        model.AddChain(SourceType.List, new[] { SelectMany(SourceType.List) });
        model.AddChain(SourceType.List, new[] { SelectMany(SourceType.List) });
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
        model.AddChain(SourceType.List, new[] { SelectMany(SourceType.List) });
        model.AddChain(SourceType.List, new[] { SelectMany(SourceType.Array) });
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
        model.AddChain(SourceType.List, new[] { SelectMany(SourceType.Array) });
        model.AddChain(SourceType.List, new[] { Select, SelectMany(SourceType.Array) });
        model.AddChain(SourceType.List, new[] { Select, SelectMany(SourceType.List) });
        model.AddChain(SourceType.List, new[] { SelectMany(SourceType.Array), Where });
        model.AddChain(SourceType.List, new[] { Select, SelectMany(SourceType.Array) });
        model.AddChain(SourceType.List, new[] { SelectMany(SourceType.Array), Where });
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
        model.AddChain(SourceType.Array, new[] { Where, ToArray });
        AssertModel(model,
@"Array
    Root
        Where
            -ToArray");
    }

    [Test]
    public void WhereToArrayAndToList() {
        var model = new LinqModel();
        model.AddChain(SourceType.Array, new[] { Where, ToList });
        model.AddChain(SourceType.Array, new[] { Where, ToArray });
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
        model.AddChain(SourceType.Array, new[] { Where });
        model.AddChain(SourceType.Array, new[] { Select });
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
        model.AddChain(SourceType.Array, new[] { Select, ToArray });
        AssertModel(model,
@"Array
    Root
        Select
            -ToArray");
    }

    [Test]
    public void WhereSelectToList() {
        var model = new LinqModel();
        model.AddChain(SourceType.Array, new[] { Where, Select, ToList });
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
        model.AddChain(SourceType.Array, new[] { Where, Select, ToList });
        model.AddChain(SourceType.Array, new[] { Where, Select, ToArray });
        model.AddChain(SourceType.Array, new[] { Where, Where, ToList });
        model.AddChain(SourceType.Array, new[] { Where, Where });
        model.AddChain(SourceType.Array, new[] { Select });
        model.AddChain(SourceType.Array, new[] { Select, ToList });
        model.AddChain(SourceType.Array, new[] { Select, ToArray });
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
        model.AddChain(SourceType.Array, new[] { Where });
        model.AddChain(SourceType.List, new[] { Where, ToArray });
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
        model.AddChain(SourceType.Array, new[] { Where });
        model.AddChain(SourceType.Array, new[] { Where, ToArray });
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
        model.AddChain(SourceType.Array, new[] { Where });
        model.AddChain(SourceType.Array, new[] { Where, ToArray });
        model.AddChain(SourceType.Array, new[] { Where });
        model.AddChain(SourceType.Array, new[] { Where, ToArray });
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
        model.AddChain(SourceType.Array, new[] { Select, Where, ToArray });
        model.AddChain(SourceType.Array, new[] { Where, Select, ToArray });
        model.AddChain(SourceType.Array, new[] { Where, ToArray });
        model.AddChain(SourceType.Array, new[] { Where });
        model.AddChain(SourceType.List, new[] { Where, ToArray });
        model.AddChain(SourceType.List, new[] { Where });
        model.AddChain(SourceType.Array, new[] { Select, ToArray });
        model.AddChain(SourceType.Array, new[] { Select });
        model.AddChain(SourceType.List, new[] { Select, ToArray});
        model.AddChain(SourceType.List, new[] { Select });
        model.AddChain(SourceType.List, new[] { OrderBy, ToArray });
        model.AddChain(SourceType.List, new[] { OrderBy });
        model.AddChain(SourceType.List, new[] { OrderByDescending, ToArray });
        model.AddChain(SourceType.List, new[] { OrderByDescending });

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
        AssertNodeComparison(0, Select, Select);
        AssertNodeComparison(-1, Select, Where);
        AssertNodeComparison(1, Where, Select);

        AssertNodeComparison(0, ToList, ToList);
        AssertNodeComparison(1, ToList, ToArray);
        AssertNodeComparison(-1, ToArray, ToList);

        AssertNodeComparison(0, SelectMany(SourceType.Array), SelectMany(SourceType.Array));
        AssertNodeComparison(-1, SelectMany(SourceType.Array), SelectMany(SourceType.List));
        AssertNodeComparison(1, SelectMany(SourceType.List), SelectMany(SourceType.Array));

        AssertNodeComparison(1, ToList, Select);
        AssertNodeComparison(-1, Select, ToList);

        AssertNodeComparison(1, ToList, SelectMany(SourceType.Array));
        AssertNodeComparison(-1, SelectMany(SourceType.Array), ToList);

        AssertNodeComparison(1, SelectMany(SourceType.Array), Select);
        AssertNodeComparison(-1, Select, SelectMany(SourceType.Array));

        AssertNodeComparison(0, OrderBy, OrderBy);
        AssertNodeComparison(1, Select, OrderBy);
        AssertNodeComparison(-1, OrderBy, Select);

        AssertNodeComparison(0, OrderByDescending, OrderByDescending);
        AssertNodeComparison(1, Select, OrderByDescending);
        AssertNodeComparison(-1, OrderByDescending, Select);
    }
}
