namespace MetaLinqTests.Unit;

[TestFixture]
public class CodeBuilderTests {
    StringBuilder sb = new();
    CodeBuilder builder = new(new StringBuilder());

    [SetUp]
    public void SetUp() {
        sb = new StringBuilder();
        builder = new CodeBuilder(sb);
    }

    [Test]
    public void SourceBuilderTest_Append() {
        Assert.AreSame(builder, builder.Append("123").Append("456"));
        AssertBuilderResult("123456");
    }

    [Test]
    public void SourceBuilderTest_AppendTab() {
        Assert.AreSame(builder.Tab, builder.Tab);
        Assert.AreSame(builder.Tab.Tab, builder.Tab.Tab);
        Assert.AreSame(builder.Tab, builder.Tab.Append("123").Append("456"));
        AssertBuilderResult("    123456");

        builder.Tab.AppendLine();
        AssertBuilderResult("    123456\r\n");

        Assert.AreSame(builder.Tab.Tab, builder.Tab.Tab.Append("789"));
        AssertBuilderResult("    123456\r\n        789");
    }

    [Test]
    public void SourceBuilderTest_Append_Range() {
        Assert.AreSame(builder, builder.Append("_123_", 1, 3).Append("_456_", 1, 3));
        AssertBuilderResult("123456");
    }

    [Test]
    public void SourceBuilderTest_AppendTab_Range() {
        Assert.AreSame(builder.Tab, builder.Tab);
        Assert.AreSame(builder.Tab.Tab, builder.Tab.Tab);
        Assert.AreSame(builder.Tab, builder.Tab.Append("_123_", 1, 3).Append("_456_", 1, 3));
        AssertBuilderResult("    123456");
    }

    [Test]
    public void SourceBuilderTest_Append_Char() {
        Assert.AreSame(builder, builder.Append('1').Append('2'));
        AssertBuilderResult("12");
    }

    [Test]
    public void SourceBuilderTest_AppendTab_Char() {
        Assert.AreSame(builder.Tab, builder.Tab);
        Assert.AreSame(builder.Tab.Tab, builder.Tab.Tab);
        Assert.AreSame(builder.Tab, builder.Tab.Append('1').Append('2'));
        AssertBuilderResult("    12");
    }

    [Test]
    public void SourceBuilderTest_AppendLine() {
        Assert.AreSame(builder, builder.Append("123").AppendLine());
        AssertBuilderResult("123\r\n");

        Assert.AreSame(builder, builder.AppendLine());
        AssertBuilderResult("123\r\n\r\n");
    }

    [Test]
    public void SourceBuilderTest_AppendLine_Tab() {
        Assert.AreSame(builder.Tab, builder.Tab.Append("123").AppendLine());
        AssertBuilderResult("    123\r\n");

        Assert.AreSame(builder.Tab, builder.Tab.AppendLine());
        AssertBuilderResult("    123\r\n\r\n");

        Assert.AreSame(builder.Tab, builder.Tab.Append("456").AppendLine());
        AssertBuilderResult("    123\r\n\r\n    456\r\n");
    }

    [Test]
    public void SourceBuilderTest_Return() {
        Assert.Null(builder.Return);
        Assert.AreSame(builder, builder.Tab.Return);
        Assert.AreSame(builder.Tab, builder.Tab.Tab.Return);
    }

    [Test]
    public void SourceBuilderTest_TabOnlyInNewLineState() {
        builder.Append("123");
        var tabbed = builder.Tab;
        var tabbed2 = builder.Tab.Tab;
        Assert.Throws<InvalidOperationException>(() => tabbed.Append("456"));

        builder.AppendLine();
        tabbed.Append("456");
        AssertBuilderResult("123\r\n    456");

        Assert.Throws<InvalidOperationException>(() => tabbed2.Append("456"));
    }
    [Test]
    public void SourceBuilderTest_AppendMultipleLines_TrimLeadingWhiteSpace() {
        builder.Tab.Tab.AppendMultipleLines("    \t   \t abc\r\n123\r\n    xyz", trimLeadingWhiteSpace: true);
        AssertBuilderResult("        abc\r\n        123\r\n        xyz\r\n");
    }
    [Test]
    public void SourceBuilderTest_AppendMultipleLines_SkipEmptyLines() {
        builder.Tab.Tab.AppendMultipleLines("\r\nabc\r\n123\r\n\r\n    xyz\r\n");
        AssertBuilderResult("        abc\r\n        123\r\n            xyz\r\n");
    }
    [Test]
    public void LineEnumeratorTest_TrimLeadingWhiteSpace() {
        AssertRanges(new[] { (10, 3), (15, 3), (24, 3) }, "    \t   \t abc\r\n123\r\n    xyz", trimLeadingWhiteSpace: true);
    }

    void AssertBuilderResult(string expectedResult) {
        Assert.AreEqual(expectedResult, sb.ToString());
    }

    [Test]
    public void LineEnumeratorTest() {
        AssertRanges(new (int, int)[0], string.Empty);
        AssertRanges(new[] { (0, 1) }, "a");
        AssertRanges(new[] { (0, 3) }, "abc");
        AssertRanges(new[] { (0, 3), (5, 2) }, "abc\r\nde");
        AssertRanges(new[] { (0, 3) }, "a\rc");
        AssertRanges(new[] { (0, 3) }, "a\nc");
    }
    static void AssertRanges((int start, int count)[] expected, string str, bool trimLeadingWhiteSpace = false) {
        CollectionAssert.AreEqual(expected, GetRanges(str, trimLeadingWhiteSpace).ToArray());
    }
    static IEnumerable<(int start, int count)> GetRanges(string str, bool trimLeadingWhiteSpace) {
        foreach(var range in new CodeBuilderExtensions.LineEnumerator(str, trimLeadingWhiteSpace: trimLeadingWhiteSpace)) {
            yield return range;
        }
    }

    [Test]
    public void BuildNamepace() {
        using(builder.BuildNamespace(out CodeBuilder nsBuilder, "Foo.Bar")) {
            nsBuilder.AppendLine("content");
        }
        AssertBuilderResult("namespace Foo.Bar {\r\n    content\r\n}\r\n");
    }

    [TestCase(TypeModifiers.Class, "class")]
    [TestCase(TypeModifiers.StaticClass, "static class")]
    [TestCase(TypeModifiers.Struct, "struct")]
    [TestCase(TypeModifiers.ReadonlyStruct, "readonly struct")]
    [Test]
    public void BuildType(TypeModifiers modifier, string expectedModifier) {
        using(builder.BuildType(out CodeBuilder nsBuilder, modifier, "FooBar")) {
            nsBuilder.AppendLine("content");
        }
        AssertBuilderResult($"{expectedModifier} FooBar {{\r\n    content\r\n}}\r\n");
    }

    [Test]
    public void BuildPartialClass() {
        using(builder.BuildType(out CodeBuilder nsBuilder, TypeModifiers.StaticClass, "FooBar", partial: true)) {
            nsBuilder.AppendLine("content");
        }
        AssertBuilderResult($"static partial class FooBar {{\r\n    content\r\n}}\r\n");
    }
    [Test]
    public void BuildPublicClass() {
        using(builder.BuildType(out CodeBuilder nsBuilder, TypeModifiers.StaticClass, "FooBar", isPublic: true)) {
            nsBuilder.AppendLine("content");
        }
        AssertBuilderResult($"public static class FooBar {{\r\n    content\r\n}}\r\n");
    }
    [Test]
    public void BuildClassWithGenericsAndBaseType() {
        using(builder.BuildType(out CodeBuilder nsBuilder, TypeModifiers.Class, "FooBar", generics: "T1, T2", baseType: "IDisposable")) {
            nsBuilder.AppendLine("content");
        }
        AssertBuilderResult($"class FooBar<T1, T2> : IDisposable {{\r\n    content\r\n}}\r\n");
    }
}
