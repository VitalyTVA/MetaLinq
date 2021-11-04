using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MetaLinq.Generator {
    [Generator]
    public class MetaLinqGenerator : ISourceGenerator {
        public void Initialize(GeneratorInitializationContext context) {
            context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
        }

        public void Execute(GeneratorExecutionContext context) {
            MetaLinqGeneratorCore.Execute(context);
        }
    }

    class SyntaxContextReceiver : ISyntaxContextReceiver {
        public (bool toArray, bool iEnumerable)? WhereInfo { get; private set; }

        INamedTypeSymbol? metaEnumerableType;
        INamedTypeSymbol? enumerableType;
        TypeSyntax metaEnumerableTypeSyntax = SyntaxFactory.ParseTypeName("MetaEnumerable");

        List<MemberAccessExpressionSyntax> visited = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
            if(metaEnumerableType == null)
                metaEnumerableType = context.SemanticModel.Compilation.GetTypeByMetadataName("MetaLinq.MetaEnumerable");
            if(enumerableType == null)
                enumerableType = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Linq.Enumerable");
            if(context.Node is MemberAccessExpressionSyntax memberAccess
                && !visited.Contains(memberAccess)
                && memberAccess.Expression is not MemberAccessExpressionSyntax
                && IsMetaEnumerableAccessible(context)
            ) {
                var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess.Name);
                if(symbolInfo.Symbol is IMethodSymbol methodSymbol
                    && SymbolEqualityComparer.Default.Equals(methodSymbol.OriginalDefinition.ContainingType, enumerableType)) {
                    if(methodSymbol.Name == "Where")
                        WhereInfo = (WhereInfo?.toArray ?? false, true);
                    if(methodSymbol.Name == "ToArray") {
                        WhereInfo = (true, WhereInfo?.iEnumerable ?? false);
                        var nested = (memberAccess.Expression as InvocationExpressionSyntax)?.Expression as MemberAccessExpressionSyntax;
                        if(nested != null)
                            visited.Add(nested);
                    }
                }
            }
        }

        bool IsMetaEnumerableAccessible(GeneratorSyntaxContext context) {
            var typeInfo = context.SemanticModel.GetSpeculativeTypeInfo(context.Node.SpanStart, metaEnumerableTypeSyntax, SpeculativeBindingOption.BindAsTypeOrNamespace);
            return SymbolEqualityComparer.Default.Equals(typeInfo.Type, metaEnumerableType);
        }
    }
}
