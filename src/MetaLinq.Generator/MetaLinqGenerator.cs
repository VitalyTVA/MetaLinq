using Microsoft.CodeAnalysis;
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
        INamedTypeSymbol? customEnumerableType,
        INamedTypeSymbol nullableType
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
            customEnumerableType: context.SemanticModel.Compilation.GetTypeByMetadataName("MetaLinq.Tests.CustomEnumerable`1"),
            nullableType: context.SemanticModel.Compilation.GetTypeByMetadataName("System.Nullable`1")!
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
                        var element = TryGetSimpleChainElement(currentMethodSymbol);
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
    LinqNode? TryGetSimpleChainElement(IMethodSymbol method) {
        ToValueType GetAggregateToValueType(
            ToValueType @int, ToValueType intN,
            ToValueType @long, ToValueType longN,
            ToValueType @float, ToValueType floatN,
            ToValueType @double, ToValueType doubleN,
            ToValueType @decimal, ToValueType decimalN
        ) {
            var type = (INamedTypeSymbol)method.ReturnType;
            bool nullable = SymbolEqualityComparer.Default.Equals(type.ConstructedFrom, types!.nullableType);
            if(nullable) { 
                type = (INamedTypeSymbol)type.TypeArguments.Single();
            }
            return (type.SpecialType, nullable) switch {
                (SpecialType.System_Int32, false) => @int,
                (SpecialType.System_Int32, true) => intN,
                (SpecialType.System_Int64, false) => @long,
                (SpecialType.System_Int64, true) => longN,
                (SpecialType.System_Single, false) => @float,
                (SpecialType.System_Single, true) => floatN,
                (SpecialType.System_Double, false) => @double,
                (SpecialType.System_Double, true) => doubleN,
                (SpecialType.System_Decimal, false) => @decimal,
                (SpecialType.System_Decimal, true) => decimalN,
                _ => throw new InvalidOperationException()
            };
        };
        ToValueType ChooseOverload(Func<ToValueType>? noArgs = null, Func<ToValueType>? oneArg = null, Func<ToValueType>? twoArgs = null, Func<ToValueType>? threeArgs = null) {
            var type = method.Parameters.Length switch {
                0 => noArgs!(),
                1 => oneArg!(),
                2 => twoArgs!(),
                3 => threeArgs!(),
                _ => throw new InvalidOperationException()
            };
            return type;
        };
        return method.Name switch {
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
            "ToList" => LinqNode.ToList,
            _ => null
        } ?? ValueTypeTraits.AsElement(method.Name switch {
            "ToArray" => ToValueType.ToArray,
            "ToHashSet" => ToValueType.ToHashSet,
            "ToDictionary" => ToValueType.ToDictionary,
            "First" => ChooseOverload(noArgs: () => ToValueType.First, oneArg: () => ToValueType.First_Predicate),
            "FirstOrDefault" => ChooseOverload(noArgs: () => ToValueType.FirstOrDefault, oneArg: () => ToValueType.FirstOrDefault_Predicate),
            "Last" => ChooseOverload(noArgs: () => ToValueType.Last, oneArg: () => ToValueType.Last_Predicate),
            "LastOrDefault" => ChooseOverload(noArgs: () => ToValueType.LastOrDefault, oneArg: () => ToValueType.LastOrDefault_Predicate),
            "Any" => ChooseOverload(noArgs: () => ToValueType.Any, oneArg: () => ToValueType.Any_Predicate),
            "All" => ToValueType.All_Predicate,
            "Single" => ChooseOverload(noArgs: () => ToValueType.Single, oneArg: () => ToValueType.Single_Predicate),
            "SingleOrDefault" => ChooseOverload(noArgs: () => ToValueType.SingleOrDefault, oneArg: () => ToValueType.SingleOrDefault_Predicate),
            "Sum" => ChooseOverload(
                noArgs: () => ToValueType.Sum, 
                oneArg: () => GetAggregateToValueType(
                    ToValueType.Sum_Int_Selector, ToValueType.Sum_IntN_Selector,
                    ToValueType.Sum_Long_Selector, ToValueType.Sum_LongN_Selector,
                    ToValueType.Sum_Float_Selector, ToValueType.Sum_FloatN_Selector,
                    ToValueType.Sum_Double_Selector, ToValueType.Sum_DoubleN_Selector,
                    ToValueType.Sum_Decimal_Selector, ToValueType.Sum_DecimalN_Selector
                )
            ),
            "Average" => ChooseOverload(
                noArgs: () => throw new InvalidOperationException("Average without selector not supported"), //TODO diagnostic instead of exception
                oneArg: () => GetAggregateToValueType(
                    ToValueType.Average_Int_Selector, ToValueType.Average_IntN_Selector,
                    ToValueType.Average_Long_Selector, ToValueType.Average_LongN_Selector,
                    ToValueType.Average_Float_Selector, ToValueType.Average_FloatN_Selector,
                    ToValueType.Average_Double_Selector, ToValueType.Average_DoubleN_Selector,
                    ToValueType.Average_Decimal_Selector, ToValueType.Average_DecimalN_Selector
                )
            ),
            "Min" => ChooseOverload(
                noArgs: () => ToValueType.Min,
                oneArg: () => GetAggregateToValueType(
                    ToValueType.Min_Int_Selector, ToValueType.Min_IntN_Selector,
                    ToValueType.Min_Long_Selector, ToValueType.Min_LongN_Selector,
                    ToValueType.Min_Float_Selector, ToValueType.Min_FloatN_Selector,
                    ToValueType.Min_Double_Selector, ToValueType.Min_DoubleN_Selector,
                    ToValueType.Min_Decimal_Selector, ToValueType.Min_DecimalN_Selector
                )
            ),
            "Max" => ChooseOverload(
                noArgs: () => ToValueType.Max,
                oneArg: () => GetAggregateToValueType(
                    ToValueType.Max_Int_Selector, ToValueType.Max_IntN_Selector,
                    ToValueType.Max_Long_Selector, ToValueType.Max_LongN_Selector,
                    ToValueType.Max_Float_Selector, ToValueType.Max_FloatN_Selector,
                    ToValueType.Max_Double_Selector, ToValueType.Max_DoubleN_Selector,
                    ToValueType.Max_Decimal_Selector, ToValueType.Max_DecimalN_Selector
                )
            ),
            "Aggregate" => ChooseOverload(
                oneArg: () => ToValueType.Aggregate, 
                twoArgs: () => ToValueType.Aggregate_Seed, 
                threeArgs: () => ToValueType.Aggregate_Seed_Result
            ),
            _ => null
        });
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
