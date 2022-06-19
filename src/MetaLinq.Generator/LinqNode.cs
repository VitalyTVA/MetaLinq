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
    //Aggregate,
    First,
    FirstOrDefault,
    Last,
    LastOrDefault,
    Single,
    SingleOrDefault,
    Any,
    All,
    Sum_Int, Sum_IntN, 
    Sum_Long, Sum_LongN, 
    Sum_Float, Sum_FloatN, 
    Sum_Double, Sum_DoubleN,
    Sum_Decimal, Sum_DecimalN,
    Average_Int, Average_IntN,
    Average_Long, Average_LongN,
    Average_Float, Average_FloatN,
    Average_Double, Average_DoubleN,
    Average_Decimal, Average_DecimalN,
    Min_Int, Min_IntN,
    Min_Long, Min_LongN,
    Min_Float, Min_FloatN,
    Min_Double, Min_DoubleN,
    Min_Decimal, Min_DecimalN,
    Max_Int, Max_IntN,
    Max_Long, Max_LongN,
    Max_Float, Max_FloatN,
    Max_Double, Max_DoubleN,
    Max_Decimal, Max_DecimalN,
}

public enum AggregateValueType { Int, Long, Float, Double, Decimal }
public enum AggregateKind { Sum, Min, Max, Average }
public record struct AggregateInfo(AggregateKind Kind, AggregateValueType Type, bool Nullable);

public static class ValueTypeTraits {
    public static AggregateInfo? GetAggregateInfo(this ToValueType value) {
        return value switch {
            ToValueType.Sum_Int => new AggregateInfo(AggregateKind.Sum, AggregateValueType.Int, false),
            ToValueType.Sum_IntN => new AggregateInfo(AggregateKind.Sum, AggregateValueType.Int, true),
            ToValueType.Sum_Long => new AggregateInfo(AggregateKind.Sum, AggregateValueType.Long, false),
            ToValueType.Sum_LongN => new AggregateInfo(AggregateKind.Sum, AggregateValueType.Long, true),
            ToValueType.Sum_Float => new AggregateInfo(AggregateKind.Sum, AggregateValueType.Float, false),
            ToValueType.Sum_FloatN => new AggregateInfo(AggregateKind.Sum, AggregateValueType.Float, true),
            ToValueType.Sum_Double => new AggregateInfo(AggregateKind.Sum, AggregateValueType.Double, false),
            ToValueType.Sum_DoubleN => new AggregateInfo(AggregateKind.Sum, AggregateValueType.Double, true),
            ToValueType.Sum_Decimal => new AggregateInfo(AggregateKind.Sum, AggregateValueType.Decimal, false),
            ToValueType.Sum_DecimalN => new AggregateInfo(AggregateKind.Sum, AggregateValueType.Decimal, true),

            ToValueType.Average_Int => new AggregateInfo(AggregateKind.Average, AggregateValueType.Int, false),
            ToValueType.Average_IntN => new AggregateInfo(AggregateKind.Average, AggregateValueType.Int, true),
            ToValueType.Average_Long => new AggregateInfo(AggregateKind.Average, AggregateValueType.Long, false),
            ToValueType.Average_LongN => new AggregateInfo(AggregateKind.Average, AggregateValueType.Long, true),
            ToValueType.Average_Float => new AggregateInfo(AggregateKind.Average, AggregateValueType.Float, false),
            ToValueType.Average_FloatN => new AggregateInfo(AggregateKind.Average, AggregateValueType.Float, true),
            ToValueType.Average_Double => new AggregateInfo(AggregateKind.Average, AggregateValueType.Double, false),
            ToValueType.Average_DoubleN => new AggregateInfo(AggregateKind.Average, AggregateValueType.Double, true),
            ToValueType.Average_Decimal => new AggregateInfo(AggregateKind.Average, AggregateValueType.Decimal, false),
            ToValueType.Average_DecimalN => new AggregateInfo(AggregateKind.Average, AggregateValueType.Decimal, true),

            ToValueType.Min_Int => new AggregateInfo(AggregateKind.Min, AggregateValueType.Int, false),
            ToValueType.Min_IntN => new AggregateInfo(AggregateKind.Min, AggregateValueType.Int, true),
            ToValueType.Min_Long => new AggregateInfo(AggregateKind.Min, AggregateValueType.Long, false),
            ToValueType.Min_LongN => new AggregateInfo(AggregateKind.Min, AggregateValueType.Long, true),
            ToValueType.Min_Float => new AggregateInfo(AggregateKind.Min, AggregateValueType.Float, false),
            ToValueType.Min_FloatN => new AggregateInfo(AggregateKind.Min, AggregateValueType.Float, true),
            ToValueType.Min_Double => new AggregateInfo(AggregateKind.Min, AggregateValueType.Double, false),
            ToValueType.Min_DoubleN => new AggregateInfo(AggregateKind.Min, AggregateValueType.Double, true),
            ToValueType.Min_Decimal => new AggregateInfo(AggregateKind.Min, AggregateValueType.Decimal, false),
            ToValueType.Min_DecimalN => new AggregateInfo(AggregateKind.Min, AggregateValueType.Decimal, true),

            ToValueType.Max_Int => new AggregateInfo(AggregateKind.Max, AggregateValueType.Int, false),
            ToValueType.Max_IntN => new AggregateInfo(AggregateKind.Max, AggregateValueType.Int, true),
            ToValueType.Max_Long => new AggregateInfo(AggregateKind.Max, AggregateValueType.Long, false),
            ToValueType.Max_LongN => new AggregateInfo(AggregateKind.Max, AggregateValueType.Long, true),
            ToValueType.Max_Float => new AggregateInfo(AggregateKind.Max, AggregateValueType.Float, false),
            ToValueType.Max_FloatN => new AggregateInfo(AggregateKind.Max, AggregateValueType.Float, true),
            ToValueType.Max_Double => new AggregateInfo(AggregateKind.Max, AggregateValueType.Double, false),
            ToValueType.Max_DoubleN => new AggregateInfo(AggregateKind.Max, AggregateValueType.Double, true),
            ToValueType.Max_Decimal => new AggregateInfo(AggregateKind.Max, AggregateValueType.Decimal, false),
            ToValueType.Max_DecimalN => new AggregateInfo(AggregateKind.Max, AggregateValueType.Decimal, true),

            _ => null
        };
    }
    public static bool IsOrderIndependentLoop(this ToValueType value) { 
       return GetAggregateInfo(value) != null || value 
            is ToValueType.All or ToValueType.Any 
            or ToValueType.Single or ToValueType.SingleOrDefault;
    }
    public static bool IsOrderDependentLoop(this ToValueType value) {
        return value is ToValueType.First or ToValueType.FirstOrDefault or ToValueType.Last or ToValueType.LastOrDefault;
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
