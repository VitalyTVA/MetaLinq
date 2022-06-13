﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MetaLinq.Generator;

[Generator]
public class MetaLinqGenerator : ISourceGenerator {
    public void Initialize(GeneratorInitializationContext context) {
        //System.Diagnostics.Debugger.Launch();
        context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
    }


    static readonly DiagnosticDescriptor ExceptionDiagnosticDescriptor = CreateDiagnosticDescriptor("ML9999", "Exception", "Exception {0}");
    static DiagnosticDescriptor CreateDiagnosticDescriptor(string id, string title, string messageFormat, DiagnosticSeverity diagnosticSeverity = DiagnosticSeverity.Error) =>
        new DiagnosticDescriptor(id, title, messageFormat, "MetaLinq", DiagnosticSeverity.Error, true);


    public void Execute(GeneratorExecutionContext context) {
        if(context.SyntaxContextReceiver is not SyntaxContextReceiver receiver)
            return;
        if(receiver.Exception is (var error, var location)) {
            context.ReportDiagnostic(Diagnostic.Create(ExceptionDiagnosticDescriptor, location, error));
            return;
        }
        try {
            foreach(var (name, source) in SourceBuilder.BuildSource(receiver.Model)) {
                if(context.CancellationToken.IsCancellationRequested)
                    break;
                context.AddSource(name, SourceText.From(source, Encoding.UTF8));
            }
        } catch(Exception e) {
            context.ReportDiagnostic(Diagnostic.Create(ExceptionDiagnosticDescriptor, null, e));
        }
    }
}

class SyntaxContextReceiver : ISyntaxContextReceiver {
    public readonly LinqModel Model = new();

    record Types(
        INamedTypeSymbol metaEnumerableType,
        INamedTypeSymbol enumerableType,
        INamedTypeSymbol listType,
        INamedTypeSymbol iListType,
        INamedTypeSymbol iCollectionType,
        INamedTypeSymbol? customCollectionType,
        INamedTypeSymbol? customEnumerableType
    );

    Types? types;

    TypeSyntax metaEnumerableTypeSyntax = SyntaxFactory.ParseTypeName("MetaEnumerable");

    List<MemberAccessExpressionSyntax> visited = new();
    HashSet<ExpressionSyntax> visitedExpressions = new();

    public (Exception error, Location locaton)? Exception { get; private set; }

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
        if(Exception != null)
            return;
        try {
            OnVisitSyntaxNodeCore(context);
        } catch(Exception e) {
            Exception = (e, context.Node.GetLocation());
        }
    }
    void OnVisitSyntaxNodeCore(GeneratorSyntaxContext context) {
        types = types ?? new Types(
            metaEnumerableType: context.SemanticModel.Compilation.GetTypeByMetadataName("MetaLinq.MetaEnumerable")!,
            enumerableType: context.SemanticModel.Compilation.GetTypeByMetadataName("System.Linq.Enumerable")!,
            listType: context.SemanticModel.Compilation.GetTypeByMetadataName("System.Collections.Generic.List`1")!,
            iListType: context.SemanticModel.Compilation.GetTypeByMetadataName("System.Collections.IList")!,
            iCollectionType: context.SemanticModel.Compilation.GetTypeByMetadataName("System.Collections.ICollection")!,
            customCollectionType: context.SemanticModel.Compilation.GetTypeByMetadataName("MetaLinq.Tests.CustomCollection`1"),
            customEnumerableType: context.SemanticModel.Compilation.GetTypeByMetadataName("MetaLinq.Tests.CustomEnumerable`1")
        );

        if(context.Node is InvocationExpressionSyntax invocation
            && !visitedExpressions.Contains(invocation)
            && IsMetaEnumerableAccessible(context)) {
            Stack<LinqNode> chain = new();
            SourceType? source = null;
            ExpressionSyntax? currentExpression = invocation;
            while(currentExpression != null) {
                bool chained = false;
                if(currentExpression is InvocationExpressionSyntax currentInvocation && currentInvocation.Expression is MemberAccessExpressionSyntax currentMemberAccess) {
                    var currentSymbolInfo = context.SemanticModel.GetSymbolInfo(currentMemberAccess.Name);
                    if(currentSymbolInfo.Symbol is IMethodSymbol currentMethodSymbol
                        && SymbolEqualityComparer.Default.Equals(currentMethodSymbol.OriginalDefinition.ContainingType, types!.enumerableType)) {
                        var element = TryGetSimpleChainElement(currentMethodSymbol.Name);
                        if(element != null) {
                            chain.Push(element);
                            chained = true;
                        }
                        if(currentMethodSymbol.Name == "SelectMany") {
                            var lambda = ((LambdaExpressionSyntax)currentInvocation.ArgumentList.Arguments[0].Expression).ExpressionBody!;
                            var soruceType = GetSourceType(context, lambda);
                            chain.Push(LinqNode.SelectMany(soruceType!.Value));
                            chained = true;
                        }
                    }
                }
                if(!chained) {
                    if(chain.Count != 0)
                        source = GetSourceType(context, currentExpression);
                    break;
                }
                currentExpression = ((currentExpression as InvocationExpressionSyntax)?.Expression as MemberAccessExpressionSyntax)?.Expression;
                if(currentExpression != null)
                    visitedExpressions.Add(currentExpression);
            }
            if(source != null) {
                Model.AddChain(source.Value, chain);
            }
        }
    }
    static LinqNode? TryGetSimpleChainElement(string funcName) {
        return funcName switch {
            "Where" => LinqNode.Where,
            "OfType" => LinqNode.OfType,
            "Cast" => LinqNode.Cast,
            "TakeWhile" => LinqNode.TakeWhile,
            "SkipWhile" => LinqNode.SkipWhile,
            "Select" => LinqNode.Select,
            "OrderBy" => LinqNode.OrderBy,
            "OrderByDescending" => LinqNode.OrderByDescending,
            "ThenBy" => LinqNode.ThenBy,
            "ThenByDescending" => LinqNode.ThenByDescending,
            "ToArray" => LinqNode.ToArray,
            "ToList" => LinqNode.ToList,
            "ToHashSet" => LinqNode.ToHashSet,
            "ToDictionary" => LinqNode.ToDictionary,
            "First" => LinqNode.First,
            "FirstOrDefault" => LinqNode.FirstOrDefault,
            "Last" => LinqNode.Last,
            "LastOrDefault" => LinqNode.LastOrDefault,
            "Any" => LinqNode.Any,
            "All" => LinqNode.All,
            "Single" => LinqNode.Single,
            "SingleOrDefault" => LinqNode.SingleOrDefault,
            "Sum" => LinqNode.Sum_Int,
            _ => null
        };
    }
    SourceType? GetSourceType(GeneratorSyntaxContext context, ExpressionSyntax expression) {
        var returnType = context.SemanticModel.GetTypeInfo(expression).Type;
        var sourceSymbol = context.SemanticModel.GetSymbolInfo(expression).Symbol;
        if(/*returnType == null || */sourceSymbol is INamedTypeSymbol)
            return null;
        if(returnType!.TypeKind == TypeKind.Array)
            return SourceType.Array;
        if(SymbolEqualityComparer.Default.Equals(types!.listType, returnType.OriginalDefinition))
            return SourceType.List;
        if(SymbolEqualityComparer.Default.Equals(types!.customEnumerableType, returnType.OriginalDefinition))
            return SourceType.CustomEnumerable;
        if(SymbolEqualityComparer.Default.Equals(types!.customCollectionType, returnType.OriginalDefinition))
            return SourceType.CustomCollection;
        if(SymbolEqualityComparer.Default.Equals(types!.iListType, returnType.OriginalDefinition))
            return SourceType.IList;
        if(SymbolEqualityComparer.Default.Equals(types!.iCollectionType, returnType.OriginalDefinition))
            return SourceType.ICollection;
        throw new InvalidOperationException();
    }
    bool IsMetaEnumerableAccessible(GeneratorSyntaxContext context) {
        var typeInfo = context.SemanticModel.GetSpeculativeTypeInfo(context.Node.SpanStart, metaEnumerableTypeSyntax, SpeculativeBindingOption.BindAsTypeOrNamespace);
        return SymbolEqualityComparer.Default.Equals(typeInfo.Type, types!.metaEnumerableType);
    }
}
