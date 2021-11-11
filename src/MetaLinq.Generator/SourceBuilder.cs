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
using System.Buffers;");
            using(builder.BuildNamespace(out CodeBuilder nsBuilder, "MetaLinq")) {
                foreach(var node in tree.GetNodes()) {
                    switch(node) {
                        case IntermediateNode intermediate:
                            EmitIntermediate(source, nsBuilder, intermediate);
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }
        }
        static void EmitIntermediate(SourceType source, CodeBuilder builder, IntermediateNode intermediate) {
            EmitExtensionMethod(source, builder, intermediate);
            var sourceGenericArg = "TSource";
            using (builder.BuildType(out CodeBuilder sourceTypeBuilder, 
                TypeModifiers.StaticClass, 
                source.GetEnumerableSourceName(), 
                partial: true, 
                generics: sourceGenericArg)
            ) {
                var enumerableSourceType = source.GetRootSourceType(sourceGenericArg);
                var context = new EmitContext(0, intermediate, enumerableSourceType, sourceGenericArg, null);
                EmitStruct(source, sourceTypeBuilder, context);
            }
        }
        static void EmitExtensionMethod(SourceType source, CodeBuilder builder, IntermediateNode intermediate)
        {
            var sourceName = source.GetEnumerableSourceName();
            var argumentName = intermediate.GetArgumentName();
            var enumerableKind = intermediate.GetEnumerableKind();
            var methodArgumentType = intermediate switch {
                WhereNode => "Func<TSource, bool>",
                SelectNode => "Func<TSource, TResult>",
                _ => throw new NotImplementedException(),
            };
            var methodEnumerableSourceType = source.GetRootSourceType("TSource");
            var ownTypeArg = intermediate.GetOwnTypeArg("TResult");
            var ownTypeArgsList = CodeGenerationTraits.ToTypeArgsList(ownTypeArg);
            using (builder.BuildType(out CodeBuilder classBuilder, TypeModifiers.StaticClass, "MetaEnumerable", partial: true))
            {
                classBuilder.AppendMultipleLines($@"
public static {sourceName}<TSource>.{enumerableKind}En{ownTypeArgsList} {enumerableKind}<TSource{(ownTypeArg != null ? ", " + ownTypeArg : null)}>(this {methodEnumerableSourceType} source, {methodArgumentType} {argumentName})
    => new {sourceName}<TSource>.{enumerableKind}En{ownTypeArgsList}(source, {argumentName});");
            }
        }
        static void EmitStructMethod(string outputType, CodeBuilder builder, IntermediateNode intermediate) {
            var argumentName = intermediate.GetArgumentName();
            var enumerableKind = intermediate.GetEnumerableKind();
            var ownTypeArg = intermediate.GetOwnTypeArg("TResult");
            var ownTypeArgsList = CodeGenerationTraits.ToTypeArgsList(ownTypeArg);
            var methodArgumentType = intermediate switch {
                WhereNode => $"Func<{outputType}, bool>",
                SelectNode => $"Func<{outputType}, TResult>",
                _ => throw new NotImplementedException(),
            };
            builder.AppendLine($"public {enumerableKind}En{ownTypeArgsList} {enumerableKind}{ownTypeArgsList}({methodArgumentType} {argumentName}) => new {enumerableKind}En{ownTypeArgsList}(this, {argumentName});");
        }

        static void EmitStruct(SourceType source, CodeBuilder builder, EmitContext context) {
            IntermediateNode intermediate = context.Node;
            var argumentName = intermediate.GetArgumentName();
            var argumentType = intermediate switch {
                WhereNode => $"Func<{context.SourceGenericArg}, bool>",
                SelectNode => $"Func<{context.SourceGenericArg}, {"Result".GetLevelGenericType(context.Level)}>",
                _ => throw new NotImplementedException(),
            };
            var enumerableKind = intermediate.GetEnumerableKind();
            var nodes = intermediate.GetNodes().ToList();
            bool implementIEnumerable = nodes.Contains(TerminalNode.Enumerable);
            var outputType = context.GetOutputType();
            string? generics = context.GetOwnTypeArg();
            string typeName = enumerableKind + "En";
            using (builder.BuildType(out CodeBuilder structBuilder,
                TypeModifiers.ReadonlyStruct,
                typeName,
                isPublic: true,
                generics: generics,
                baseType: implementIEnumerable ? $"IEnumerable<{outputType}>" : null)
            ) {
                structBuilder.AppendMultipleLines($@"
readonly {context.SourceType} source;
readonly {argumentType} {argumentName};
public {enumerableKind}En({context.SourceType} source, {argumentType} {argumentName}) {{
    this.source = source;
    this.{argumentName} = {argumentName};
}}");

                foreach(var node in nodes) {
                    switch(node) {
                        case TerminalNode { Type: TerminalNodeType.ToArray }:
                            EmitToArray(source, structBuilder, context);
                            break;
                        case TerminalNode { Type: TerminalNodeType.Enumerable }:
                            EmitGetEnumerator(source, structBuilder, context);
                            break;
                        case IntermediateNode nextIntermediate:
                            EmitStructMethod(outputType, structBuilder, nextIntermediate);
                            var nextContext = new EmitContext(context.Level + 1, nextIntermediate, $"{typeName}{CodeGenerationTraits.ToTypeArgsList(generics)}", outputType, context);
                            EmitStruct(source, structBuilder, nextContext);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }

            }
        }

        static void EmitGetEnumerator(SourceType source, CodeBuilder builder, EmitContext context) {
            IntermediateNode intermediate = context.Node;
            var countName = source switch {
                SourceType.List => "Count",
                SourceType.Array => "Length",
                _ => throw new NotImplementedException(),
            };
            var outputType = context.GetOutputType();
            var enumerableKind = intermediate.GetEnumerableKind();
            var ownTypeArgsList = CodeGenerationTraits.ToTypeArgsList(context.GetOwnTypeArg());
            var contexts = context.GetReversedContexts();
            //var source = this{ CodeGenerationTraits.GetSourcePath(contexts.Length + 1)}
            builder.AppendLine("#nullable disable");
            using(builder.BuildType(out CodeBuilder enumeratorBuilder, TypeModifiers.Struct, "Enumerator", isPublic: true, baseType: $"IEnumerator<{outputType}>")) {
                enumeratorBuilder.AppendMultipleLines($@"
{enumerableKind}En{ownTypeArgsList} source;
int index;
{outputType} current;
public {CodeGenerationTraits.EnumeratorTypeName}({enumerableKind}En{ownTypeArgsList} source) {{
    this.source = source;
    index = -1;
    current = default;
}}
public {outputType} Current => current;
public bool MoveNext() {{
    var rootSource = source{CodeGenerationTraits.GetSourcePath(contexts.Length)};
    var len = rootSource.{countName};
    while(true) {{
        index++;
        if(index >= len)
            break;
        var item0 = rootSource[index];");
                EmitLoopBody(0, enumeratorBuilder.Tab.Tab, contexts, (b, level) => {
                    b.AppendMultipleLines($"" +
@$"current = item{level};
return true;");
                });
                enumeratorBuilder.AppendMultipleLines($@"
    }}
    return false;
}}
public void Dispose() {{ }}
public void Reset() {{ }}
object IEnumerator.Current => throw new NotImplementedException();");
            }
            builder.AppendLine("#nullable restore");
            builder.AppendMultipleLines($@"
public Enumerator GetEnumerator() => new {CodeGenerationTraits.EnumeratorTypeName}(this);
IEnumerator<{outputType}> IEnumerable<{outputType}>.GetEnumerator() {{
    return new Enumerator(this);
}}
IEnumerator IEnumerable.GetEnumerator() {{
    throw new NotImplementedException();
}}
");
        }

        static void EmitToArray(SourceType source, CodeBuilder builder, EmitContext context) {
            IntermediateNode intermediate = context.Node;
            var outputType = context.GetOutputType();

            builder.AppendMultipleLines($@"
public {outputType}[] ToArray() {{
    using var result = new LargeArrayBuilder<{outputType}>(ArrayPool<{outputType}>.Shared, false);");
            var contexts = context.GetReversedContexts();
            builder.Tab.AppendLine($"var rootSource = this{CodeGenerationTraits.GetSourcePath(contexts.Length)};");
            builder.Tab.AppendLine($"var source = this;");
            if(source == SourceType.Array)
                builder.Tab.AppendMultipleLines($@"
var len = rootSource.Length;
for(int i = 0; i < len; i++) {{
    var item0 = rootSource[i];");
            if(source == SourceType.List)
                builder.Tab.AppendMultipleLines(@"
foreach(var item0 in rootSource) {");

            EmitLoopBody(0, builder.Tab.Tab, contexts, (b, level) => b.AppendLine($"result.Add(item{level});"));
            builder.AppendMultipleLines(@"
    }
    return result.ToArray();
}");
        }

        static void EmitLoopBody(int level, CodeBuilder builder, ReadOnlySpan<EmitContext> contexts, Action<CodeBuilder, int> finish) {
            if(level >= contexts.Length) {
                finish(builder, level);
                return;
            }
            var intermediate = contexts[level].Node;
            var sourcePath = CodeGenerationTraits.GetSourcePath(contexts.Length - 1 - level);
            switch(intermediate) {
                case WhereNode:
                    builder.AppendMultipleLines($@"
var item{level + 1} = item{level};
if(source{sourcePath}.predicate(item{level + 1})) {{");
                    EmitLoopBody(level + 1, builder.Tab, contexts, finish);
                    builder.AppendLine("}"); //use using(builder...) { ... }
                    break;
                case SelectNode:
                    builder.AppendLine($@"var item{level + 1} = source{sourcePath}.selector(item{level});");
                    EmitLoopBody(level + 1, builder, contexts, finish);
                    break;
                default:
                    throw new NotImplementedException();

            }
        }
    }

    public record EmitContext(int Level, IntermediateNode Node, string SourceType, string SourceGenericArg, EmitContext? Parent);

    public static class CodeGenerationTraits {
        public static Span<EmitContext> GetReversedContexts(this EmitContext context) {
            return Extensions.Unfold(context, x => x.Parent).Reverse().ToArray().AsSpan();
        }

        public const string EnumeratorTypeName = "Enumerator";
        public static string GetSourcePath(int count) => count == 0 ? string.Empty : "." + string.Join(".", Enumerable.Repeat("source", count));

        public static string? ToTypeArgsList(string? ownTypeArg) {
            return ownTypeArg == null ? null : $"<{ownTypeArg}>";
        }
        public static string GetRootSourceType(this SourceType source, string sourceGenericArg) {
            return source switch {
                SourceType.List => $"List<{sourceGenericArg}>",
                SourceType.Array => $"{sourceGenericArg}[]",
                _ => throw new NotImplementedException(),
            };
        }
        public static string GetLevelGenericType(this string name, int level) => $"T{level}_{name}";
        public const string RootStaticTypePrefix = "Meta_";
        public static string GetEnumerableSourceName(this SourceType source) {
            return RootStaticTypePrefix + source switch {
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
        public static string? GetOwnTypeArg(this EmitContext context) {
            return context.Node.GetOwnTypeArg("Result".GetLevelGenericType(context.Level));
        }
        public static string? GetOwnTypeArg(this IntermediateNode intermediate, string argName) {
            return intermediate switch {
                WhereNode => null,
                SelectNode => argName,
                _ => throw new NotImplementedException(),
            };
        }
        public static string GetOutputType(this EmitContext context) {
            return context.Node switch {
                WhereNode => context.SourceGenericArg,
                SelectNode => "Result".GetLevelGenericType(context.Level),
                _ => throw new NotImplementedException(),
            };
        }
        public static string GetArgumentName(this IntermediateNode intermediate) {
            return intermediate switch {
                WhereNode => "predicate",
                SelectNode => "selector",
                _ => throw new NotImplementedException(),
            };
        }
    }
}
