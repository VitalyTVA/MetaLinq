﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        public LinqModel Model = new();

        INamedTypeSymbol? metaEnumerableType;
        INamedTypeSymbol? enumerableType;
        INamedTypeSymbol? listType;
        TypeSyntax metaEnumerableTypeSyntax = SyntaxFactory.ParseTypeName("MetaEnumerable");

        List<MemberAccessExpressionSyntax> visited = new();
        HashSet<ExpressionSyntax> visitedExpressions = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
            if(metaEnumerableType == null)
                metaEnumerableType = context.SemanticModel.Compilation.GetTypeByMetadataName("MetaLinq.MetaEnumerable");
            if(enumerableType == null)
                enumerableType = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Linq.Enumerable");
            if(listType == null)
                listType = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Collections.Generic.List`1");

            if(context.Node is InvocationExpressionSyntax invocation
                && !visitedExpressions.Contains(invocation)
                && IsMetaEnumerableAccessible(context)) {
                Stack<ChainElement> chain = new();
                SourceType? source = null;
                ExpressionSyntax? currentExpression = invocation;
                while(currentExpression != null) {
                    bool chained = false;
                    if((currentExpression as InvocationExpressionSyntax)?.Expression is MemberAccessExpressionSyntax currentMemberAccess) {
                        var currentSymbolInfo = context.SemanticModel.GetSymbolInfo(currentMemberAccess.Name);
                        if(currentSymbolInfo.Symbol is IMethodSymbol currentMethodSymbol
                            && SymbolEqualityComparer.Default.Equals(currentMethodSymbol.OriginalDefinition.ContainingType, enumerableType)) {
                            if(currentMethodSymbol.Name == "Where") {
                                chain.Push(ChainElement.Where);
                                chained = true;
                            }
                            if(currentMethodSymbol.Name == "Select") {
                                chain.Push(ChainElement.Select);
                                chained = true;
                            }
                            if(currentMethodSymbol.Name == "ToArray") {
                                chain.Push(ChainElement.ToArray);
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
        SourceType? GetSourceType(GeneratorSyntaxContext context, ExpressionSyntax expression) {
            var sourceSymbol = context.SemanticModel.GetSymbolInfo(expression).Symbol;
            var returnType = sourceSymbol switch {
                IMethodSymbol method => method.ReturnType,
                ILocalSymbol local => local.Type,
                IFieldSymbol field => field.Type,
                IPropertySymbol property => property.Type,
                INamedTypeSymbol => null,
                _ => throw new InvalidOperationException()
            };
            if(returnType == null)
                return null;
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
