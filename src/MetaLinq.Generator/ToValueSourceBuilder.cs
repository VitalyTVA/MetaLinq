using System;
using System.Linq;

namespace MetaLinq.Generator;

public enum ToValueType {
    ToArray,
    ToHashSet,
    ToDictionary,
    First,
    FirstOrDefault,
}

public static class ToValueSourceBuilder {
    public static void EmitToValue(SourceType source, CodeBuilder builder, EmitContext context, ToValueType toInstanceType) {
        IntermediateNode intermediate = context.Node;
        var outputType = context.GetOutputType();

        var sourcePath = CodeGenerationTraits.GetSourcePath(context.Level.Next.Value);

        var methodDefinition = toInstanceType switch {
            ToValueType.ToArray => $"{outputType}[] ToArray()",
            ToValueType.ToHashSet => $"HashSet<{outputType}> ToHashSet()",
            ToValueType.ToDictionary => $"Dictionary<TKey, {outputType}> ToDictionary<TKey>(Func<{outputType}, TKey> keySelector) where TKey : notnull",
            ToValueType.First => $"{outputType} First(Func<{outputType}, bool> predicate)",
            ToValueType.FirstOrDefault => $"{outputType}? FirstOrDefault(Func<{outputType}, bool> predicate)",
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
        if(toInstanceType is ToValueType.First or ToValueType.FirstOrDefault && piece.ResultType is not ResultType.OrderBy)
            builder.Tab.AppendLine($"firstFound{lastLevel}:");

        builder.Tab.AppendMultipleLines(result);
    }

    static (string init, string add, string result) GetArrayBuilder(SourceType source, ToValueType? toInstanceType, string sourcePath, PieceOfWork piece) {
        var sourceGenericArg = piece.Contexts.LastOrDefault()?.SourceGenericArg ?? EmitContext.RootSourceType;
        var outputType = piece.Contexts.LastOrDefault()?.GetOutputType() ?? EmitContext.RootSourceType;
        var topLevel = piece.TopLevel;
        var lastLevel = piece.LastLevel;

        var capacityExpression = piece.SameSize ? $"{sourcePath}.{source.GetCountName()}" : null;

        switch((piece.SameSize, piece.ResultType, toInstanceType)) {
            case (_, ResultType.ToValue, ToValueType.First):
                return (
$@"var result{topLevel} = default({outputType});
bool found{topLevel} = false;",
$@"if(predicate(item{lastLevel.Next})) {{
    found{topLevel} = true;
    result{topLevel} = item{lastLevel.Next};
    goto firstFound{lastLevel};
}}",
$@"if(!found{topLevel})
    throw new InvalidOperationException(""Sequence contains no matching element"");
var result_{lastLevel} = result{topLevel}!;"
                );
            case (_, ResultType.ToValue, ToValueType.FirstOrDefault):
                return (
$@"var result{topLevel} = default({outputType});",
$@"if(predicate(item{lastLevel.Next})) {{
    result{topLevel} = item{lastLevel.Next};
    goto firstFound{lastLevel};
}}",
$@"var result_{lastLevel} = result{topLevel};"
                );

            case (false, ResultType.ToValue, ToValueType.ToArray):
                return (
                    $"using var result{topLevel} = new LargeArrayBuilder<{outputType}>();",
                    $"result{topLevel}.Add(item{lastLevel.Next});",
                    $@"var result_{lastLevel} = result{topLevel}.ToArray();"
                );
            case (true, ResultType.ToValue, ToValueType.ToArray):
                return (
                    $"var result{topLevel} = Allocator.Array<{outputType}>({capacityExpression});",
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
                    $"var result{topLevel} = Allocator.Dictionary<TKey, {outputType}>({capacityExpression});",
                    $"result{topLevel}.Add(keySelector(item{lastLevel.Next}), item{lastLevel.Next});",
                    $@"var result_{lastLevel} = result{topLevel};"
                );
            case (true, ResultType.OrderBy, _):
                var order = piece.Contexts
                    .SkipWhile(x => x.Node is not (OrderByNode or OrderByDescendingNode or ThenByNode or ThenByDescendingNode))
                    .Select(x => (x.Level, sortKeyGenericType: x.GetResultGenericType(), descending: x.Node is OrderByDescendingNode or ThenByDescendingNode))
                    .ToArray();
                var sortKeyVars = order.Select((x, i) => {
                    return $"var sortKeys{topLevel}_{i} = Allocator.Array<{x.sortKeyGenericType}>({capacityExpression});\r\n";
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
                if(toInstanceType == ToValueType.First)
                    resultExpression = $"System.Linq.Enumerable.First({resultExpression}, predicate)";
                if(toInstanceType == ToValueType.FirstOrDefault)
                    resultExpression = $"System.Linq.Enumerable.FirstOrDefault({resultExpression}, predicate)";
                bool useSourceInSort = piece.SameType && source.HasIndexer();
                return (
@$"var result{topLevel} = {(useSourceInSort ? sourcePath : $"Allocator.Array<{sourceGenericArg}>({capacityExpression})")};
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
