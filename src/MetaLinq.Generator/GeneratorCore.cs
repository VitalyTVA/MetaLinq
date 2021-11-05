using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MetaLinq.Generator {
    static class MetaLinqGeneratorCore {
        public static void Execute(GeneratorExecutionContext context) {
            //Debugger.Launch();
            if(context.SyntaxContextReceiver is not SyntaxContextReceiver receiver)
                return;

            Compilation compilation = context.Compilation;

            StringBuilder source = new();
            CodeBuilder builder = new(source);

            foreach(var (sourceType, tree) in receiver.Model.GetTrees()) {
                if(context.CancellationToken.IsCancellationRequested)
                    break;
                BuildSource(sourceType, tree, builder);
                context.AddSource($"Meta_{sourceType}.cs", SourceText.From(source.ToString(), Encoding.UTF8));
                source.Clear();
            }
        }

        private static void BuildSource(SourceType source, RootNode tree, CodeBuilder builder) {
            var where = (WhereNode)tree.GetNodes().Single();
            var terminals = where.GetNodes().Cast<TerminalNode>();

            bool toArray = terminals.SingleOrDefault(x => x.Type == TerminalNodeType.ToArray) != null;
            bool iEnumerable = terminals.SingleOrDefault(x => x.Type == TerminalNodeType.Enumerable) != null;

            builder.AppendMultipleLines(
@"using System;
using System.Collections;
using System.Collections.Generic;
using System.Buffers;
namespace MetaLinq {
    static partial class MetaEnumerable {"
);

            var sourceName = source.ToString();
            var enumerableSourceType = source switch {
                SourceType.List => "List<TSource>",
                SourceType.Array => "TSource[]",
                _ => throw new NotImplementedException(),
            };
            builder.Tab.Tab.AppendMultipleLines(
$@"public static {sourceName}WhereEnumerable<TSource> Where<TSource>(this {enumerableSourceType} source, Func<TSource, bool> predicate)
    => new {sourceName}WhereEnumerable<TSource>(source, predicate);");

            builder.Tab.AppendLine("}");


            builder.Tab.AppendMultipleLines(
$@"
struct {sourceName}WhereEnumerable<TSource> {(iEnumerable ? ": IEnumerable<TSource>" : null)} {{
    public readonly {enumerableSourceType} source;
    public readonly Func<TSource, bool> predicate;
    public {sourceName}WhereEnumerable({enumerableSourceType} source, Func<TSource, bool> predicate) {{
        this.source = source;
        this.predicate = predicate;
    }}");
            if(toArray) {
                builder.Tab.Tab.AppendMultipleLines(
@"public TSource[] ToArray() {
    using var result = new LargeArrayBuilder<TSource>(ArrayPool<TSource>.Shared, false);");

                if(source == SourceType.Array)
                    builder.Tab.Tab.Tab.AppendMultipleLines(
@"var len = source.Length;
for(int i = 0; i < len; i++) {
    var item = source[i];");
                if(source == SourceType.List)
                    builder.Tab.Tab.Tab.AppendMultipleLines(
@"foreach(var item in source) {");

                builder.Tab.Tab.AppendMultipleLines(
@"        if(predicate(item)) {
            result.Add(item);
        }
    }
    return result.ToArray();
}");
            }

            var countName = source switch {
                SourceType.List => "Count",
                SourceType.Array => "Length",
                _ => throw new NotImplementedException(),
            };
            if(iEnumerable)
                builder.Tab.Tab.AppendMultipleLines(
$@"public struct Enumerator {{
    {sourceName}WhereEnumerable<TSource> source;
    int index;
    public Enumerator({sourceName}WhereEnumerable<TSource> source) {{
        this.source = source;
        index = -1;
    }}
    public TSource Current => source.source[index];
    public bool MoveNext() {{
        var len = source.source.{countName};
        while(true) {{
            index++;
            if(index >= len)
                break;
            if(source.predicate(source.source[index]))
                return true;
        }}
        return false;
    }}
}}
public Enumerator GetEnumerator() => new Enumerator(this);
IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator() {{
    var len = source.{countName};
    for(int i = 0; i < len; i++) {{
        var item = source[i];
        if(predicate(item)) {{
            yield return item;
        }}
    }}
}}
IEnumerator IEnumerable.GetEnumerator() {{
    throw new NotImplementedException();
}}");

            builder.Tab.AppendLine("}");
            builder.AppendLine("}");
        }
    }
}
