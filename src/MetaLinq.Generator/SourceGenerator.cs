using Microsoft.CodeAnalysis;
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
        public bool WhereFound { get; private set; }

        INamedTypeSymbol? metaLinqEnumerableType;

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
            if(metaLinqEnumerableType == null)
                metaLinqEnumerableType = context.SemanticModel.Compilation.GetTypeByMetadataName("MetaLinq.Enumerable");
            if(context.Node is MemberAccessExpressionSyntax memberAccess) {
                var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess.Name);
                if(symbolInfo.Symbol is IMethodSymbol { Name: "Where" } methodSymbol
                    && SymbolEqualityComparer.Default.Equals(methodSymbol.OriginalDefinition.ContainingType, metaLinqEnumerableType)) {
                    WhereFound = true;
                }
            }
        }
    }
}
