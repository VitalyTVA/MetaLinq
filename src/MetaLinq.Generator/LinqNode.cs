using System.ComponentModel;

namespace MetaLinq.Generator;

public enum SourceType { List, Array, CustomCollection, CustomEnumerable, IList, ICollection }

public static class SourceTypeExtensions {
    public static bool HasCount(this SourceType sourceType) {
        return sourceType switch {
            SourceType.List or SourceType.Array or SourceType.CustomCollection or SourceType.IList or SourceType.ICollection => true,
            SourceType.CustomEnumerable => false,
            _ => throw new NotImplementedException(),
        };
    }
    public static bool HasIndexer(this SourceType sourceType) {
        return sourceType switch {
            SourceType.List or SourceType.Array or SourceType.IList => true,
            SourceType.CustomCollection or SourceType.CustomEnumerable or SourceType.ICollection => false,
            _ => throw new NotImplementedException(),
        };
    }
}

public abstract record LinqNode {
    public static LinqNode Where => WhereNode.Instance;
    public static LinqNode TakeWhile => TakeWhileNode.Instance;
    public static LinqNode SkipWhile => SkipWhileNode.Instance;
    public static LinqNode Select => SelectNode.Instance;
    public static LinqNode Cast => CastNode.Instance;
    public static LinqNode OrderBy => new OrderByNode(ListSortDirection.Ascending);
    public static LinqNode OrderByDescending = new OrderByNode(ListSortDirection.Descending);
    public static LinqNode ThenBy => new ThenByNode(ListSortDirection.Ascending);
    public static LinqNode ThenByDescending => new ThenByNode(ListSortDirection.Descending);
    public static LinqNode SelectMany(SourceType sourceType) => new SelectManyNode(sourceType);
    public static LinqNode OfType => OfTypeNode.Instance;
    public static ToListNode ToList => ToListNode.Instance;
    public static EnumerableNode Enumerable => EnumerableNode.Instance;

    public static readonly IComparer<LinqNode> Comparer = Comparer<LinqNode>.Create((x1, x2) => {
        var typeComparison = Comparer<string>.Default.Compare(x1.GetType().Name, x2.GetType().Name);
        if(typeComparison != 0)
            return typeComparison;
        return Comparer<string>.Default.Compare(x1.ToString(), x2.ToString());
    });
}

public enum ToValueType { 
    ToArray, 
    ToHashSet, 
    ToDictionary,
    Aggregate, Aggregate_Seed, Aggregate_Seed_Result,
    First, First_Predicate,
    FirstOrDefault, FirstOrDefault_Predicate,
    Last, Last_Predicate,
    LastOrDefault, LastOrDefault_Predicate,
    Single, Single_Predicate,
    SingleOrDefault, SingleOrDefault_Predicate,
    Any, Any_Predicate,
    All_Predicate,

    Sum,
    Average,
    Min,
    Max,

    Sum_Int_Selector, Sum_IntN_Selector, 
    Sum_Long_Selector, Sum_LongN_Selector, 
    Sum_Float_Selector, Sum_FloatN_Selector, 
    Sum_Double_Selector, Sum_DoubleN_Selector,
    Sum_Decimal_Selector, Sum_DecimalN_Selector,
    Average_Int_Selector, Average_IntN_Selector,
    Average_Long_Selector, Average_LongN_Selector,
    Average_Float_Selector, Average_FloatN_Selector,
    Average_Double_Selector, Average_DoubleN_Selector,
    Average_Decimal_Selector, Average_DecimalN_Selector,
    Min_Int_Selector, Min_IntN_Selector,
    Min_Long_Selector, Min_LongN_Selector,
    Min_Float_Selector, Min_FloatN_Selector,
    Min_Double_Selector, Min_DoubleN_Selector,
    Min_Decimal_Selector, Min_DecimalN_Selector,
    Max_Int_Selector, Max_IntN_Selector,
    Max_Long_Selector, Max_LongN_Selector,
    Max_Float_Selector, Max_FloatN_Selector,
    Max_Double_Selector, Max_DoubleN_Selector,
    Max_Decimal_Selector, Max_DecimalN_Selector,
}

public enum AggregateValueType { Int, Long, Float, Double, Decimal }
public enum AggregateKind { Sum, Min, Max, Average }
public record struct AggregateInfo(AggregateKind Kind, AggregateValueType? Type, bool Nullable, bool HasSelector);

public static class ValueTypeTraits {
    public static string ToMethodName(this ToValueType type) {
        return type switch {
            ToValueType.First or ToValueType.First_Predicate => "First",
            ToValueType.FirstOrDefault or ToValueType.FirstOrDefault_Predicate => "FirstOrDefault",
            ToValueType.Last or ToValueType.Last_Predicate => "Last",
            ToValueType.LastOrDefault or ToValueType.LastOrDefault_Predicate => "LastOrDefault",
            ToValueType.Single or ToValueType.Single_Predicate => "Single",
            ToValueType.SingleOrDefault or ToValueType.SingleOrDefault_Predicate => "SingleOrDefault",
            ToValueType.Any or ToValueType.Any_Predicate => "Any",
            ToValueType.All_Predicate => "All",
            _ => throw new InvalidOperationException(),
        };
    }
    public static AggregateInfo? GetAggregateInfo(this ToValueType value) {
        return value switch {
            ToValueType.Sum => new AggregateInfo(AggregateKind.Sum, null, false, HasSelector: false),
            ToValueType.Average => new AggregateInfo(AggregateKind.Average, null, false, HasSelector: false),
            ToValueType.Min => new AggregateInfo(AggregateKind.Min, null, false, HasSelector: false),
            ToValueType.Max => new AggregateInfo(AggregateKind.Max, null, false, HasSelector: false),

            ToValueType.Sum_Int_Selector => new AggregateInfo(AggregateKind.Sum, AggregateValueType.Int, false, HasSelector: true),
            ToValueType.Sum_IntN_Selector => new AggregateInfo(AggregateKind.Sum, AggregateValueType.Int, true, HasSelector: true),
            ToValueType.Sum_Long_Selector => new AggregateInfo(AggregateKind.Sum, AggregateValueType.Long, false, HasSelector: true),
            ToValueType.Sum_LongN_Selector => new AggregateInfo(AggregateKind.Sum, AggregateValueType.Long, true, HasSelector: true),
            ToValueType.Sum_Float_Selector => new AggregateInfo(AggregateKind.Sum, AggregateValueType.Float, false, HasSelector: true),
            ToValueType.Sum_FloatN_Selector => new AggregateInfo(AggregateKind.Sum, AggregateValueType.Float, true, HasSelector: true),
            ToValueType.Sum_Double_Selector => new AggregateInfo(AggregateKind.Sum, AggregateValueType.Double, false, HasSelector: true),
            ToValueType.Sum_DoubleN_Selector => new AggregateInfo(AggregateKind.Sum, AggregateValueType.Double, true, HasSelector: true),
            ToValueType.Sum_Decimal_Selector => new AggregateInfo(AggregateKind.Sum, AggregateValueType.Decimal, false, HasSelector: true),
            ToValueType.Sum_DecimalN_Selector => new AggregateInfo(AggregateKind.Sum, AggregateValueType.Decimal, true, HasSelector: true),

            ToValueType.Average_Int_Selector => new AggregateInfo(AggregateKind.Average, AggregateValueType.Int, false, HasSelector: true),
            ToValueType.Average_IntN_Selector => new AggregateInfo(AggregateKind.Average, AggregateValueType.Int, true, HasSelector: true),
            ToValueType.Average_Long_Selector => new AggregateInfo(AggregateKind.Average, AggregateValueType.Long, false, HasSelector: true),
            ToValueType.Average_LongN_Selector => new AggregateInfo(AggregateKind.Average, AggregateValueType.Long, true, HasSelector: true),
            ToValueType.Average_Float_Selector => new AggregateInfo(AggregateKind.Average, AggregateValueType.Float, false, HasSelector: true),
            ToValueType.Average_FloatN_Selector => new AggregateInfo(AggregateKind.Average, AggregateValueType.Float, true, HasSelector: true),
            ToValueType.Average_Double_Selector => new AggregateInfo(AggregateKind.Average, AggregateValueType.Double, false, HasSelector: true),
            ToValueType.Average_DoubleN_Selector => new AggregateInfo(AggregateKind.Average, AggregateValueType.Double, true, HasSelector: true),
            ToValueType.Average_Decimal_Selector => new AggregateInfo(AggregateKind.Average, AggregateValueType.Decimal, false, HasSelector: true),
            ToValueType.Average_DecimalN_Selector => new AggregateInfo(AggregateKind.Average, AggregateValueType.Decimal, true, HasSelector: true),

            ToValueType.Min_Int_Selector => new AggregateInfo(AggregateKind.Min, AggregateValueType.Int, false, HasSelector: true),
            ToValueType.Min_IntN_Selector => new AggregateInfo(AggregateKind.Min, AggregateValueType.Int, true, HasSelector: true),
            ToValueType.Min_Long_Selector => new AggregateInfo(AggregateKind.Min, AggregateValueType.Long, false, HasSelector: true),
            ToValueType.Min_LongN_Selector => new AggregateInfo(AggregateKind.Min, AggregateValueType.Long, true, HasSelector: true),
            ToValueType.Min_Float_Selector => new AggregateInfo(AggregateKind.Min, AggregateValueType.Float, false, HasSelector: true),
            ToValueType.Min_FloatN_Selector => new AggregateInfo(AggregateKind.Min, AggregateValueType.Float, true, HasSelector: true),
            ToValueType.Min_Double_Selector => new AggregateInfo(AggregateKind.Min, AggregateValueType.Double, false, HasSelector: true),
            ToValueType.Min_DoubleN_Selector => new AggregateInfo(AggregateKind.Min, AggregateValueType.Double, true, HasSelector: true),
            ToValueType.Min_Decimal_Selector => new AggregateInfo(AggregateKind.Min, AggregateValueType.Decimal, false, HasSelector: true),
            ToValueType.Min_DecimalN_Selector => new AggregateInfo(AggregateKind.Min, AggregateValueType.Decimal, true, HasSelector: true),

            ToValueType.Max_Int_Selector => new AggregateInfo(AggregateKind.Max, AggregateValueType.Int, false, HasSelector: true),
            ToValueType.Max_IntN_Selector => new AggregateInfo(AggregateKind.Max, AggregateValueType.Int, true, HasSelector: true),
            ToValueType.Max_Long_Selector => new AggregateInfo(AggregateKind.Max, AggregateValueType.Long, false, HasSelector: true),
            ToValueType.Max_LongN_Selector => new AggregateInfo(AggregateKind.Max, AggregateValueType.Long, true, HasSelector: true),
            ToValueType.Max_Float_Selector => new AggregateInfo(AggregateKind.Max, AggregateValueType.Float, false, HasSelector: true),
            ToValueType.Max_FloatN_Selector => new AggregateInfo(AggregateKind.Max, AggregateValueType.Float, true, HasSelector: true),
            ToValueType.Max_Double_Selector => new AggregateInfo(AggregateKind.Max, AggregateValueType.Double, false, HasSelector: true),
            ToValueType.Max_DoubleN_Selector => new AggregateInfo(AggregateKind.Max, AggregateValueType.Double, true, HasSelector: true),
            ToValueType.Max_Decimal_Selector => new AggregateInfo(AggregateKind.Max, AggregateValueType.Decimal, false, HasSelector: true),
            ToValueType.Max_DecimalN_Selector => new AggregateInfo(AggregateKind.Max, AggregateValueType.Decimal, true, HasSelector: true),

            _ => null
        };
    }
    public static bool IsOrderIndependentLoop(this ToValueType value) { 
       return GetAggregateInfo(value) != null || value 
            is ToValueType.All_Predicate 
            or ToValueType.Any or ToValueType.Any_Predicate 
            or ToValueType.Single or ToValueType.Single_Predicate
            or ToValueType.SingleOrDefault or ToValueType.SingleOrDefault_Predicate
            or ToValueType.ToDictionary or ToValueType.ToHashSet;
    }
    public static bool IsOrderDependentLoop(this ToValueType value) {
        return value 
            is ToValueType.First or ToValueType.First_Predicate
            or ToValueType.FirstOrDefault or ToValueType.FirstOrDefault_Predicate
            or ToValueType.Last or ToValueType.Last_Predicate
            or ToValueType.LastOrDefault or ToValueType.LastOrDefault_Predicate;
    }
    public static ToValueChainElement AsElement(this ToValueType type) => new ToValueChainElement(type);
    public static ToValueChainElement? AsElement(this ToValueType? type) => type != null ? new ToValueChainElement(type.Value) : null;
}

public abstract record class TerminalNode : LinqNode { }

public sealed record ToValueChainElement(ToValueType Type) : TerminalNode {
}

public sealed record EnumerableNode : TerminalNode {
    public static readonly EnumerableNode Instance = new EnumerableNode();
    EnumerableNode() { }
}

public sealed record ToListNode : TerminalNode {
    public static readonly ToListNode Instance = new ToListNode();
    ToListNode() { }
}

public abstract record class IntermediateNode : LinqNode { 
    internal abstract string Type { get; }
}

public sealed record WhereNode : IntermediateNode {
    public static readonly WhereNode Instance = new WhereNode();
    WhereNode() { }
    internal override string Type => "Where";
}

public sealed record OfTypeNode : IntermediateNode {
    public static readonly OfTypeNode Instance = new OfTypeNode();
    OfTypeNode() { }
    internal override string Type => "OfType";
}

public sealed record CastNode : IntermediateNode {
    public static readonly CastNode Instance = new CastNode();
    CastNode() { }
    internal override string Type => "Cast";
}

public sealed record IdentityNode : IntermediateNode {
    public static readonly IdentityNode Instance = new IdentityNode();
    IdentityNode() { }
    internal override string Type => "Identity";
}

public sealed record TakeWhileNode : IntermediateNode {
    public static readonly TakeWhileNode Instance = new TakeWhileNode();
    TakeWhileNode() { }
    internal override string Type => "TakeWhile";
}

public sealed record SkipWhileNode : IntermediateNode {
    public static readonly SkipWhileNode Instance = new SkipWhileNode();
    SkipWhileNode() { }
    internal override string Type => "SkipWhile";
}

public sealed record SelectNode : IntermediateNode {
    public static readonly SelectNode Instance = new SelectNode();
    SelectNode() { }
    internal override string Type => "Select";
}

public sealed record OrderByNode(ListSortDirection Direction) : IntermediateNode {
    internal override string Type => "OrderBy" + Direction.GetDescendingSuffix();
}
public sealed record ThenByNode(ListSortDirection Direction) : IntermediateNode {
    internal override string Type => "ThenBy" + Direction.GetDescendingSuffix();
}
public static class ListSortDirectionExtensions {
    public static string? GetDescendingSuffix(this ListSortDirection direction) => direction == ListSortDirection.Descending ? "Descending" : null;
}

public sealed record SelectManyNode(SourceType SourceType) : IntermediateNode {
    internal override string Type => "SelectMany " + SourceType;
}
