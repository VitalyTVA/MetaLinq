﻿using System;
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
                var enumerableSourceType = source.GetSourceTypeName(sourceGenericArg);
                var context = new EmitContext(0, intermediate, enumerableSourceType, sourceGenericArg, null);
                EmitStruct(source, sourceTypeBuilder, context);
            }
        }
        static void EmitExtensionMethod(SourceType source, CodeBuilder builder, IntermediateNode intermediate)
        {
            var sourceName = source.GetEnumerableSourceName();
            var argumentName = intermediate.GetArgumentName();
            var enumerableKind = intermediate.GetEnumerableKind();
            var methodArgumentType = intermediate.GetArgumentType("TSource", "TResult");
            var methodEnumerableSourceType = source.GetSourceTypeName("TSource");
            var ownTypeArgsList = intermediate.GetOwnTypeArgsList("TResult");
            var ownTypeArg = intermediate.GetOwnTypeArg("TResult");
            var enumerableTypeName = $"{intermediate.GetEnumerableTypeName(0)}{ownTypeArgsList}";
            
            using (builder.BuildType(out CodeBuilder classBuilder, TypeModifiers.StaticClass, "MetaEnumerable", partial: true))
            {
                classBuilder.AppendMultipleLines($@"
public static {sourceName}<TSource>.{enumerableTypeName} {enumerableKind}<TSource{(ownTypeArg != null ? ", " + ownTypeArg : null)}>(this {methodEnumerableSourceType} source, {methodArgumentType} {argumentName})
    => new {sourceName}<TSource>.{enumerableTypeName}(source, {argumentName});");
            }
        }
        static void EmitStructMethod(CodeBuilder builder, EmitContext context) {
            var intermediate = context.Node;
            var argumentName = context.Node.GetArgumentName();
            var methodArgumentType = intermediate.GetArgumentType(context.SourceGenericArg, "TResult");

            var ownTypeArgsList = intermediate.GetOwnTypeArgsList("TResult");
            var enumerableKind = intermediate.GetEnumerableKind();
            var enumerableTypeName = $"{intermediate.GetEnumerableTypeName(context.Level)}{ownTypeArgsList}";

            builder.AppendLine($"public {enumerableTypeName} {enumerableKind}{ownTypeArgsList}({methodArgumentType} {argumentName}) => new {enumerableTypeName}(this, {argumentName});");
        }

        static void EmitStruct(SourceType source, CodeBuilder builder, EmitContext context) {
            IntermediateNode intermediate = context.Node;
            var argumentName = intermediate.GetArgumentName();
            var argumentType = intermediate.GetArgumentType(context.SourceGenericArg, "Result".GetLevelGenericType(context.Level));
            var nodes = intermediate.GetNodes().ToList();
            bool implementIEnumerable = nodes.Contains(TerminalNode.Enumerable);
            var outputType = context.GetOutputType();
            string typeName = intermediate.GetEnumerableTypeName(context.Level) + context.GetOwnTypeArgsList();
            using (builder.BuildType(out CodeBuilder structBuilder,
                TypeModifiers.ReadonlyStruct,
                typeName,
                isPublic: true,
                baseType: implementIEnumerable ? $"IEnumerable<{outputType}>" : null)
            ) {
                structBuilder.AppendMultipleLines($@"
readonly {context.SourceType} source;
readonly {argumentType} {argumentName};
public {intermediate.GetEnumerableTypeName(context.Level)}({context.SourceType} source, {argumentType} {argumentName}) {{
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
                            var nextContext = new EmitContext(context.Level + 1, nextIntermediate, $"{typeName}", outputType, context);
                            EmitStructMethod(structBuilder, nextContext);
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
            var ownTypeArgsList = context.GetOwnTypeArgsList();
            var contexts = context.GetReversedContexts();
            var enumerableTypeName = $"{intermediate.GetEnumerableTypeName(context.Level)}{ownTypeArgsList}";
            //var source = this{ CodeGenerationTraits.GetSourcePath(contexts.Length + 1)}
            builder.AppendLine("#nullable disable");
            using(builder.BuildType(out CodeBuilder enumeratorBuilder, TypeModifiers.Struct, "Enumerator", isPublic: true, baseType: $"IEnumerator<{outputType}>")) {
                enumeratorBuilder.AppendMultipleLines($@"
{enumerableTypeName} source;
int index;
{outputType} current;
public {CodeGenerationTraits.EnumeratorTypeName}({enumerableTypeName} source) {{
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

        static void EmitLoop(SourceType source, CodeBuilder builder, int level, string sourceExpression, Action<CodeBuilder> emitBody) {
            builder.AppendLine($"var source{level} = {sourceExpression};");
            if(source == SourceType.Array)
                builder.AppendMultipleLines($@"
var len{level} = source{level}.Length;
for(int i{level} = 0; i{level} < len{level}; i{level}++) {{
    var item{level} = source{level}[i{level}];");
            if(source == SourceType.List)
                builder.AppendMultipleLines($@"
foreach(var item{level} in source{level}) {{");
            emitBody(builder.Tab);
            builder.AppendLine("}");
        }

        static void EmitToArray(SourceType source, CodeBuilder builder, EmitContext context) {
            IntermediateNode intermediate = context.Node;
            var outputType = context.GetOutputType();

            builder.AppendMultipleLines($@"
public {outputType}[] ToArray() {{
    using var result = new LargeArrayBuilder<{outputType}>(ArrayPool<{outputType}>.Shared, false);");
            var contexts = context.GetReversedContexts();
            builder.Tab.AppendLine($"var source = this;");

            EmitLoop(source, builder.Tab, 0, "this" + CodeGenerationTraits.GetSourcePath(contexts.Length),
                bodyBuilder => EmitLoopBody(0, bodyBuilder, contexts, (b, level) => b.AppendLine($"result.Add(item{level});")));

            builder.AppendMultipleLines(@"
    return result.ToArray();
}");
        }

        static void EmitLoopBody(int level, CodeBuilder builder, EmitContext[] contexts, Action<CodeBuilder, int> finish) {
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
                    builder.AppendLine("}");
                    break;
                case SelectNode:
                    builder.AppendLine($@"var item{level + 1} = source{sourcePath}.selector(item{level});");
                    EmitLoopBody(level + 1, builder, contexts, finish);
                    break;
                case SelectManyNode selectMany:
                    EmitLoop(selectMany.SourceType, builder, level + 1, $"source{sourcePath}.selector(item{level})",
                        bodyBuilder => EmitLoopBody(level + 1, bodyBuilder.Tab, contexts, finish));
                    break;
                default:
                    throw new NotImplementedException();

            }
        }
    }

    public record EmitContext(int Level, IntermediateNode Node, string SourceType, string SourceGenericArg, EmitContext? Parent);

    public static class CodeGenerationTraits {
        public static string GetEnumerableTypeName(this IntermediateNode intermediate, int level) {
            var enumerableKind = intermediate.GetEnumerableKind();
            var sourceTypePart = intermediate switch {
                SelectManyNode selectMany => "_" + selectMany.SourceType.GetEnumerableSourceName(),
                _ => null
            };
            return enumerableKind + "En" + sourceTypePart + level;
        }
        public static EmitContext[] GetReversedContexts(this EmitContext context) {
            return Extensions.Unfold(context, x => x.Parent).Reverse().ToArray();
        }

        public const string EnumeratorTypeName = "Enumerator";
        public static string GetSourcePath(int count) => count == 0 ? string.Empty : "." + string.Join(".", Enumerable.Repeat("source", count));

        public static string GetSourceTypeName(this SourceType source, string sourceGenericArg) {
            return source switch {
                SourceType.List => $"List<{sourceGenericArg}>",
                SourceType.Array => $"{sourceGenericArg}[]",
                _ => throw new NotImplementedException(),
            };
        }
        public static string GetArgumentType(this IntermediateNode intermediate, string inType, string outType) {
            return intermediate switch {
                WhereNode => $"Func<{inType}, bool>",
                SelectNode => $"Func<{inType}, {outType}>",
                SelectManyNode selectMany => $"Func<{inType}, {selectMany.SourceType.GetSourceTypeName(outType)}>",
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
                SelectManyNode => "SelectMany",
                _ => throw new NotImplementedException(),
            };
        }
        public static string? GetOwnTypeArgsList(this EmitContext context) {
            return context.Node.GetOwnTypeArgsList("Result".GetLevelGenericType(context.Level));
        }
        public static string? GetOwnTypeArgsList(this IntermediateNode intermediate, string argName) {
            var ownTypeArg = intermediate.GetOwnTypeArg(argName);
            return ownTypeArg == null ? null : $"<{ownTypeArg}>";
        }

        public static string? GetOwnTypeArg(this IntermediateNode intermediate, string argName) {
            return intermediate switch {
                WhereNode => null,
                SelectNode or SelectManyNode => argName,
                _ => throw new NotImplementedException(),
            };
        }

        public static string GetOutputType(this EmitContext context) {
            return context.Node switch {
                WhereNode => context.SourceGenericArg,
                SelectNode or SelectManyNode => "Result".GetLevelGenericType(context.Level),
                _ => throw new NotImplementedException(),
            };
        }
        public static string GetArgumentName(this IntermediateNode intermediate) {
            return intermediate switch {
                WhereNode => "predicate",
                SelectNode or SelectManyNode => "selector",
                _ => throw new NotImplementedException(),
            };
        }
    }
}
