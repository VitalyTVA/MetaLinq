using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLinq.Generator;
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
        EmitExtensionMethod(source, builder, intermediate.Element);
        using (builder.BuildType(out CodeBuilder sourceTypeBuilder, 
            TypeModifiers.StaticClass, 
            source.GetEnumerableSourceName(), 
            partial: true, 
            generics: EmitContext.RootSourceType)
        ) {
            var context = EmitContext.Root(source, intermediate.Element);
            EmitStruct(source, sourceTypeBuilder, context, intermediate.GetNodes().ToList());
        }
    }
    static void EmitExtensionMethod(SourceType source, CodeBuilder builder, IntermediateChainElement intermediate) {
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
        var intermediate = context.Element;
        var argumentName = intermediate.GetArgumentName();
        var methodArgumentType = intermediate.GetArgumentType(context.SourceGenericArg, "TResult");

        var ownTypeArgsList = intermediate.GetOwnTypeArgsList("TResult");
        var enumerableKind = intermediate.GetEnumerableKind();
        var enumerableTypeName = $"{intermediate.GetEnumerableTypeName(context.Level)}{ownTypeArgsList}";

        builder.AppendLine($"public {enumerableTypeName} {enumerableKind}{ownTypeArgsList}({methodArgumentType} {argumentName}) => new {enumerableTypeName}(this, {argumentName});");
    }

    static void EmitStruct(SourceType source, CodeBuilder builder, EmitContext context, List<LinqNode> nodes) {
        var intermediate = context.Element;
        var argumentName = intermediate.GetArgumentName();
        var argumentType = intermediate.GetArgumentType(context.SourceGenericArg, context.GetResultGenericType());
        bool implementIEnumerable = nodes.Any(node => node is TerminalNode { Element: EnumerableChainElement });
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
                    case TerminalNode terminalNode:
                        switch(terminalNode.Element) {
                            case ToListChainElement:
                                EmitToList(source, structBuilder, context);
                                break;
                            case EnumerableChainElement:
                                EmitGetEnumerator(source, structBuilder, context);
                                break;
                            case ToValueChainElement toValueChainElement:
                                ToValueSourceBuilder.EmitToValue(source, structBuilder, context, toValueChainElement.Type);
                                break;
                        }
                        break;
                    case IntermediateNode nextIntermediate:
                        var nextContext = context.Next(nextIntermediate.Element);
                        EmitStructMethod(structBuilder, nextContext);
                        EmitStruct(source, structBuilder, nextContext, nextIntermediate.GetNodes().ToList());
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

        }
    }
    static void EmitGetEnumerator(SourceType source, CodeBuilder builder, EmitContext context) {
            var intermediate = context.Element;
            var countName = source.GetCountName();
            var outputType = context.GetOutputType();
            var enumerableKind = intermediate.GetEnumerableKind();
            var ownTypeArgsList = context.GetOwnTypeArgsList();
            var contexts = context.GetReversedContexts().ToArray();
            var enumerableTypeName = $"{intermediate.GetEnumerableTypeName(context.Level)}{ownTypeArgsList}";
            builder.AppendLine("#nullable disable");
            var selectManyLevels = contexts
                .Where(x => x.Element is SelectManyChainElement)
                .Select(x => (index: x.Level.Next, node: (SelectManyChainElement)x.Element, outputType: x.GetOutputType()))
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
                case WhereChainElement:
                    builder.AppendMultipleLines($@"
var item{level.Next} = item{level};
if(!source{sourcePath}.predicate(item{level.Next}))
    goto next{context.GetLabelIndex(skip: 0)};");
                    break;
                case SelectChainElement:
                    builder.AppendLine($@"var item{level.Next} = source{sourcePath}.selector(item{level});");
                    break;
                case SelectManyChainElement selectMany:
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

    static void EmitToList(SourceType source, CodeBuilder builder, EmitContext context) {
            var outputType = context.GetOutputType();
            builder.AppendLine($@"public List<{outputType}> ToList() => Utils.AsList(ToArray());");
        }
}
public record EmitContext(Level Level, IntermediateChainElement Element, string SourceType, string SourceGenericArg, EmitContext? Parent) {

    public const string RootSourceType = "TSource";

    public static EmitContext Root(SourceType source, IntermediateChainElement element) 
        => new EmitContext(Level.Zero, element, source.GetSourceTypeName(RootSourceType), RootSourceType, null);

    public EmitContext Next(IntermediateChainElement element)
        => new EmitContext(Level.Next, element, Element.GetEnumerableTypeName(Level) + this.GetOwnTypeArgsList(), this.GetOutputType(), this);
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
    //public Level Offset(int value) => new Level(Value + value);
}

public static class CodeGenerationTraits {
    public static Level GetOrderByLevel(this PieceOfWork piece) => piece.Contexts.First(x => x.Element is OrderByChainElement or OrderByDescendingChainElement).Level;
    public static string GetEnumerableTypeName(this IntermediateChainElement intermediate, Level level) {
            var enumerableKind = intermediate.GetEnumerableKind();
            var sourceTypePart = intermediate switch {
                SelectManyChainElement selectMany => "_" + selectMany.SourceType.GetEnumerableSourceNameShort(),
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
    public static string GetArgumentType(this IntermediateChainElement intermediate, string inType, string outType) {
            return intermediate switch {
                WhereChainElement or TakeWhileChainElement or SkipWhileChainElement => $"Func<{inType}, bool>",
                SelectChainElement or OrderByChainElement or OrderByDescendingChainElement or ThenByChainElement or ThenByDescendingChainElement => $"Func<{inType}, {outType}>",
                SelectManyChainElement selectMany => $"Func<{inType}, {selectMany.SourceType.GetSourceTypeName(outType)}>",
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
    public static string GetEnumerableKind(this IntermediateChainElement intermediate) {
            return intermediate switch {
                WhereChainElement => "Where",
                TakeWhileChainElement => "TakeWhile",
                SkipWhileChainElement => "SkipWhile",
                SelectChainElement => "Select",
                SelectManyChainElement => "SelectMany",
                OrderByChainElement => "OrderBy",
                OrderByDescendingChainElement => "OrderByDescending",
                ThenByChainElement => "ThenBy",
                ThenByDescendingChainElement => "ThenByDescending",
                _ => throw new NotImplementedException(),
            };
        }
    public static string? GetOwnTypeArgsList(this EmitContext context) {
            return context.Element.GetOwnTypeArgsList(context.GetResultGenericType());
        }
    public static string? GetOwnTypeArgsList(this IntermediateChainElement intermediate, string argName) {
            var ownTypeArg = intermediate.GetOwnTypeArg(argName);
            return ownTypeArg == null ? null : $"<{ownTypeArg}>";
        }

    public static string? GetOwnTypeArg(this IntermediateChainElement intermediate, string argName) {
            return intermediate switch {
                WhereChainElement or TakeWhileChainElement or SkipWhileChainElement => null,
                SelectChainElement or SelectManyChainElement or OrderByChainElement or OrderByDescendingChainElement or ThenByChainElement or ThenByDescendingChainElement => argName,
                _ => throw new NotImplementedException(),
            };
        }

    public static string GetOutputType(this EmitContext context) {
            return context.Element switch {
                WhereChainElement or TakeWhileChainElement or SkipWhileChainElement or OrderByChainElement or OrderByDescendingChainElement or ThenByChainElement or ThenByDescendingChainElement => context.SourceGenericArg,
                SelectChainElement or SelectManyChainElement => context.GetResultGenericType(),
                _ => throw new NotImplementedException(),
            };
        }
    public static string GetArgumentName(this IntermediateChainElement intermediate) {
            return intermediate switch {
                WhereChainElement or TakeWhileChainElement or SkipWhileChainElement => "predicate",
                SelectChainElement or SelectManyChainElement => "selector",
                OrderByChainElement or OrderByDescendingChainElement or ThenByChainElement or ThenByDescendingChainElement => "keySelector",
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
            return context.GetContexts().Where(x => x.Element is SelectManyChainElement).Skip(skip).FirstOrDefault()?.Level.Next ?? Level.Zero;
        }
}
