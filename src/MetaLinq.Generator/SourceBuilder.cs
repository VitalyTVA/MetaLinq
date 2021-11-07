using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLinq.Generator {
    public static class SourceBuilder {
        public static IEnumerable<(string name, string source)> BuildSource(LinqModel model) {
            StringBuilder source = new();
            CodeBuilder builder = new(source);

            foreach(var (sourceType, tree) in model.GetTrees()) {
                BuildSource(sourceType, tree, builder);
                yield return ($"Meta_{sourceType}.cs", source.ToString());
                source.Clear();
            }
        }

        private static void BuildSource(SourceType source, RootNode tree, CodeBuilder builder) {
            builder.AppendMultipleLines(@"
using System;
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
            builder.AppendMultipleLines(@"
static partial class MetaEnumerable {");
            var sourceName = source.GetEnumerableSourceName();

            var enumerableSourceType = source switch {
                SourceType.List => "List<TSource>",
                SourceType.Array => "TSource[]",
                _ => throw new NotImplementedException(),
            };
            var enumerableKind = intermediate.GetEnumerableKind();
            var additionalTypeArgs = intermediate.GetAdditionalTypeArgs();
            var enumeratorType = intermediate.GetEnumeratorType();
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

            builder.Tab.AppendMultipleLines($@"
public static {sourceName}{enumerableKind}Enumerable<TSource{additionalTypeArgs}> {enumerableKind}<TSource{additionalTypeArgs}>(this {enumerableSourceType} source, {argumentType} {argumentName})
    => new {sourceName}{enumerableKind}Enumerable<TSource{additionalTypeArgs}>(source, {argumentName});");

            builder.AppendLine("}");

            var nodes = intermediate.GetNodes().ToList();

            builder.AppendMultipleLines($@"
struct {sourceName}{enumerableKind}Enumerable<TSource{additionalTypeArgs}> {(nodes.Contains(TerminalNode.Enumerable) ? $": IEnumerable<{enumeratorType}>" : null)} {{
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
                        EmitGetEnumerator(source, builder.Tab, intermediate);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            builder.AppendLine("}");
        }

        static void EmitGetEnumerator(SourceType source, CodeBuilder builder, IntermediateNode intermediate) {
            var countName = source switch {
                SourceType.List => "Count",
                SourceType.Array => "Length",
                _ => throw new NotImplementedException(),
            };
            var enumeratorType = intermediate.GetEnumeratorType();
            var sourceName = source.GetEnumerableSourceName();
            var enumerableKind = intermediate.GetEnumerableKind();
            var additionalTypeArgs = intermediate.GetAdditionalTypeArgs();
            builder.AppendMultipleLines($@"
#nullable disable
public struct Enumerator : IEnumerator<{enumeratorType}> {{
    {sourceName}{enumerableKind}Enumerable<TSource{additionalTypeArgs}> source;
    int index;
    {enumeratorType} current;
    public Enumerator({sourceName}{enumerableKind}Enumerable<TSource{additionalTypeArgs}> source) {{
        this.source = source;
        index = -1;
        current = default;
    }}
    public {enumeratorType} Current => current;
    public bool MoveNext() {{
        var len = source.source.{countName};
        while(true) {{
            index++;
            if(index >= len)
                break;");

            switch(intermediate) {
                case WhereNode:
                    builder.AppendMultipleLines($@"
            if(source.predicate(source.source[index])) {{
                current = source.source[index];
                return true;
            }}");
                    break;
                case SelectNode:
                    builder.AppendMultipleLines($@"
            current = source.selector(source.source[index]);
                return true;");
                    break;
                default:
                    throw new NotImplementedException();

            }

            builder.AppendMultipleLines($@"
        }}
        return false;
    }}
    public void Dispose() {{ }}
    public void Reset() {{ }}
    object IEnumerator.Current => throw new NotImplementedException();
}}
#nullable restore
public Enumerator GetEnumerator() => new Enumerator(this);
IEnumerator<{enumeratorType}> IEnumerable<{enumeratorType}>.GetEnumerator() {{
    return new Enumerator(this);
}}
IEnumerator IEnumerable.GetEnumerator() {{
    throw new NotImplementedException();
}}
");
        }

        static void EmitToArray(SourceType source, CodeBuilder builder, IntermediateNode intermediate) {
            var resultType = intermediate switch {
                WhereNode => "TSource",
                SelectNode => "TResult",
                _ => throw new NotImplementedException(),
            };

            builder.AppendMultipleLines($@"
public {resultType}[] ToArray() {{
    using var result = new LargeArrayBuilder<{resultType}>(ArrayPool<{resultType}>.Shared, false);");

            if(source == SourceType.Array)
                builder.Tab.AppendMultipleLines(@"
var len = source.Length;
for(int i = 0; i < len; i++) {
    var item = source[i];");
            if(source == SourceType.List)
                builder.Tab.AppendMultipleLines(@"
foreach(var item in source) {");

            switch(intermediate) {
                case WhereNode:
                    builder.AppendMultipleLines(@"
        if(predicate(item)) {
            result.Add(item);
        }");
                    break;
                case SelectNode:
                    builder.AppendMultipleLines(@"
        result.Add(selector(item));");
                    break;
                default:
                    throw new NotImplementedException();

            }

            builder.AppendMultipleLines(@"
    }
    return result.ToArray();
}");
        }
    }
    static class CodeGenerationTraits {
        public static string GetEnumerableSourceName(this SourceType source) {
            return source switch {
                SourceType.List => "List",
                SourceType.Array => "Array",
                _ => throw new NotImplementedException(),
            };
        }
        public static string GetEnumerableKind(this IntermediateNode intermediate) {
            return intermediate switch {
                WhereNode => "Where",
                SelectNode => "Select",
                _ => throw new NotImplementedException(),
            };
        }
        public static string? GetAdditionalTypeArgs(this IntermediateNode intermediate) {
            return intermediate switch {
                WhereNode => null,
                SelectNode => ", TResult",
                _ => throw new NotImplementedException(),
            };
        }
        public static string GetEnumeratorType(this IntermediateNode intermediate) {
            return intermediate switch {
                WhereNode => "TSource",
                SelectNode => "TResult",
                _ => throw new NotImplementedException(),
            };
        }
    }
}
