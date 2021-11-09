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
                var enumerableSourceType = source.GetRootSourceType(sourceGenericArg);
                var context = new EmitContext(0, enumerableSourceType, sourceGenericArg, null);
                EmitStruct(source, intermediate, sourceTypeBuilder, context);
            }
        }
        static void EmitExtensionMethod(SourceType source, CodeBuilder builder, IntermediateNode intermediate)
        {
            var sourceName = source.GetEnumerableSourceName();
            var argumentName = intermediate.GetArgumentName();
            var enumerableKind = intermediate.GetEnumerableKind();
            var methodArgumentType = intermediate switch
            {
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
            if(intermediate is WhereNode) {
                //something like EmitExtensionMethod here...
                var nextIntermediateType = intermediate.GetEnumerableKind();
                builder.AppendLine($"public {nextIntermediateType}En {nextIntermediateType}(Func<{outputType}, bool> predicate) => new WhereEn(this, predicate);");
            } else
                throw new NotImplementedException();
        }

        static void EmitStruct(SourceType source, IntermediateNode intermediate, CodeBuilder builder, EmitContext context) {
            var argumentName = intermediate.GetArgumentName();
            var argumentType = intermediate switch {
                WhereNode => $"Func<{context.SourceGenericArg}, bool>",
                SelectNode => $"Func<{context.SourceGenericArg}, {"Result".GetLevelGenericType(context.Level)}>",
                _ => throw new NotImplementedException(),
            };
            var enumerableKind = intermediate.GetEnumerableKind();
            var nodes = intermediate.GetNodes().ToList();
            bool implementIEnumerable = nodes.Contains(TerminalNode.Enumerable);
            var outputType = intermediate.GetOutputType(context);
            string? generics = intermediate.GetOwnTypeArg(context);
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
                            EmitToArray(source, structBuilder, intermediate, context);
                            break;
                        case TerminalNode { Type: TerminalNodeType.Enumerable }:
                            EmitGetEnumerator(source, structBuilder, intermediate, context);
                            break;
                        case IntermediateNode nextIntermediate:
                            EmitStructMethod(outputType, structBuilder, nextIntermediate);
                            var nextContext = new EmitContext(context.Level + 1, $"{typeName}{CodeGenerationTraits.ToTypeArgsList(generics)}", outputType, context);
                            EmitStruct(source, nextIntermediate, structBuilder, nextContext);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }

            }
        }

        static void EmitGetEnumerator(SourceType source, CodeBuilder builder, IntermediateNode intermediate, EmitContext context) {
            var countName = source switch {
                SourceType.List => "Count",
                SourceType.Array => "Length",
                _ => throw new NotImplementedException(),
            };
            var outputType = intermediate.GetOutputType(context);
            var enumerableKind = intermediate.GetEnumerableKind();
            var ownTypeArgsList = CodeGenerationTraits.ToTypeArgsList(intermediate.GetOwnTypeArg(context));
            builder.AppendLine("#nullable disable");
            using(builder.BuildType(out CodeBuilder enumeratorBuilder, TypeModifiers.Struct, "Enumerator", isPublic: true, baseType: $"IEnumerator<{outputType}>")) {
                enumeratorBuilder.AppendMultipleLines($@"
{enumerableKind}En{ownTypeArgsList} source;
int index;
{outputType} current;
public Enumerator({enumerableKind}En{ownTypeArgsList} source) {{
    this.source = source;
    index = -1;
    current = default;
}}
public {outputType} Current => current;
public bool MoveNext() {{
    var len = source.source.{countName};
    while(true) {{
        index++;
        if(index >= len)
            break;");

            switch(intermediate) {
                case WhereNode:
                    enumeratorBuilder.AppendMultipleLines($@"
        if(source.predicate(source.source[index])) {{
            current = source.source[index];
            return true;
        }}");
                    break;
                case SelectNode:
                    enumeratorBuilder.AppendMultipleLines($@"
        current = source.selector(source.source[index]);
            return true;");
                    break;
                default:
                    throw new NotImplementedException();

            }

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
public Enumerator GetEnumerator() => new Enumerator(this);
IEnumerator<{outputType}> IEnumerable<{outputType}>.GetEnumerator() {{
    return new Enumerator(this);
}}
IEnumerator IEnumerable.GetEnumerator() {{
    throw new NotImplementedException();
}}
");
        }

        static void EmitToArray(SourceType source, CodeBuilder builder, IntermediateNode intermediate, EmitContext context) {
            var outputType = intermediate.GetOutputType(context);

            builder.AppendMultipleLines($@"
public {outputType}[] ToArray() {{
    using var result = new LargeArrayBuilder<{outputType}>(ArrayPool<{outputType}>.Shared, false);");

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

    public record EmitContext(int Level, string SourceType, string SourceGenericArg, EmitContext? parent);

    public static class CodeGenerationTraits {
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
        public static string? GetOwnTypeArg(this IntermediateNode intermediate, EmitContext context) {
            return intermediate.GetOwnTypeArg("Result".GetLevelGenericType(context.Level));
        }
        public static string? GetOwnTypeArg(this IntermediateNode intermediate, string argName) {
            return intermediate switch {
                WhereNode => null,
                SelectNode => argName,
                _ => throw new NotImplementedException(),
            };
        }
        public static string GetOutputType(this IntermediateNode intermediate, EmitContext context) {
            return intermediate switch {
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
