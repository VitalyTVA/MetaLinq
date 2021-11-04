using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MetaLinq.Generator {
    [Generator]
    public class MetaLinqGenerator : ISourceGenerator {
        public void Initialize(GeneratorInitializationContext context) {
            //Debugger.Launch();
            context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
        }

        public void Execute(GeneratorExecutionContext context) {
            MetaLinqGeneratorCore.Execute(context);
        }
    }

    class SyntaxContextReceiver : ISyntaxContextReceiver {
        public (bool toArray, bool iEnumerable)? ArrayWhereInfo { get; private set; }
        public (bool toArray, bool iEnumerable)? ListWhereInfo { get; private set; }

        INamedTypeSymbol? metaEnumerableType;
        INamedTypeSymbol? enumerableType;
        INamedTypeSymbol? listType;
        TypeSyntax metaEnumerableTypeSyntax = SyntaxFactory.ParseTypeName("MetaEnumerable");

        List<MemberAccessExpressionSyntax> visited = new();

        enum SourceType { List, Array }

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
            if(metaEnumerableType == null)
                metaEnumerableType = context.SemanticModel.Compilation.GetTypeByMetadataName("MetaLinq.MetaEnumerable");
            if(enumerableType == null)
                enumerableType = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Linq.Enumerable");
            if(listType == null)
                listType = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Collections.Generic.List`1");

            if(context.Node is MemberAccessExpressionSyntax memberAccess
                && !visited.Contains(memberAccess)
                && memberAccess.Expression is not MemberAccessExpressionSyntax
                && IsMetaEnumerableAccessible(context)
            ) {
                var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess.Name);
                if(symbolInfo.Symbol is IMethodSymbol methodSymbol
                    && SymbolEqualityComparer.Default.Equals(methodSymbol.OriginalDefinition.ContainingType, enumerableType)) {
                    if(methodSymbol.Name == "Where") {
                        var sourceType = GetSourceType(context, memberAccess!);
                        if(sourceType == SourceType.Array)
                            ArrayWhereInfo = (ArrayWhereInfo?.toArray ?? false, true);
                        else
                            ListWhereInfo = (ListWhereInfo?.toArray ?? false, true);
                    }
                    if(methodSymbol.Name == "ToArray") {
                        var nested = (memberAccess.Expression as InvocationExpressionSyntax)?.Expression as MemberAccessExpressionSyntax;
                        if(nested != null) {
                            var sourceType = GetSourceType(context, nested);
                            if(sourceType == SourceType.Array)
                                ArrayWhereInfo = (true, ArrayWhereInfo?.iEnumerable ?? false);
                            else
                                ListWhereInfo = (true, ListWhereInfo?.iEnumerable ?? false);
                            visited.Add(nested);
                        }
                    }
                }
            }
        }
        SourceType GetSourceType(GeneratorSyntaxContext context, MemberAccessExpressionSyntax memberAccess) {
            var sourceSymbol = context.SemanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
            var returnType = sourceSymbol switch {
                IMethodSymbol method => method.ReturnType,
                ILocalSymbol local => local.Type,
                IFieldSymbol field => field.Type,
                IPropertySymbol property => property.Type,
                _ => throw new InvalidOperationException()
            };
            if(returnType.TypeKind == TypeKind.Array)
                return SourceType.Array;
            if(SymbolEqualityComparer.Default.Equals(listType, returnType.OriginalDefinition))
                return SourceType.List;
            throw new InvalidOperationException();
        }
        bool IsMetaEnumerableAccessible(GeneratorSyntaxContext context) {
            var typeInfo = context.SemanticModel.GetSpeculativeTypeInfo(context.Node.SpanStart, metaEnumerableTypeSyntax, SpeculativeBindingOption.BindAsTypeOrNamespace);
            return SymbolEqualityComparer.Default.Equals(typeInfo.Type, metaEnumerableType);
        }
    }
}
