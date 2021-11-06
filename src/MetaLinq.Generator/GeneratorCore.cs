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
            builder.AppendMultipleLines(
@"using System;
using System.Collections;
using System.Collections.Generic;
using System.Buffers;
namespace MetaLinq {");
            foreach(var node in tree.GetNodes()) {
                switch(node) {
                    case IntermediateNode intermediate:
                        EmitIntermediate(source, builder.Tab, intermediate);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            builder.AppendLine("}");
        }

        static void EmitIntermediate(SourceType source, CodeBuilder builder, IntermediateNode intermediate) {
            builder.AppendMultipleLines(
@"static partial class MetaEnumerable {");
            var sourceName = source.ToString();

            var enumerableSourceType = source switch {
                SourceType.List => "List<TSource>",
                SourceType.Array => "TSource[]",
                _ => throw new NotImplementedException(),
            };
            var enumerableKind = intermediate switch {
                WhereNode => "Where",
                SelectNode => "Select",
                _ => throw new NotImplementedException(),
            };
            var additionalTypeArgs = intermediate switch {
                WhereNode => null,
                SelectNode => ", TResult",
                _ => throw new NotImplementedException(),
            };
            var argumentName = intermediate switch {
                WhereNode => "predicate",
                SelectNode => "selector",
                _ => throw new NotImplementedException(),
            };
            var argumentType = intermediate switch {
                WhereNode => "Func<TSource, bool>",
                SelectNode => "Func<TSource, TResult>",
                _ => throw new NotImplementedException(),
            };

            builder.Tab.AppendMultipleLines(
$@"public static {sourceName}{enumerableKind}Enumerable<TSource{additionalTypeArgs}> {enumerableKind}<TSource{additionalTypeArgs}>(this {enumerableSourceType} source, {argumentType} {argumentName})
    => new {sourceName}{enumerableKind}Enumerable<TSource{additionalTypeArgs}>(source, {argumentName});");

            builder.AppendLine("}");

            var nodes = intermediate.GetNodes().ToList();

            builder.AppendMultipleLines(
$@"
struct {sourceName}{enumerableKind}Enumerable<TSource{additionalTypeArgs}> {(nodes.Contains(TerminalNode.Enumerable) ? ": IEnumerable<TSource>" : null)} {{
    public readonly {enumerableSourceType} source;
    public readonly {argumentType} {argumentName};
    public {sourceName}{enumerableKind}Enumerable({enumerableSourceType} source, {argumentType} {argumentName}) {{
        this.source = source;
        this.{argumentName} = {argumentName};
    }}");

            foreach(var node in nodes) {
                switch(node) {
                    case TerminalNode { Type: TerminalNodeType.ToArray }:
                        EmitToArray(source, builder.Tab, intermediate);
                        break;
                    case TerminalNode { Type: TerminalNodeType.Enumerable }:
                        EmitGetEnumerator(source, builder.Tab, sourceName);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            builder.AppendLine("}");
        }

        static void EmitGetEnumerator(SourceType source, CodeBuilder builder, string sourceName) {
            var countName = source switch {
                SourceType.List => "Count",
                SourceType.Array => "Length",
                _ => throw new NotImplementedException(),
            };
            builder.AppendMultipleLines(
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
        }

        static void EmitToArray(SourceType source, CodeBuilder builder, IntermediateNode intermediate) {
            var resultType = intermediate switch {
                WhereNode => "TSource",
                SelectNode => "TResult",
                _ => throw new NotImplementedException(),
            };

            builder.AppendMultipleLines(
$@"public {resultType}[] ToArray() {{
    using var result = new LargeArrayBuilder<{resultType}>(ArrayPool<{resultType}>.Shared, false);");

            if(source == SourceType.Array)
                builder.Tab.AppendMultipleLines(
@"var len = source.Length;
for(int i = 0; i < len; i++) {
    var item = source[i];");
            if(source == SourceType.List)
                builder.Tab.AppendMultipleLines(
@"foreach(var item in source) {");

            switch(intermediate) {
                case WhereNode:
                    builder.AppendMultipleLines(
@"        if(predicate(item)) {
            result.Add(item);
        }");
                    break;
                case SelectNode:
                    builder.AppendMultipleLines(
@"        result.Add(selector(item));");
                    break;
                default:
                    throw new NotImplementedException();

            }

            builder.AppendMultipleLines(
@"    }
    return result.ToArray();
}");
        }
    }
}
