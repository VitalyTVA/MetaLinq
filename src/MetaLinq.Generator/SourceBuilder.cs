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
using MetaLinq.Internal;");
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
                var context = EmitContext.Root(source, intermediate);
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
                        case TerminalNode { Type: TerminalNodeType.ToList }:
                            EmitToList(source, structBuilder, context);
                            break;
                        case TerminalNode { Type: TerminalNodeType.Enumerable }:
                            EmitGetEnumerator(source, structBuilder, context);
                            break;
                        case IntermediateNode nextIntermediate:
                            var nextContext = context.Next(nextIntermediate);
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
            var countName = source.GetCountName();
            var outputType = context.GetOutputType();
            var enumerableKind = intermediate.GetEnumerableKind();
            var ownTypeArgsList = context.GetOwnTypeArgsList();
            var contexts = context.GetReversedContexts().ToArray();
            var enumerableTypeName = $"{intermediate.GetEnumerableTypeName(context.Level)}{ownTypeArgsList}";
            builder.AppendLine("#nullable disable");
            var selectManyLevels = contexts
                .Select((x, i) => (x, i))
                .Where(x => x.x.Node is SelectManyNode)
                .Select(x => (index: x.i + 1, node: (SelectManyNode)x.x.Node, outputType: x.x.GetOutputType()))
                .ToArray();
            using(builder.BuildType(out CodeBuilder enumeratorBuilder, TypeModifiers.Struct, "Enumerator", isPublic: true, baseType: $"IEnumerator<{outputType}>")) {
                enumeratorBuilder.AppendMultipleLines($@"
readonly {enumerableTypeName} source;
int i0;
{string.Concat(selectManyLevels.Select(x => $"int i{x.index};\r\n"))}
{string.Concat(selectManyLevels.Select(x => $"{x.node.SourceType.GetSourceTypeName(x.outputType)} source{x.index};\r\n"))}
{outputType} current;
int state;
public {CodeGenerationTraits.EnumeratorTypeName}({enumerableTypeName} source) {{
    this.source = source;
    i0 = -1;
{string.Concat(selectManyLevels.Select(x => $"    i{x.index} = -1;\r\n    source{x.index} = default;"))}
    current = default;
    state = -1;
}}
public {outputType} Current => current;
public bool MoveNext() {{
    if(state == 0) //in progress
        goto next{(selectManyLevels.Any() ? selectManyLevels.Last().index : 0)};
    if(state == -1) //start
        goto next0;
    return false; //finished
next0:
    i0++;
    var source0 = this.source{CodeGenerationTraits.GetSourcePath(contexts.Length)};
    if(i0 == source0.{countName}) {{
        state = 1;
        return false;
    }}
    var item0 = source0[i0];");
                foreach(var item in contexts) {
                    EmitEnumeratorLevel(enumeratorBuilder.Tab, item, contexts.Length);
                }
                builder.Tab.Tab.AppendLine($"current = item{contexts.Length};");
                enumeratorBuilder.AppendMultipleLines($@"
    state = 0;
    return true;
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

        static void EmitEnumeratorLevel(CodeBuilder builder, EmitContext context, int totalLevels) {
            var level = context.Level;
            var intermediate = context.Node;
            var sourcePath = CodeGenerationTraits.GetSourcePath(totalLevels - 1 - level);
            switch(intermediate) {
                case WhereNode:
                    builder.AppendMultipleLines($@"
var item{level + 1} = item{level};
if(!source{sourcePath}.predicate(item{level + 1}))
    goto next{context.GetLabelIndex(skip: 0)};");
                    break;
                case SelectNode:
                    builder.AppendLine($@"var item{level + 1} = source{sourcePath}.selector(item{level});");
                    break;
                case SelectManyNode selectMany:
                    builder.Return!.AppendMultipleLines($@"
    source{level + 1} = source{sourcePath}.selector(item{level});
    i{level + 1} = -1;
next{level + 1}:
    i{level + 1}++;
    if(i{level + 1} == source{level + 1}.{selectMany.SourceType.GetCountName()})
        goto next{context.GetLabelIndex(skip: 1)};
    var item{level + 1} = source{level + 1}[i{level + 1}];");
                    break;
                default:
                    throw new NotImplementedException();

            }
        }

        static void EmitLoop(SourceType source, CodeBuilder builder, int level, string sourceExpression, Action<CodeBuilder> emitBody) {
            builder.AppendLine($"var source{level} = {sourceExpression};");
            if(source == SourceType.Array) {
                builder.AppendMultipleLines($@"
var len{level} = source{level}.Length;
for(int i{level} = 0; i{level} < len{level}; i{level}++) {{
    var item{level} = source{level}[i{level}];");
            }
            if(source == SourceType.List)
                builder.AppendMultipleLines($@"
int i{level} = 0;
foreach(var item{level} in source{level}) {{");
            emitBody(builder.Tab);
            if(source == SourceType.List)
                builder.AppendLine($"i{level}++;");
            builder.AppendLine("}");
        }

        static void EmitToArray(SourceType source, CodeBuilder builder, EmitContext context) {
            IntermediateNode intermediate = context.Node;
            var outputType = context.GetOutputType();

            var sourcePath = CodeGenerationTraits.GetSourcePath(context.Level + 1);

            builder.AppendLine($"public {outputType}[] ToArray() {{");

            foreach(var piece in context.GetPieces()) {
                bool first = piece.Contexts.First().Level == 0;
                EmitPieceOrWork(
                    first ? source : SourceType.Array, 
                    builder,
                    first ? "this" + sourcePath : "result_" + (piece.Contexts.First().Level - 1), 
                    piece,
                    context.Level);
            }
            builder.Tab.AppendLine($"return result_{context.Level};");
            builder.AppendLine("}");
        }

        private static void EmitPieceOrWork(SourceType source, CodeBuilder builder, string sourcePath, PieceOfWork piece, int totalLevels) {
            var builderType = (piece.SameSize, piece.ResultType);
            var lastContext = piece.Contexts.Last();
            var outputType = lastContext.GetOutputType();

            var topLevel = piece.Contexts.First().Level;
            var lastLevel = piece.Contexts.Last().Level;

            string arrayBuilder = builderType switch {
                (false, ResultType.ToArray) => $"using var result{topLevel} = new LargeArrayBuilder<{outputType}>();",
                (true, ResultType.ToArray) => $"var result{topLevel} = new {outputType}[{sourcePath}.{source.GetCountName()}];",
                (true, ResultType.OrderBy) =>
@$"var result{topLevel} = {(piece.SameType ? sourcePath : $"new {lastContext.SourceGenericArg}[{sourcePath}.{source.GetCountName()}]")};
var sortKeys{topLevel} = new {"Result".GetLevelGenericType(lastContext.Level)}[{sourcePath}.{source.GetCountName()}];
var map{topLevel} = ArrayPool<int>.Shared.Rent({sourcePath}.{source.GetCountName()});",
                _ => throw new NotImplementedException(),
            };
            builder.Tab.AppendMultipleLines(arrayBuilder);

            EmitLoop(source, builder.Tab, topLevel, sourcePath,
                bodyBuilder => EmitLoopBody(topLevel, bodyBuilder, piece.Contexts, (b, level) => {
                    string addValue = builderType switch {
                        (false, ResultType.ToArray) => $"result{topLevel}.Add(item{level});",
                        (true, ResultType.ToArray) => $"result{topLevel}[i{topLevel}] = item{level};",
                        (true, ResultType.OrderBy) => $"sortKeys{topLevel}[i{topLevel}] = item{level}; map{topLevel}[i{topLevel}] = i{topLevel};{(piece.SameType ? null : $"result{topLevel}[i{topLevel}] = item{level - 1};")}",
                        _ => throw new NotImplementedException(),
                    };
                    b.AppendLine(addValue);
                }, totalLevels));

            string result = builderType switch {
                (false, ResultType.ToArray) => $@"var result_{lastLevel} = result{topLevel}.ToArray();",
                (true, ResultType.ToArray) => $@"var result_{lastLevel} = result{topLevel};",
                (true, ResultType.OrderBy) =>
$@"ArrayPool<int>.Shared.Return(map{topLevel});
var result_{lastLevel} = SortHelper.Sort(result{topLevel}, map{topLevel}, sortKeys{topLevel}, descending: {(lastContext.Node is OrderByDescendingNode ? "true" : "false")});",
                _ => throw new NotImplementedException(),
            };

            builder.Tab.AppendMultipleLines(result);
        }

        static void EmitToList(SourceType source, CodeBuilder builder, EmitContext context) {
            var outputType = context.GetOutputType();
            builder.AppendLine($@"public List<{outputType}> ToList() => Utils.AsList(ToArray());");
        }
        static void EmitLoopBody(int level, CodeBuilder builder, EmitContext[] contexts, Action<CodeBuilder, int> finish, int totalLevels) {
            if(level > contexts.Last().Level) {
                finish(builder, level);
                return;
            }
            void EmitNext(CodeBuilder nextBuilder) => EmitLoopBody(level + 1, nextBuilder, contexts, finish, totalLevels);
            var intermediate = contexts[level - contexts.First().Level].Node;
            var sourcePath = CodeGenerationTraits.GetSourcePath(totalLevels - level);
            switch(intermediate) {
                case WhereNode:
                    builder.AppendMultipleLines($@"
var item{level + 1} = item{level};
if(!this{sourcePath}.predicate(item{level + 1}))
    continue;");
                    EmitNext(builder);
                    break;
                case SelectNode:
                    builder.AppendLine($@"var item{level + 1} = this{sourcePath}.selector(item{level});");
                    EmitNext(builder);
                    break;
                case SelectManyNode selectMany:
                    EmitLoop(selectMany.SourceType, builder, level + 1, $"this{sourcePath}.selector(item{level})",
                        bodyBuilder => EmitNext(bodyBuilder));
                    break;
                case OrderByNode or OrderByDescendingNode:
                    builder.AppendLine($"var item{level + 1} = keySelector(item{level});");
                    EmitNext(builder);
                    break;
                default:
                    throw new NotImplementedException();

            }
        }
    }

    public record EmitContext(int Level, IntermediateNode Node, string SourceType, string SourceGenericArg, EmitContext? Parent) {
        const string RootSourceType = "TSource";

        public static EmitContext Root(SourceType source, IntermediateNode Node) 
            => new EmitContext(0, Node, source.GetSourceTypeName(RootSourceType), RootSourceType, null);

        public EmitContext Next(IntermediateNode node)
            => new EmitContext(Level + 1, node, Node.GetEnumerableTypeName(Level) + this.GetOwnTypeArgsList(), this.GetOutputType(), this);
    }

    public static class CodeGenerationTraits {
        public static string GetEnumerableTypeName(this IntermediateNode intermediate, int level) {
            var enumerableKind = intermediate.GetEnumerableKind();
            var sourceTypePart = intermediate switch {
                SelectManyNode selectMany => "_" + selectMany.SourceType.GetEnumerableSourceNameShort(),
                _ => null
            };
            return enumerableKind + "En" + sourceTypePart + level;
        }
        public static IEnumerable<EmitContext> GetReversedContexts(this EmitContext context) {
            return context.GetContexts().Reverse();
        }
        public static IEnumerable<EmitContext> GetContexts(this EmitContext context) {
            return Extensions.Unfold(context, x => x.Parent);
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
                SelectNode or OrderByNode or OrderByDescendingNode => $"Func<{inType}, {outType}>",
                SelectManyNode selectMany => $"Func<{inType}, {selectMany.SourceType.GetSourceTypeName(outType)}>",
                _ => throw new NotImplementedException(),
            };
        }
        public static string GetLevelGenericType(this string name, int level) => $"T{level}_{name}";
        public const string RootStaticTypePrefix = "Meta_";
        public static string GetEnumerableSourceName(this SourceType source) {
            return RootStaticTypePrefix + source.GetEnumerableSourceNameShort();
        }
        public static string GetEnumerableSourceNameShort(this SourceType source) {
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
                SelectManyNode => "SelectMany",
                OrderByNode => "OrderBy",
                OrderByDescendingNode => "OrderByDescending",
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
                SelectNode or SelectManyNode or OrderByNode or OrderByDescendingNode => argName,
                _ => throw new NotImplementedException(),
            };
        }

        public static string GetOutputType(this EmitContext context) {
            return context.Node switch {
                WhereNode or OrderByNode or OrderByDescendingNode => context.SourceGenericArg,
                SelectNode or SelectManyNode => "Result".GetLevelGenericType(context.Level),
                _ => throw new NotImplementedException(),
            };
        }
        public static string GetArgumentName(this IntermediateNode intermediate) {
            return intermediate switch {
                WhereNode => "predicate",
                SelectNode or SelectManyNode => "selector",
                OrderByNode or OrderByDescendingNode => "keySelector",
                _ => throw new NotImplementedException(),
            };
        }
        public static string GetCountName(this SourceType source) {
            return source switch {
                SourceType.List => "Count",
                SourceType.Array => "Length",
                _ => throw new NotImplementedException(),
            };
        }
        public static int GetLabelIndex(this EmitContext context, int skip) {
            return context.GetContexts().Where(x => x.Node is SelectManyNode).Skip(skip).FirstOrDefault()?.Level + 1 ?? 0;
        }
    }
}
