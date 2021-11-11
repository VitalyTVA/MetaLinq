using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MetaLinq.Generator {
    [Generator]
    public class MetaLinqGenerator : ISourceGenerator {
        public void Initialize(GeneratorInitializationContext context) {
            //Debugger.Launch();
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

        INamedTypeSymbol? metaEnumerableType;
        INamedTypeSymbol? enumerableType;
        INamedTypeSymbol? listType;
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
                IParameterSymbol parameter => parameter.Type,
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
