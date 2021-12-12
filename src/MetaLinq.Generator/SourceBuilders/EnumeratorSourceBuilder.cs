namespace MetaLinq.Generator;

public static class EnumeratorSourceBuilder {
    public static void EmitGetEnumerator(SourceType source, CodeBuilder builder, EmitContext context) {
        var intermediate = context.Element;
        var countName = source.GetCountName();
        var outputType = context.GetOutputType();
        var enumerableKind = intermediate.GetEnumerableKind();
        var ownTypeArgsList = context.GetOwnTypeArgsList();
        var contexts = context.GetReversedContexts().ToArray();
        var enumerableTypeName = $"{intermediate.GetEnumerableTypeName(context.Level)}{ownTypeArgsList}";
        builder.AppendLine("#nullable disable");
        var selectManyLevels = contexts
            .Where(x => x.Element is SelectManyNode)
            .Select(x => (index: x.Level.Next, node: (SelectManyNode)x.Element, outputType: x.GetOutputType()))
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
        var sourcePath = CodeGenerationTraits.GetSourcePath(totalLevels.Prev.Minus(level));
        switch(context.Element) {
            case WhereNode:
                builder.AppendMultipleLines($@"
var item{level.Next} = item{level};
if(!source{sourcePath}.predicate(item{level.Next}))
    goto next{context.GetLabelIndex(skip: 0)};");
                break;
            case OfTypeNode:
                builder.AppendMultipleLines($@"
if(item{level} is not {context.GetResultGenericType()})
    goto next{context.GetLabelIndex(skip: 0)};
var item{level.Next} = ({context.GetResultGenericType()})(object)item{level};");
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
}
