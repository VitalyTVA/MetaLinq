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
            using (builder.BuildType(out CodeBuilder sourceTypeBuilder, 
                TypeModifiers.StaticClass, 
                source.GetEnumerableSourceName(), 
                partial: true, 
                generics: EmitContext.RootSourceType)
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
            var methodArgumentType = intermediate.GetArgumentType(EmitContext.RootSourceType, "TResult");
            var methodEnumerableSourceType = source.GetSourceTypeName(EmitContext.RootSourceType);
            var ownTypeArgsList = intermediate.GetOwnTypeArgsList("TResult");
            var ownTypeArg = intermediate.GetOwnTypeArg("TResult");
            var enumerableTypeName = $"{intermediate.GetEnumerableTypeName(Level.Zero)}{ownTypeArgsList}";
            
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
            var argumentType = intermediate.GetArgumentType(context.SourceGenericArg, context.GetResultGenericType());
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
                        case TerminalNode { Type: TerminalNodeType.ToList }:
                            EmitToList(source, structBuilder, context);
                            break;
                        case TerminalNode { Type: TerminalNodeType.Enumerable }:
                            EmitGetEnumerator(source, structBuilder, context);
                            break;
                        case TerminalNode terminalNode:
                            var toInstanceType = terminalNode.Type switch { 
                                TerminalNodeType.ToArray => ToValueType.ToArray, 
                                TerminalNodeType.ToHashSet => ToValueType.ToHashSet, 
                                TerminalNodeType.ToDictionary => ToValueType.ToDictionary, 
                                _ => throw new InvalidOperationException()
                            };
                            EmitToValue(source, structBuilder, context, toInstanceType);
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
                .Where(x => x.Node is SelectManyNode)
                .Select(x => (index: x.Level.Next, node: (SelectManyNode)x.Node, outputType: x.GetOutputType()))
                .ToArray();
            using(builder.BuildType(out CodeBuilder enumeratorBuilder, TypeModifiers.Struct, "Enumerator", isPublic: true, baseType: $"IEnumerator<{outputType}>")) {
                
                enumeratorBuilder.AppendMultipleLines($@"
readonly {enumerableTypeName} source;
int i{Level.Zero};
{string.Concat(selectManyLevels.Select(x => $"int i{x.index};\r\n"))}
{string.Concat(selectManyLevels.Select(x => $"{x.node.SourceType.GetSourceTypeName(x.outputType)} source{x.index};\r\n"))}
{outputType} current;
int state;
public {CodeGenerationTraits.EnumeratorTypeName}({enumerableTypeName} source) {{
    this.source = source;
    i{Level.Zero} = -1;
{string.Concat(selectManyLevels.Select(x => $"    i{x.index} = -1;\r\n    source{x.index} = default;"))}
    current = default;
    state = -1;
}}
public {outputType} Current => current;
public bool MoveNext() {{
    if(state == 0) //in progress
        goto next{(selectManyLevels.Any() ? selectManyLevels.Last().index : Level.Zero)};
    if(state == -1) //start
        goto next{Level.Zero};
    return false; //finished
next{Level.Zero}:
    i{Level.Zero}++;
    var source{Level.Zero} = this.source{CodeGenerationTraits.GetSourcePath(contexts.Length)};
    if(i{Level.Zero} == source{Level.Zero}.{countName}) {{
        state = 1;
        return false;
    }}
    var item{Level.Zero} = source{Level.Zero}[i{Level.Zero}];");
                var finalLevel = contexts.Last().Level.Next;
                foreach(var item in contexts) {
                    EmitEnumeratorLevel(enumeratorBuilder.Tab, item, finalLevel);
                }
                builder.Tab.Tab.AppendLine($"current = item{finalLevel};");
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

        static void EmitEnumeratorLevel(CodeBuilder builder, EmitContext context, Level totalLevels) {
            var level = context.Level;
            var intermediate = context.Node;
            var sourcePath = CodeGenerationTraits.GetSourcePath(totalLevels.Prev.Minus(level));
            switch(intermediate) {
                case WhereNode:
                    builder.AppendMultipleLines($@"
var item{level.Next} = item{level};
if(!source{sourcePath}.predicate(item{level.Next}))
    goto next{context.GetLabelIndex(skip: 0)};");
                    break;
                case SelectNode:
                    builder.AppendLine($@"var item{level.Next} = source{sourcePath}.selector(item{level});");
                    break;
                case SelectManyNode selectMany:
                    builder.Return!.AppendMultipleLines($@"
    source{level.Next} = source{sourcePath}.selector(item{level});
    i{level.Next} = -1;
next{level.Next}:
    i{level.Next}++;
    if(i{level.Next} == source{level.Next}.{selectMany.SourceType.GetCountName()})
        goto next{context.GetLabelIndex(skip: 1)};
    var item{level.Next} = source{level.Next}[i{level.Next}];");
                    break;
                default:
                    throw new NotImplementedException();

            }
        }

        static void EmitLoop(SourceType source, CodeBuilder builder, Level level, string sourceExpression, Action<CodeBuilder> emitBody) {
            builder.AppendLine($"var source{level} = {sourceExpression};");
            if(source.HasIndexer()) {
                builder.AppendMultipleLines($@"
var len{level} = source{level}.{source.GetCountName()};
for(int i{level} = 0; i{level} < len{level}; i{level}++) {{
    var item{level} = source{level}[i{level}];");
            }
            if(!source.HasIndexer())
                builder.AppendMultipleLines($@"
int i{level} = 0;
foreach(var item{level} in source{level}) {{");
            emitBody(builder.Tab);
            if(!source.HasIndexer())
                builder.AppendLine($"i{level}++;");
            builder.AppendLine("}");
        }
        enum ToValueType { ToArray, ToHashSet, ToDictionary };
        static void EmitToValue(SourceType source, CodeBuilder builder, EmitContext context, ToValueType toInstanceType) {
            IntermediateNode intermediate = context.Node;
            var outputType = context.GetOutputType();

            var sourcePath = CodeGenerationTraits.GetSourcePath(context.Level.Next.Value);

            var methodDefinition = toInstanceType switch {
                ToValueType.ToArray => $"{outputType}[] ToArray()",
                ToValueType.ToHashSet => $"HashSet<{outputType}> ToHashSet()",
                ToValueType.ToDictionary => $"Dictionary<TKey, {outputType}> ToDictionary<TKey>(Func<{outputType}, TKey> keySelector) where TKey : notnull",
                _ => throw new NotImplementedException(),
            };
            builder.AppendLine($"public {methodDefinition} {{");

            var pieces = context.GetPieces(source);
            foreach(var piece in pieces) {
                bool first = pieces.First() == piece;
                bool last = pieces.Last() == piece;
                EmitPieceOrWork(
                    first ? source : SourceType.Array,
                    last ? toInstanceType : default,
                    builder,
                    first ? "this" + sourcePath : "result_" + piece.TopLevel.Prev,
                    piece,
                    context.Level);
            }
            builder.Tab.AppendLine($"return result_{context.Level};");
            builder.AppendLine("}");
        }

        static void EmitPieceOrWork(SourceType source, ToValueType toInstanceType, CodeBuilder builder, string sourcePath, PieceOfWork piece, Level totalLevels) {

            var topLevel = piece.TopLevel;
            var lastLevel = piece.LastLevel;

            var (arrayBuilder, addValue, result) = GetArrayBuilder(source, toInstanceType, sourcePath, piece);
            builder.Tab.AppendMultipleLines(arrayBuilder);

            foreach(var item in piece.Contexts) {
                if(item.Node is SkipWhileNode)
                    builder.Tab.AppendLine($"var skipWhile{item.Level.Next} = true;");
            }

            EmitLoop(source, builder.Tab, topLevel, sourcePath,
                bodyBuilder => EmitLoopBody(topLevel, bodyBuilder, piece, b => b.AppendMultipleLines(addValue), totalLevels));

            foreach(var item in piece.Contexts) {
                if(item.Node is TakeWhileNode)
                    builder.Tab.AppendLine($"takeWhile{item.Level.Next}:");
            }

            builder.Tab.AppendMultipleLines(result);
        }
        static (string init, string add, string result) GetArrayBuilder(SourceType source, ToValueType? toInstanceType, string sourcePath, PieceOfWork piece) {
            var sourceGenericArg = piece.Contexts.LastOrDefault()?.SourceGenericArg ?? EmitContext.RootSourceType;
            var outputType = piece.Contexts.LastOrDefault()?.GetOutputType() ?? EmitContext.RootSourceType;
            var topLevel = piece.TopLevel;
            var lastLevel = piece.LastLevel;

            var capacityExpression = piece.SameSize ? $"{sourcePath}.{source.GetCountName()}" : null;

            switch((piece.SameSize, piece.ResultType, toInstanceType)) {
                case (false, ResultType.ToValue, ToValueType.ToArray):
                    return (
                        $"using var result{topLevel} = new LargeArrayBuilder<{outputType}>();",
                        $"result{topLevel}.Add(item{lastLevel.Next});",
                        $@"var result_{lastLevel} = result{topLevel}.ToArray();"
                    );
                case (true, ResultType.ToValue, ToValueType.ToArray):
                    return (
                        $"var result{topLevel} = new {outputType}[{capacityExpression}];",
                        $"result{topLevel}[i{topLevel}] = item{lastLevel.Next};",
                        $@"var result_{lastLevel} = result{topLevel};"
                    );
                case (_, ResultType.ToValue, ToValueType.ToHashSet):
                    return (
                        $"var result{topLevel} = new HashSet<{outputType}>({capacityExpression});",
                        $"result{topLevel}.Add(item{lastLevel.Next});",
                        $@"var result_{lastLevel} = result{topLevel};"
                    );
                case (_, ResultType.ToValue, ToValueType.ToDictionary):
                    return (
                        $"var result{topLevel} = new Dictionary<TKey, {outputType}>({capacityExpression});",
                        $"result{topLevel}.Add(keySelector(item{lastLevel.Next}), item{lastLevel.Next});",
                        $@"var result_{lastLevel} = result{topLevel};"
                    );
                case (true, ResultType.OrderBy, _):
                    var order = piece.Contexts
                        .SkipWhile(x => x.Node is not (OrderByNode or OrderByDescendingNode or ThenByNode or ThenByDescendingNode))
                        .Select(x => (x.Level, sortKeyGenericType: x.GetResultGenericType(), descending: x.Node is OrderByDescendingNode or ThenByDescendingNode))
                        .ToArray();
                    var sortKeyVars = order.Select((x, i) => {
                        return $"var sortKeys{topLevel}_{i} = new {x.sortKeyGenericType}[{capacityExpression}];\r\n";
                    });
                    var sortKeyAssigns = order.Select((x, i) => {
                        return $"sortKeys{topLevel}_{i}[i{topLevel}] = item{order.First().Level.Offset(i + 1)};\r\n";
                    });
                    List<string> comparerTypes = new();
                    foreach(var (_, sortKeyGenericType, descending) in order.Reverse()) {
                        var last = comparerTypes.LastOrDefault();
                        var suffix = descending ? "Descending" : null;
                        if(last != null)
                            comparerTypes.Add($"KeysComparer{suffix}<{sortKeyGenericType}, {last}>");
                        else
                            comparerTypes.Add($"KeysComparer{suffix}<{sortKeyGenericType}>");
                    }
                    var parts = comparerTypes
                        .ToArray()
                        .Reverse()
                        .Select((type, i) => $"new {type}(sortKeys{topLevel}_{i}")
                        .ToArray();
                    var comparerExpression = string.Join(", ", parts) + new string(')', comparerTypes.Count);
                    var resultExpression = $"SortHelper.Sort(result{topLevel}, map{topLevel}, comparer{lastLevel}, sortKeys{topLevel}_0.Length)";
                    if(toInstanceType == ToValueType.ToHashSet)
                        resultExpression = $"new HashSet<{sourceGenericArg}>({resultExpression})";
                    if(toInstanceType == ToValueType.ToDictionary)
                        resultExpression = $"DictionaryHelper.ArrayToDictionary({resultExpression}, keySelector)";
                    bool useSourceInSort = piece.SameType && source.HasIndexer();
                    return (
@$"var result{topLevel} = {(useSourceInSort ? sourcePath : $"new {sourceGenericArg}[{capacityExpression}]")};
{string.Join(null, sortKeyVars)}
var map{topLevel} = ArrayPool<int>.Shared.Rent({capacityExpression});",

string.Join(null, sortKeyAssigns) + $"map{topLevel}[i{topLevel}] = i{topLevel};{(useSourceInSort ? null : $"result{topLevel}[i{topLevel}] = item{piece.GetOrderByLevel()};")}",

$@"ArrayPool<int>.Shared.Return(map{topLevel});
var comparer{lastLevel} = {comparerExpression};
var result_{lastLevel} = {resultExpression};"
                    );

                default:
                    throw new NotImplementedException();
            };
        }

        static void EmitToList(SourceType source, CodeBuilder builder, EmitContext context) {
            var outputType = context.GetOutputType();
            builder.AppendLine($@"public List<{outputType}> ToList() => Utils.AsList(ToArray());");
        }
        static void EmitLoopBody(Level level, CodeBuilder builder, PieceOfWork piece, Action<CodeBuilder> finish, Level totalLevels) {
            if(level.Minus(piece.LastLevel) > 0) {
                finish(builder);
                return;
            }
            void EmitNext(CodeBuilder nextBuilder) => EmitLoopBody(level.Next, nextBuilder, piece, finish, totalLevels);
            if(!piece.Contexts.Any()) {
                builder.AppendLine($@"var item{level.Next} = item{level};");
                EmitNext(builder.Tab);
                return;
            }
            var intermediate = piece.Contexts[level.Minus(piece.TopLevel)].Node;
            var sourcePath = CodeGenerationTraits.GetSourcePath(totalLevels.Minus(level));
            switch(intermediate) {
                case WhereNode:
                    builder.AppendMultipleLines($@"
var item{level.Next} = item{level};
if(!this{sourcePath}.predicate(item{level.Next}))
    continue;");
                    EmitNext(builder);
                    break;
                case TakeWhileNode:
                    builder.AppendMultipleLines($@"
var item{level.Next} = item{level};
if(!this{sourcePath}.predicate(item{level.Next}))
    goto takeWhile{level.Next};");
                    EmitNext(builder);
                    break;
                case SkipWhileNode:
                    builder.AppendMultipleLines($@"
var item{level.Next} = item{level};
if(skipWhile{level.Next}) {{
    if(this{sourcePath}.predicate(item{level.Next})) {{
        continue;
    }} else {{
        skipWhile{level.Next} = false;
    }}
}}");
                    EmitNext(builder);
                    break;
                case SelectNode:
                    builder.AppendLine($@"var item{level.Next} = this{sourcePath}.selector(item{level});");
                    EmitNext(builder);
                    break;
                case SelectManyNode selectMany:
                    EmitLoop(selectMany.SourceType, builder, level.Next, $"this{sourcePath}.selector(item{level})",
                        bodyBuilder => EmitNext(bodyBuilder));
                    break;
                case OrderByNode or OrderByDescendingNode:
                    builder.AppendLine($"var item{level.Next} = this{sourcePath}.keySelector(item{level});");
                    EmitNext(builder);
                    break;
                case ThenByNode or ThenByDescendingNode:
                    builder.AppendLine($"var item{level.Next} = this{sourcePath}.keySelector(item{piece.GetOrderByLevel()});");
                    EmitNext(builder);
                    break;
                default:
                    throw new NotImplementedException();

            }
        }
    }

    public record EmitContext(Level Level, IntermediateNode Node, string SourceType, string SourceGenericArg, EmitContext? Parent) {
        public const string RootSourceType = "TSource";

        public static EmitContext Root(SourceType source, IntermediateNode Node) 
            => new EmitContext(Level.Zero, Node, source.GetSourceTypeName(RootSourceType), RootSourceType, null);

        public EmitContext Next(IntermediateNode node)
            => new EmitContext(Level.Next, node, Node.GetEnumerableTypeName(Level) + this.GetOwnTypeArgsList(), this.GetOutputType(), this);
    }
    public record struct Level {
        public static Level MinusOne => new Level(-1);
        public static Level Zero => new Level(0);
        public int Value { get; }
        Level(int value) {
            Value = value;
        }
        public override string ToString() => (Value + 1).ToString();
        public Level Next => new Level(Value + 1);
        public Level Prev => new Level(Value - 1);
        public int Minus(Level level) => Value - level.Value;
        public Level Offset(int value) => new Level(Value + value);
    }

    public static class CodeGenerationTraits {
        public static Level GetOrderByLevel(this PieceOfWork piece) => piece.Contexts.First(x => x.Node is OrderByNode or OrderByDescendingNode).Level;
        public static string GetEnumerableTypeName(this IntermediateNode intermediate, Level level) {
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
                SourceType.CustomCollection => $"MetaLinq.Tests.CustomCollection<{sourceGenericArg}>",
                SourceType.CustomEnumerable => $"MetaLinq.Tests.CustomEnumerable<{sourceGenericArg}>",
                _ => throw new NotImplementedException(),
            };
        }
        public static string GetArgumentType(this IntermediateNode intermediate, string inType, string outType) {
            return intermediate switch {
                WhereNode or TakeWhileNode or SkipWhileNode => $"Func<{inType}, bool>",
                SelectNode or OrderByNode or OrderByDescendingNode or ThenByNode or ThenByDescendingNode => $"Func<{inType}, {outType}>",
                SelectManyNode selectMany => $"Func<{inType}, {selectMany.SourceType.GetSourceTypeName(outType)}>",
                _ => throw new NotImplementedException(),
            };
        }
        public static string GetResultGenericType(this EmitContext context) => $"T{context.Level}_{"Result"}";
        public const string RootStaticTypePrefix = "Meta_";
        public static string GetEnumerableSourceName(this SourceType source) {
            return RootStaticTypePrefix + source.GetEnumerableSourceNameShort();
        }
        public static string GetEnumerableSourceNameShort(this SourceType source) {
            return source switch {
                SourceType.List => "List",
                SourceType.Array => "Array",
                SourceType.CustomCollection => "CustomCollection",
                SourceType.CustomEnumerable => "CustomEnumerable",
                _ => throw new NotImplementedException(),
            };
        }
        public static string GetEnumerableKind(this IntermediateNode intermediate) {
            return intermediate switch {
                WhereNode => "Where",
                TakeWhileNode => "TakeWhile",
                SkipWhileNode => "SkipWhile",
                SelectNode => "Select",
                SelectManyNode => "SelectMany",
                OrderByNode => "OrderBy",
                OrderByDescendingNode => "OrderByDescending",
                ThenByNode => "ThenBy",
                ThenByDescendingNode => "ThenByDescending",
                _ => throw new NotImplementedException(),
            };
        }
        public static string? GetOwnTypeArgsList(this EmitContext context) {
            return context.Node.GetOwnTypeArgsList(context.GetResultGenericType());
        }
        public static string? GetOwnTypeArgsList(this IntermediateNode intermediate, string argName) {
            var ownTypeArg = intermediate.GetOwnTypeArg(argName);
            return ownTypeArg == null ? null : $"<{ownTypeArg}>";
        }

        public static string? GetOwnTypeArg(this IntermediateNode intermediate, string argName) {
            return intermediate switch {
                WhereNode or TakeWhileNode or SkipWhileNode => null,
                SelectNode or SelectManyNode or OrderByNode or OrderByDescendingNode or ThenByNode or ThenByDescendingNode => argName,
                _ => throw new NotImplementedException(),
            };
        }

        public static string GetOutputType(this EmitContext context) {
            return context.Node switch {
                WhereNode or TakeWhileNode or SkipWhileNode or OrderByNode or OrderByDescendingNode or ThenByNode or ThenByDescendingNode => context.SourceGenericArg,
                SelectNode or SelectManyNode => context.GetResultGenericType(),
                _ => throw new NotImplementedException(),
            };
        }
        public static string GetArgumentName(this IntermediateNode intermediate) {
            return intermediate switch {
                WhereNode or TakeWhileNode or SkipWhileNode => "predicate",
                SelectNode or SelectManyNode => "selector",
                OrderByNode or OrderByDescendingNode or ThenByNode or ThenByDescendingNode => "keySelector",
                _ => throw new NotImplementedException(),
            };
        }
        public static string GetCountName(this SourceType source) {
            return source switch {
                SourceType.List => "Count",
                SourceType.Array => "Length",
                SourceType.CustomCollection => "Count",
                _ => throw new NotImplementedException(),
            };
        }
        public static Level GetLabelIndex(this EmitContext context, int skip) {
            return context.GetContexts().Where(x => x.Node is SelectManyNode).Skip(skip).FirstOrDefault()?.Level.Next ?? Level.Zero;
        }
    }
}
