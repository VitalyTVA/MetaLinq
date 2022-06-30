using System;
using System.ComponentModel;
using System.Linq;

namespace MetaLinq.Generator;

public static class ToValueSourceBuilder {
    public static void EmitToValue(SourceType source, CodeBuilder builder, EmitContext context, ToValueType toValueType) {
        var outputType = context.GetOutputType();

        var sourcePath = CodeGenerationTraits.GetSourcePath(context.Level.Next.Value);

        string? GetAggregateMethodDefinition(ToValueType toValueType) {
            var info = toValueType.GetAggregateInfo();
            if(info is not null) {
                if(!info.Value.HasSelector)
                    return $"{outputType} {info.Value.Kind}()";
                return $"{info.Value.GetAggregateOutputType()} {info.Value.Kind}(Func<{outputType}, {info.Value.GetAggregateInputType()}> selector)";
            }
            return null;
        };

        var methodDefinition = GetAggregateMethodDefinition(toValueType) ?? toValueType switch {
            ToValueType.ToArray => $"{outputType}[] ToArray()",
            ToValueType.ToHashSet => $"HashSet<{outputType}> ToHashSet()",
            ToValueType.ToDictionary => $"Dictionary<TKey, {outputType}> ToDictionary<TKey>(Func<{outputType}, TKey> keySelector) where TKey : notnull",
            ToValueType.First or ToValueType.FirstOrDefault or ToValueType.Last or ToValueType.LastOrDefault or ToValueType.Single or ToValueType.SingleOrDefault => $"{outputType} {toValueType.ToMethodName()}()",
            ToValueType.First_Predicate or ToValueType.Last_Predicate or ToValueType.Single_Predicate => $"{outputType} {toValueType.ToMethodName()}(Func<{outputType}, bool> predicate)",
            ToValueType.FirstOrDefault_Predicate or ToValueType.LastOrDefault_Predicate or ToValueType.SingleOrDefault_Predicate => $"{outputType}? {toValueType.ToMethodName()}(Func<{outputType}, bool> predicate)",
            ToValueType.Any_Predicate or ToValueType.All_Predicate => $"bool {toValueType.ToMethodName()}(Func<{outputType}, bool> predicate)",
            ToValueType.Any => $"bool {toValueType.ToMethodName()}()",
            ToValueType.Aggregate => $"{outputType} Aggregate(Func<{outputType}, {outputType}, {outputType}> func)",
            ToValueType.Aggregate_Seed => $"TAccumulate Aggregate<TAccumulate>(TAccumulate seed, Func<TAccumulate, {outputType}, TAccumulate> func)",
            ToValueType.Aggregate_Seed_Result => $"TResult Aggregate<TAccumulate, TResult>(TAccumulate seed, Func<TAccumulate, {outputType}, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)",
            _ => throw new NotImplementedException(),
        };
        builder.AppendLine($"public {methodDefinition} {{");

        var pieces = context.GetPieces(source, toValueType);
        foreach(var piece in pieces) {
            bool first = pieces.First() == piece;
            bool last = pieces.Last() == piece;
            EmitPieceOrWork(
                first ? source : SourceType.Array,
                last ? toValueType : ToValueType.ToArray,
                builder,
                first ? "this" + sourcePath : "result_" + piece.TopLevel.Prev,
                piece,
                context.Level);
        }
        builder.Tab.AppendLine($"return result_{context.Level};");
        builder.AppendLine("}");
    }

    static string GetAggregateInputType(this AggregateInfo info)
        => GetAggregateTypeCore(info) + GetAggregateNullableAnnotation(info);
    static string GetAggregateOutputType(this AggregateInfo info)
        => (info.Kind is AggregateKind.Average && info.Type is AggregateValueType.Int or AggregateValueType.Long
            ? "double"
            : GetAggregateTypeCore(info))
            + GetAggregateNullableAnnotation(info);
    static string GetAggregateTypeCore(AggregateInfo info) => info.Type.ToString().ToLower();
    static string? GetAggregateNullableAnnotation(AggregateInfo info) => (info.Nullable ? "?" : null);

    static void EmitPieceOrWork(SourceType source, ToValueType toInstanceType, CodeBuilder builder, string sourcePath, PieceOfWork piece, Level totalLevels) {

        var topLevel = piece.TopLevel;
        var lastLevel = piece.LastLevel;

        var (arrayBuilder, addValue, result) = GetArrayBuilder(source, toInstanceType, sourcePath, piece);
        builder.Tab.AppendMultipleLines(arrayBuilder);

        foreach(var item in piece.Contexts) {
            if(item.Element is SkipWhileNode)
                builder.Tab.AppendLine($"var skipWhile{item.Level.Next} = true;");
        }

        if(toInstanceType is ToValueType.Any)
            builder.Tab.AppendLine("#pragma warning disable 0162");
        EmitLoop(source, piece.LoopType.IsForwardLoop(), builder.Tab, topLevel, sourcePath,
            bodyBuilder => EmitLoopBody(topLevel, bodyBuilder, piece, b => b.AppendMultipleLines(addValue), totalLevels));
        if(toInstanceType is ToValueType.Any)
            builder.Tab.AppendLine("#pragma warning restore 0162");

        foreach(var item in piece.Contexts) {
            if(item.Element is TakeWhileNode)
                builder.Tab.AppendLine($"takeWhile{item.Level.Next}:");
        }
        if((toInstanceType is ToValueType.First or ToValueType.First_Predicate or ToValueType.FirstOrDefault or ToValueType.FirstOrDefault_Predicate && piece.LoopType is LoopType.Forward)
            || (toInstanceType is ToValueType.Last or ToValueType.Last_Predicate or ToValueType.LastOrDefault or ToValueType.LastOrDefault_Predicate && piece.LoopType is LoopType.Backward)
            || (toInstanceType is ToValueType.Any or ToValueType.Any_Predicate or ToValueType.All_Predicate && piece.LoopType is LoopType.Forward))
            builder.Tab.AppendLine($"firstFound{lastLevel}:");

        builder.Tab.AppendMultipleLines(result);
    }

    static (string init, string add, string result) GetArrayBuilder(SourceType source, ToValueType toValueType, string sourcePath, PieceOfWork piece) {
        var sourceGenericArg = piece.Contexts.LastOrDefault()?.SourceGenericArg ?? EmitContext.RootSourceType;
        var outputType = piece.Contexts.LastOrDefault()?.GetOutputType() ?? EmitContext.RootSourceType;
        var topLevel = piece.TopLevel;
        var lastLevel = piece.LastLevel;

        var capacityExpression = piece.KnownSize ? $"{sourcePath}.{source.GetCountName()}" : null;

        (Level Level, string sortKeyGenericType, ListSortDirection direction)[] GetOrder() => piece.Contexts
            .SkipWhile(x => x.Element is not (OrderByNode or ThenByNode))
            .Select(x => (
                x.Level,
                sortKeyGenericType: x.GetResultGenericType(),
                direction: (x.Element as OrderByNode)?.Direction ?? (x.Element as ThenByNode)!.Direction
            ))
            .ToArray();
        string GetFirstLastSingleResultStatement() =>
$@"if(!found{topLevel})
    throw new InvalidOperationException(""Sequence contains no matching element"");
var result_{lastLevel} = result{topLevel}!;";
        string GetFirstLastSingleOrDefaultResultStatement() =>
$@"var result_{lastLevel} = result{topLevel};";

        #region aggregates
        var aggregateInfo = toValueType.GetAggregateInfo();
        if(aggregateInfo != null) {
            if(piece.LoopType != LoopType.Forward)
                throw new InvalidOperationException();
            if(!aggregateInfo.Value.HasSelector && aggregateInfo.Value.Kind == AggregateKind.Sum) {
                return (
$@"{outputType} result{topLevel} = AggregateHelper.Zero<{outputType}>();",
$@"var value{lastLevel.Next} = item{lastLevel.Next};
{(aggregateInfo.Value.Nullable ? $"if(value{lastLevel.Next} != null) " : null)} {{ 
    result{topLevel} = AggregateHelper.Sum(result{topLevel}, value{lastLevel.Next});
}}",
$@"var result_{lastLevel} = result{topLevel};"
                );
            }
            switch(aggregateInfo.Value.Kind) {
                case AggregateKind.Sum:
                    return (
$@"{aggregateInfo.Value.GetAggregateOutputType()} result{topLevel} = 0;",
$@"var value{lastLevel.Next} = selector(item{lastLevel.Next});
{(aggregateInfo.Value.Nullable ? $"if(value{lastLevel.Next} != null) " : null)} {{ 
    result{topLevel} += value{lastLevel.Next};
}}",
$@"var result_{lastLevel} = result{topLevel};"
                    );
                case AggregateKind.Average:
                    return (
$@"{aggregateInfo.Value.GetAggregateOutputType()} result{topLevel} = 0;
var count{lastLevel.Next} = 0;",
$@"var value{lastLevel.Next} = selector(item{lastLevel.Next});
{(aggregateInfo.Value.Nullable ? $"if(value{lastLevel.Next} != null) " : null)} {{ 
    result{topLevel} += value{lastLevel.Next};
    count{lastLevel.Next}++;
}}",
$@"{(!aggregateInfo.Value.Nullable ? $"if(count{lastLevel.Next} == 0) throw new InvalidOperationException(\"Sequence contains no elements\");" : null)}
var result_{lastLevel} = {(aggregateInfo.Value.Nullable ? $"count{lastLevel.Next} == 0 ? null : " : null)} result{topLevel} / count{lastLevel.Next};"
                    );
                case AggregateKind.Min or AggregateKind.Max:
                    return (
$@"{aggregateInfo.Value.GetAggregateOutputType()} result{topLevel} = 0;
var found{lastLevel.Next} = false;",
$@"var value{lastLevel.Next} = selector(item{lastLevel.Next});
{(aggregateInfo.Value.Nullable ? $"if(value{lastLevel.Next} != null) " : null)} {{
    if(found{lastLevel.Next} && value{lastLevel.Next} {(aggregateInfo.Value.Kind == AggregateKind.Min ? '<' : '>')} result{topLevel}) {{
        result{topLevel} = value{lastLevel.Next};
    }} else if(!found{lastLevel.Next}) {{
        result{topLevel} = value{lastLevel.Next};
        found{lastLevel.Next} = true;
    }}
}}",
$@"{(!aggregateInfo.Value.Nullable ? $"if(!found{lastLevel.Next}) throw new InvalidOperationException(\"Sequence contains no elements\");" : null)}
var result_{lastLevel} = {(aggregateInfo.Value.Nullable ? $"!found{lastLevel.Next} ? null : " : null)} result{topLevel};"
                    );
                default:
                    throw new NotImplementedException();
            }
        }
        #endregion

        switch((piece.KnownSize, piece.LoopType, toValueType)) {
            case (_, LoopType.Forward, ToValueType.First_Predicate) or (_, LoopType.Backward, ToValueType.Last_Predicate):
                return (
$@"var result{topLevel} = default({outputType});
bool found{topLevel} = false;",
$@"if(predicate(item{lastLevel.Next})) {{
    found{topLevel} = true;
    result{topLevel} = item{lastLevel.Next};
    goto firstFound{lastLevel};
}}",
GetFirstLastSingleResultStatement()
                );
            case (_, LoopType.Forward, ToValueType.First) or (_, LoopType.Backward, ToValueType.Last):
                return (
$@"var result{topLevel} = default({outputType});
bool found{topLevel} = false;",
$@"found{topLevel} = true;
result{topLevel} = item{lastLevel.Next};
goto firstFound{lastLevel};",
$@"if(!found{topLevel})
    throw new InvalidOperationException(""Sequence contains no elements"");
var result_{lastLevel} = result{topLevel}!;"
                );
            case (_, LoopType.Forward, ToValueType.FirstOrDefault_Predicate) or (_, LoopType.Backward, ToValueType.LastOrDefault_Predicate):
                return (
$@"var result{topLevel} = default({outputType});",
$@"if(predicate(item{lastLevel.Next})) {{
    result{topLevel} = item{lastLevel.Next};
    goto firstFound{lastLevel};
}}",
GetFirstLastSingleOrDefaultResultStatement()
                );
            case (_, LoopType.Forward, ToValueType.FirstOrDefault) or (_, LoopType.Backward, ToValueType.LastOrDefault):
                return (
$@"var result{topLevel} = default({outputType})!;",
$@"result{topLevel} = item{lastLevel.Next};
goto firstFound{lastLevel};",
GetFirstLastSingleOrDefaultResultStatement()
                );
            case (_, LoopType.Forward, ToValueType.Any_Predicate or ToValueType.All_Predicate):
                string invert = toValueType switch {
                    ToValueType.Any_Predicate => string.Empty,
                    ToValueType.All_Predicate => "!",
                    _ => throw new InvalidOperationException()
                };
                return (
$@"bool found{topLevel} = false;",
$@"if({invert}predicate(item{lastLevel.Next})) {{
    found{topLevel} = true;
    goto firstFound{lastLevel};
}}",
$@"var result_{lastLevel} = {invert}found{topLevel};"
                );
            case (_, LoopType.Forward, ToValueType.Any):
                return (
$@"bool found{topLevel} = false;",
$@"found{topLevel} = true;
goto firstFound{lastLevel};",
$@"var result_{lastLevel} = found{topLevel};"
                );
            case (_, LoopType.Forward, ToValueType.Single_Predicate):
                return (
$@"var result{topLevel} = default({outputType});
bool found{topLevel} = false;",
$@"if(predicate(item{lastLevel.Next})) {{
    if(!found{topLevel}) {{
        found{topLevel} = true;
        result{topLevel} = item{lastLevel.Next};
    }} else {{
        throw new InvalidOperationException(""Sequence contains more than one matching element"");
    }}
}}",
GetFirstLastSingleResultStatement()
                );
            case (_, LoopType.Forward, ToValueType.Single):
                return (
$@"var result{topLevel} = default({outputType});
bool found{topLevel} = false;",
$@"if(!found{topLevel}) {{
    found{topLevel} = true;
    result{topLevel} = item{lastLevel.Next};
}} else {{
    throw new InvalidOperationException(""Sequence contains more than one element"");
}}",
GetFirstLastSingleResultStatement()
                );
            case (_, LoopType.Forward, ToValueType.SingleOrDefault_Predicate):
                return (
$@"var result{topLevel} = default({outputType});
bool found{topLevel} = false;",
$@"if(predicate(item{lastLevel.Next})) {{
    if(!found{topLevel}) {{
        found{topLevel} = true;
        result{topLevel} = item{lastLevel.Next};
    }} else {{
        throw new InvalidOperationException(""Sequence contains more than one matching element"");
    }}
}}",
GetFirstLastSingleOrDefaultResultStatement()
                );
            case (_, LoopType.Forward, ToValueType.SingleOrDefault):
                return (
$@"var result{topLevel} = default({outputType})!;
bool found{topLevel} = false;",
$@"if(!found{topLevel}) {{
    found{topLevel} = true;
    result{topLevel} = item{lastLevel.Next};
}} else {{
    throw new InvalidOperationException(""Sequence contains more than one element"");
}}",
GetFirstLastSingleOrDefaultResultStatement()
                );
            case (_, LoopType.Forward, ToValueType.Last_Predicate):
                return (
$@"var result{topLevel} = default({outputType});
bool found{topLevel} = false;",
$@"if(predicate(item{lastLevel.Next})) {{
    found{topLevel} = true;
    result{topLevel} = item{lastLevel.Next};
}}",
GetFirstLastSingleResultStatement()
                );
            case (_, LoopType.Forward, ToValueType.LastOrDefault_Predicate):
                return (
$@"var result{topLevel} = default({outputType});",
$@"if(predicate(item{lastLevel.Next})) {{
    result{topLevel} = item{lastLevel.Next};
}}",
GetFirstLastSingleOrDefaultResultStatement()
                );

            case (false, LoopType.Forward, ToValueType.ToArray):
                return (
                    $"using var result{topLevel} = new LargeArrayBuilder<{outputType}>();",
                    $"result{topLevel}.Add(item{lastLevel.Next});",
                    $@"var result_{lastLevel} = result{topLevel}.ToArray();"
                );
            case (true, LoopType.Forward, ToValueType.ToArray):
                return (
                    $"var result{topLevel} = Allocator.Array<{outputType}>({capacityExpression});",
                    $"result{topLevel}[i{topLevel}] = item{lastLevel.Next};",
                    $@"var result_{lastLevel} = result{topLevel};"
                );
            case (_, LoopType.Forward, ToValueType.ToHashSet):
                return (
                    $"var result{topLevel} = Allocator.HashSet<{outputType}>({capacityExpression});",
                    $"result{topLevel}.Add(item{lastLevel.Next});",
                    $@"var result_{lastLevel} = result{topLevel};"
                );
            case (_, LoopType.Forward, ToValueType.ToDictionary):
                return (
                    $"var result{topLevel} = Allocator.Dictionary<TKey, {outputType}>({capacityExpression});",
                    $"result{topLevel}.Add(keySelector(item{lastLevel.Next}), item{lastLevel.Next});",
                    $@"var result_{lastLevel} = result{topLevel};"
                );
            case (true, LoopType.Sort, ToValueType.ToArray or ToValueType.Aggregate or ToValueType.Aggregate_Seed or ToValueType.Aggregate_Seed_Result):
                var order = GetOrder();
                var sortKeyVars = order.Select((x, i) => {
                    return $"var sortKeys{topLevel}_{i} = Allocator.Array<{x.sortKeyGenericType}>({capacityExpression});\r\n";
                });
                var sortKeyAssigns = order.Select((x, i) => {
                    return $"sortKeys{topLevel}_{i}[i{topLevel}] = item{x.Level.Next};\r\n";
                });
                List<string> comparerTypes = new();
                foreach(var (_, sortKeyGenericType, direction) in order.Reverse()) {
                    var last = comparerTypes.LastOrDefault();
                    if(last != null)
                        comparerTypes.Add($"KeysComparer{direction.GetDescendingSuffix()}<{sortKeyGenericType}, {last}>");
                    else
                        comparerTypes.Add($"KeysComparer{direction.GetDescendingSuffix()}<{sortKeyGenericType}>");
                }
                var parts = comparerTypes
                    .ToArray()
                    .Reverse()
                    .Select((type, i) => $"new {type}(sortKeys{topLevel}_{i}")
                    .ToArray();
                var comparerExpression = string.Join(", ", parts) + new string(')', comparerTypes.Count);
                var resultExpression = toValueType switch { 
                    ToValueType.ToArray => $"SortHelper.Sort(result{topLevel}, map{topLevel}, comparer{lastLevel}, sortKeys{topLevel}_0.Length)",
                    ToValueType.Aggregate => $"SortHelper.Aggregate(result{topLevel}, map{topLevel}, comparer{lastLevel}, sortKeys{topLevel}_0.Length, func)",
                    ToValueType.Aggregate_Seed or ToValueType.Aggregate_Seed_Result => $"SortHelper.Aggregate(result{topLevel}, map{topLevel}, comparer{lastLevel}, sortKeys{topLevel}_0.Length, seed, func)",
                    _ => throw new InvalidOperationException()
                };
                if(toValueType is ToValueType.Aggregate_Seed_Result)
                    resultExpression = $"resultSelector({resultExpression})";
                bool useSourceInSort = piece.KnownType && source.HasIndexer();
                return (
@$"var result{topLevel} = {(useSourceInSort ? sourcePath : $"Allocator.Array<{sourceGenericArg}>({capacityExpression})")};
{string.Join(null, sortKeyVars)}
var map{topLevel} = ArrayPool<int>.Shared.Rent({capacityExpression});",

string.Join(null, sortKeyAssigns) + $"map{topLevel}[i{topLevel}] = i{topLevel};{(useSourceInSort ? null : $"result{topLevel}[i{topLevel}] = item{piece.GetOrderByLevel()};")}",

$@"ArrayPool<int>.Shared.Return(map{topLevel});
var comparer{lastLevel} = {comparerExpression};
var result_{lastLevel} = {resultExpression};"
                );

            case (_, LoopType.Sort, 
                ToValueType.First or ToValueType.First_Predicate or
                ToValueType.FirstOrDefault or ToValueType.FirstOrDefault_Predicate or
                ToValueType.Last or ToValueType.Last_Predicate or
                ToValueType.LastOrDefault or ToValueType.LastOrDefault_Predicate
            ):
                var order_ = GetOrder();
                var itemLevel = order_.First().Level;
                var keyDefinitions = order_
                    .Select(x => $"var foundKey{x.Level} = default({x.sortKeyGenericType});");
                var keyAssignments = order_
                    .Select((x, i) => $"        foundKey{x.Level} = item{x.Level.Next};");
                var compares = order_
                    .Select((x, i) => {
                        var args = x.direction == ListSortDirection.Descending
                            ? $"foundKey{x.Level}, item{x.Level.Next}"
                            : $"item{x.Level.Next}, foundKey{x.Level}";
                        var result = $"compareResult = SortHelper.CompareValues({args});";
                        if(i > 0)
                            result = "if(compareResult == 0) " + result;
                        return "    " + result;
                    });
                char sign = toValueType switch {
                    ToValueType.First or ToValueType.First_Predicate or ToValueType.FirstOrDefault or ToValueType.FirstOrDefault_Predicate => '<',
                    ToValueType.Last or ToValueType.Last_Predicate or ToValueType.LastOrDefault or ToValueType.LastOrDefault_Predicate => '>',
                    _ => throw new InvalidOperationException()
                };
                var getResult =
@$"if(!found{topLevel}) {{
{string.Join(Environment.NewLine, keyAssignments)}
    result{topLevel} = item{itemLevel};
}}
int compareResult = 0;
{string.Join(Environment.NewLine, compares)}
if(compareResult {sign} 0) {{
{string.Join(Environment.NewLine, keyAssignments)}
    result{topLevel} = item{itemLevel};
}}
found{topLevel} = true;";
                if(toValueType is not (ToValueType.First or ToValueType.FirstOrDefault or ToValueType.Last or ToValueType.LastOrDefault)) {
                    getResult =
@$"if(predicate(item{itemLevel})) {{
{getResult}
}}";
                }
                return (
@$"var result{topLevel} = default({outputType})!;
bool found{topLevel} = false;
{string.Join(Environment.NewLine, keyDefinitions)}",
                    getResult,
                    toValueType is ToValueType.First or ToValueType.First_Predicate or ToValueType.Last or ToValueType.Last_Predicate
                        ? GetFirstLastSingleResultStatement()
                        : GetFirstLastSingleOrDefaultResultStatement()
                );
            case (_, LoopType.Forward, ToValueType.Aggregate_Seed or ToValueType.Aggregate_Seed_Result):
                return (
                    $"var result{topLevel} = seed;",
                    $"result{topLevel} = func(result{topLevel}, item{lastLevel.Next});",
                    toValueType switch {
                        ToValueType.Aggregate_Seed => $@"var result_{lastLevel} = result{topLevel};",
                        ToValueType.Aggregate_Seed_Result => $@"var result_{lastLevel} = resultSelector(result{topLevel});",
                        _ => throw new InvalidOperationException()
                    }
                );
            case (_, LoopType.Forward, ToValueType.Aggregate):
                return (
$@"{outputType} result{topLevel} = default({outputType})!;
bool found{topLevel} = false;",
$@"if(found{topLevel}) {{ 
    result{topLevel} = func(result{topLevel}, item{lastLevel.Next});
}} else {{
    result{topLevel} = item{lastLevel.Next};
    found{topLevel} = true;
}}",
$@"if(!found{topLevel})
    throw new InvalidOperationException(""Sequence contains no elements"");
var result_{lastLevel} = result{topLevel};"
                );
            default:
                throw new NotImplementedException();
        };
    }

    static void EmitLoop(SourceType source, bool forward, CodeBuilder builder, Level level, string sourceExpression, Action<CodeBuilder> emitBody) {
        builder.AppendLine($"var source{level} = {sourceExpression};");
        if(source.HasIndexer()) {
            if(forward)
                builder.AppendMultipleLines($@"
var len{level} = source{level}.{source.GetCountName()};
for(int i{level} = 0; i{level} < len{level}; i{level}++) {{
    var item{level} = source{level}[i{level}];");
            else
                builder.AppendMultipleLines($@"
var len{level} = source{level}.{source.GetCountName()};
for(int i{level} = len{level} - 1; i{level} >= 0; i{level}--) {{
    var item{level} = source{level}[i{level}];");
        }
        if(!source.HasIndexer()) {
            if(!forward)
                throw new InvalidOperationException("Forward loop expected.");
            builder.AppendMultipleLines($@"
int i{level} = 0;
foreach(var item{level} in source{level}) {{");
        }
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
        var context = piece.Contexts[level.Minus(piece.TopLevel)];
        var intermediate = context.Element;
        var sourcePath = CodeGenerationTraits.GetSourcePath(totalLevels.Minus(level));
        switch(intermediate) {
            case WhereNode:
                builder.AppendMultipleLines($@"
var item{level.Next} = item{level};
if(!this{sourcePath}.predicate(item{level.Next}))
    continue;");
                EmitNext(builder);
                break;
            case OfTypeNode:
                builder.AppendMultipleLines($@"
if(item{level} is not {context.GetResultGenericType()})
    continue;
var item{level.Next} = ({context.GetResultGenericType()})(object)item{level};");
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
            case CastNode:
                builder.AppendLine($@"var item{level.Next} = ({context.GetResultGenericType()})(object)item{level}!;");
                EmitNext(builder);
                break;
            case IdentityNode:
                builder.AppendLine($@"var item{level.Next} = item{level};");
                EmitNext(builder);
                break;
            case SelectManyNode selectMany:
                EmitLoop(selectMany.SourceType, piece.LoopType.IsForwardLoop(), builder, level.Next, $"this{sourcePath}.selector(item{level})", //TODO forward should not always be true here
                    bodyBuilder => EmitNext(bodyBuilder));
                break;
            case OrderByNode:
                builder.AppendLine($"var item{level.Next} = this{sourcePath}.keySelector(item{level});");
                EmitNext(builder);
                break;
            case ThenByNode:
                builder.AppendLine($"var item{level.Next} = this{sourcePath}.keySelector(item{piece.GetOrderByLevel()});");
                EmitNext(builder);
                break;
            default:
                throw new NotImplementedException();

        }
    }
}
