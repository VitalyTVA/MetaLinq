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
                            if(currentMethodSymbol.Name == "ToArray") {
                                chain.Push(ChainElement.ToArray);
                                chained = true;
                            }
                        }
                    }
                    if(!chained) {
                        if(chain.Count != 0)
                            source = GetSourceType___(context, currentExpression);
                        break;
                    }
                    currentExpression = ((currentExpression as InvocationExpressionSyntax)?.Expression as MemberAccessExpressionSyntax)?.Expression;
                    if(currentExpression != null)
                        visitedExpressions.Add(currentExpression);
                }
                if(source != null) {
                    var top = chain.Pop();
                    Debug.Assert(top == ChainElement.Where);
                    bool toArray = false;
                    bool iEnumerable = false;

                    if(chain.Count == 0)
                        iEnumerable = true;
                    else {
                        var next = chain.Pop();
                        Debug.Assert(next == ChainElement.ToArray);
                        toArray = true;
                    }

                    Debug.Assert(chain.Count == 0);
                    if(source == SourceType.Array) {
                        ArrayWhereInfo = (
                            toArray || (ArrayWhereInfo?.toArray ?? false),
                            iEnumerable || (ArrayWhereInfo?.iEnumerable ?? false)
                        );
                    }
                    if(source == SourceType.List) {
                        ListWhereInfo = (
                            toArray || (ListWhereInfo?.toArray ?? false),
                            iEnumerable || (ListWhereInfo?.iEnumerable ?? false)
                        );
                    }
                }
            }
        }
        SourceType? GetSourceType___(GeneratorSyntaxContext context, ExpressionSyntax expression) {
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
    enum SourceType { List, Array }

    enum ChainElement { Where, ToArray }

    //class LinqTree {
    //    public readonly SourceType Source;
    //    public readonly List<LinqNode> Nodes = new List<LinqNode>();
    //    public LinqTree(SourceType source) {
    //        Source = source;
    //    }

    //}
    //abstract class LinqNode { 
    //}

    //enum TerminalNodeType { ToArray, Enumerable }

    //sealed class TerminalNode : LinqNode {
    //    public static readonly TerminalNode ToArray = new TerminalNode(TerminalNodeType.ToArray);
    //    public static readonly TerminalNode Enumerable = new TerminalNode(TerminalNodeType.Enumerable);
    //    public readonly TerminalNodeType Type;
    //    TerminalNode(TerminalNodeType type) {
    //        Type = type;
    //    }
    //}

    //abstract class IntermediateNode : LinqNode {
    //    public readonly List<LinqNode> Nodes = new List<LinqNode>();
    //}

    //sealed class WhereNode : IntermediateNode {
    //    public WhereNode() { }
    //}

    //sealed class SelectNode : IntermediateNode {
    //}
}
