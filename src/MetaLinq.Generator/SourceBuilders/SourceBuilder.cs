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

    private static void BuildSource(SourceType source, LinqTree tree, CodeBuilder builder) {
        builder.AppendMultipleLines(@"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Buffers;
using MetaLinq.Internal;");
        using(builder.BuildNamespace(out CodeBuilder nsBuilder, "MetaLinq")) {
            foreach(var node in tree.GetNodes()) {
                switch(node) {
                    case LinqTreeIntermediateNode intermediate:
                        EmitIntermediate(source, nsBuilder, intermediate);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }
    }
    static void EmitIntermediate(SourceType source, CodeBuilder builder, LinqTreeIntermediateNode intermediate) {
        EmitExtensionMethod(source, builder, intermediate.Element);
        using (builder.BuildType(out CodeBuilder sourceTypeBuilder, 
            TypeModifiers.StaticClass, 
            source.GetEnumerableSourceName(), 
            partial: true, 
            generics: source.HasGenericArg() ? EmitContext.RootSourceType : null)
        ) {
            var context = EmitContext.Root(source, intermediate.Element);
            EmitStruct(source, sourceTypeBuilder, context, intermediate.GetNodes().ToList());
        }
    }
    static void EmitExtensionMethod(SourceType source, CodeBuilder builder, IntermediateNode intermediate) {
        var sourceName = source.GetEnumerableSourceName();
        var enumerableKind = intermediate.GetEnumerableKind();
        var ownTypeArgsList = intermediate.GetOwnTypeArgsList("TResult");
        var enumerableTypeName = $"{intermediate.GetEnumerableTypeName(Level.Zero)}{ownTypeArgsList}";

        var argumentInfo = intermediate.GetArgumentInfo(EmitContext.RootSourceType, "TResult");

        var sourceGenericArg = source.HasGenericArg() ? EmitContext.RootSourceType : null;
        var methodEnumerableSourceType = source.GetSourceTypeName(EmitContext.RootSourceType);

        var sourceGenericArgs = sourceGenericArg.YieldToArray().GetTypeArgsList();
        var methodGenericArgs = new[] {
            sourceGenericArg,
            intermediate.GetOwnTypeArg("TResult")
        }.GetTypeArgsList();

        using(builder.BuildType(out CodeBuilder classBuilder, TypeModifiers.StaticClass, "MetaEnumerable", partial: true)) {
            classBuilder.AppendMultipleLines($@"
public static {sourceName}{sourceGenericArgs}.{enumerableTypeName} {enumerableKind}{methodGenericArgs}(this {methodEnumerableSourceType} source{argumentInfo.ToArgumentList()})
    => new {sourceName}{sourceGenericArgs}.{enumerableTypeName}(source{argumentInfo.ToParameterList()});");
        }
    }

    static void EmitStructMethod(CodeBuilder builder, EmitContext context) {
        var intermediate = context.Element;
        var argumentInfo = intermediate.GetArgumentInfo(context.SourceGenericArg, "TResult");

        var ownTypeArgsList = intermediate.GetOwnTypeArgsList("TResult");
        var enumerableKind = intermediate.GetEnumerableKind();
        var enumerableTypeName = $"{intermediate.GetEnumerableTypeName(context.Level)}{ownTypeArgsList}";

        builder.AppendLine($"public {enumerableTypeName} {enumerableKind}{ownTypeArgsList}({argumentInfo.ToDisplayString()}) => new {enumerableTypeName}(this{argumentInfo.ToParameterList()});");
    }

    static void EmitStruct(SourceType source, CodeBuilder builder, EmitContext context, List<LinqTreeNode> nodes) {
        var intermediate = context.Element;

        var argumentInfo = intermediate.GetArgumentInfo(context.SourceGenericArg, context.GetResultGenericType());
        var argumentAssignment = argumentInfo != null ? $"this.{argumentInfo.Value.Name} = {argumentInfo.Value.Name};" : null;
        var argumentField = argumentInfo != null ? $"readonly {argumentInfo.ToDisplayString()};" : null;

        bool implementIEnumerable = nodes.Any(node => node is LinqTreeTerminalNode { Element: EnumerableNode });
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
{argumentField}
public {intermediate.GetEnumerableTypeName(context.Level)}({context.SourceType} source{argumentInfo.ToArgumentList()}) {{
    this.source = source;
    {argumentAssignment}
}}");

            foreach(var node in nodes) {
                switch(node) {
                    case LinqTreeTerminalNode terminalNode:
                        switch(terminalNode.Element) {
                            case ToListNode:
                                EmitToList(source, structBuilder, context);
                                break;
                            case EnumerableNode:
                                EnumeratorSourceBuilder.EmitGetEnumerator(source, structBuilder, context);
                                break;
                            case ToValueChainElement toValueChainElement:
                                ToValueSourceBuilder.EmitToValue(source, structBuilder, context, toValueChainElement.Type);
                                break;
                        }
                        break;
                    case LinqTreeIntermediateNode nextIntermediate:
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

    static void EmitToList(SourceType source, CodeBuilder builder, EmitContext context) {
        var outputType = context.GetOutputType();
        builder.AppendLine($@"public List<{outputType}> ToList() => Utils.AsList(ToArray());");
    }
}
public record EmitContext(Level Level, IntermediateNode Element, string SourceType, string SourceGenericArg, EmitContext? Parent) {

    public const string RootSourceType = "TSource";

    public static EmitContext Root(SourceType source, IntermediateNode element) 
        => new EmitContext(Level.Zero, element, source.GetSourceTypeName(RootSourceType), RootSourceType, null);

    public EmitContext Next(IntermediateNode element)
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

public record struct ArgumentInfo(string Type, string Name);

public static class CodeGenerationTraits {
    public static Level GetOrderByLevel(this PieceOfWork piece) => piece.Contexts.First(x => x.Element is OrderByNode).Level;
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

    public static bool HasGenericArg(this SourceType source) {
        return source switch {
            SourceType.List or SourceType.Array or SourceType.CustomEnumerable or SourceType.CustomCollection => true,
            SourceType.IList or SourceType.ICollection => false,
            _ => throw new NotImplementedException(),
        };
    }

    public static string GetSourceTypeName(this SourceType source, string sourceGenericArg) {
        return source switch {
            SourceType.List => $"List<{sourceGenericArg}>",
            SourceType.Array => $"{sourceGenericArg}[]",
            SourceType.CustomCollection => $"MetaLinq.Tests.CustomCollection<{sourceGenericArg}>",
            SourceType.CustomEnumerable => $"MetaLinq.Tests.CustomEnumerable<{sourceGenericArg}>",
            SourceType.IList => "IList",
            SourceType.ICollection => "ICollection",
            _ => throw new NotImplementedException(),
        };
    }

    public static ArgumentInfo? GetArgumentInfo(this IntermediateNode intermediate, string inType, string outType) {
        return intermediate switch {
            WhereNode or TakeWhileNode or SkipWhileNode => new ArgumentInfo($"Func<{inType}, bool>", "predicate"),
            OrderByNode or ThenByNode => new ArgumentInfo($"Func<{inType}, {outType}>", "keySelector"),
            SelectNode => new ArgumentInfo($"Func<{inType}, {outType}>", "selector"),
            SelectManyNode selectMany => new ArgumentInfo($"Func<{inType}, {selectMany.SourceType.GetSourceTypeName(outType)}>", "selector"),
            OfTypeNode or CastNode => null,
            _ => throw new NotImplementedException(),
        };
    }

    public static string ToDisplayString(this ArgumentInfo? info) => info != null ? $"{info.Value.Type} {info.Value.Name}" : string.Empty;
    public static string ToParameterList(this ArgumentInfo? info) => info != null ? $", {info.Value.Name}" : string.Empty;
    public static string ToArgumentList(this ArgumentInfo? info) => info != null ? $", {info.ToDisplayString()}" : string.Empty;

    public static string GetResultGenericType(this EmitContext context) => $"T{context.Level}_Result";
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
            SourceType.IList => "IList",
            SourceType.ICollection => "ICollection",
            _ => throw new NotImplementedException(),
        };
    }
    public static string GetEnumerableKind(this IntermediateNode intermediate) {
        return intermediate switch {
            WhereNode => "Where",
            OfTypeNode => "OfType",
            CastNode => "Cast",
            TakeWhileNode => "TakeWhile",
            SkipWhileNode => "SkipWhile",
            SelectNode => "Select",
            SelectManyNode => "SelectMany",
            OrderByNode orderBy => "OrderBy" + orderBy.Direction.GetDescendingSuffix(),
            ThenByNode thenBy => "ThenBy" + thenBy.Direction.GetDescendingSuffix(),
            _ => throw new NotImplementedException(),
        };
    }
    public static string? GetOwnTypeArgsList(this EmitContext context) {
        return context.Element.GetOwnTypeArgsList(context.GetResultGenericType());
    }
    public static string? GetOwnTypeArgsList(this IntermediateNode intermediate, string argName) {
        var ownTypeArg = intermediate.GetOwnTypeArg(argName);
        return ownTypeArg == null ? null : ownTypeArg.YieldToArray().GetTypeArgsList();
    }

    public static string? GetOwnTypeArg(this IntermediateNode intermediate, string argName) {
        return intermediate switch {
            WhereNode or TakeWhileNode or SkipWhileNode => null,
            SelectNode or SelectManyNode or OfTypeNode or CastNode or OrderByNode or ThenByNode => argName,
            _ => throw new NotImplementedException(),
        };
    }

    public static string? GetTypeArgsList(this string?[] args) {
        if(args.Length == 0) return null;
        return "<" + string.Join(", ", args.Where(x => x != null)) + ">";
    }

    public static string GetOutputType(this EmitContext context) {
            return context.Element switch {
                WhereNode or TakeWhileNode or SkipWhileNode or OrderByNode or ThenByNode or IdentityNode => context.SourceGenericArg,
                SelectNode or SelectManyNode or OfTypeNode or CastNode => context.GetResultGenericType(),
                _ => throw new NotImplementedException(),
            };
        }
    public static string GetCountName(this SourceType source) {
            return source switch {
                SourceType.List or SourceType.CustomCollection or SourceType.IList or SourceType.ICollection => "Count",
                SourceType.Array => "Length",
                _ => throw new NotImplementedException(),
            };
        }
    public static Level GetLabelIndex(this EmitContext context, int skip) {
            return context.GetContexts().Where(x => x.Element is SelectManyNode).Skip(skip).FirstOrDefault()?.Level.Next ?? Level.Zero;
        }
}
