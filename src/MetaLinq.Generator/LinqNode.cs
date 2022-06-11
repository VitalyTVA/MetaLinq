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
    public static readonly ToValueChainElement ToArray = new ToValueChainElement(ToValueType.ToArray);
    public static readonly ToValueChainElement ToHashSet = new ToValueChainElement(ToValueType.ToHashSet);
    public static readonly ToValueChainElement ToDictionary = new ToValueChainElement(ToValueType.ToDictionary);
    public static readonly ToValueChainElement First = new ToValueChainElement(ToValueType.First);
    public static readonly ToValueChainElement FirstOrDefault = new ToValueChainElement(ToValueType.FirstOrDefault);
    public static readonly ToValueChainElement Last = new ToValueChainElement(ToValueType.Last);
    public static readonly ToValueChainElement LastOrDefault = new ToValueChainElement(ToValueType.LastOrDefault);
    public static readonly ToValueChainElement Any = new ToValueChainElement(ToValueType.Any);
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
    First,
    FirstOrDefault,
    Last,
    LastOrDefault,
    Any,
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
