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
        public (bool toArray, bool iEnumerable)? WhereInfo { get; private set; }

        INamedTypeSymbol? metaLinqEnumerableType;

        List<MemberAccessExpressionSyntax> visited = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
            if(metaLinqEnumerableType == null)
                metaLinqEnumerableType = context.SemanticModel.Compilation.GetTypeByMetadataName("MetaLinq.Enumerable");
            if(context.Node is MemberAccessExpressionSyntax memberAccess && !visited.Contains(memberAccess)) {
                var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess.Name);
                if(symbolInfo.Symbol is IMethodSymbol methodSymbol
                    && SymbolEqualityComparer.Default.Equals(methodSymbol.OriginalDefinition.ContainingType, metaLinqEnumerableType)) {
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
    }
}
