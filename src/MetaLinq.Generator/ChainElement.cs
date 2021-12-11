namespace MetaLinq.Generator;

public enum SourceType { List, Array, CustomCollection, CustomEnumerable }

public static class SourceTypeExtensions {
    public static bool HasCount(this SourceType sourceType) {
        return sourceType switch {
            SourceType.List or SourceType.Array or SourceType.CustomCollection => true,
            SourceType.CustomEnumerable => false,
            _ => throw new NotImplementedException(),
        };
    }
    public static bool HasIndexer(this SourceType sourceType) {
        return sourceType switch {
            SourceType.List or SourceType.Array => true,
            SourceType.CustomCollection or SourceType.CustomEnumerable => false,
            _ => throw new NotImplementedException(),
        };
    }
}

public abstract record ChainElement {
    public static ChainElement Where => WhereChainElement.Instance;
    public static ChainElement TakeWhile => TakeWhileChainElement.Instance;
    public static ChainElement SkipWhile => SkipWhileChainElement.Instance;
    public static ChainElement Select => SelectChainElement.Instance;
    public static ChainElement OrderBy => OrderByChainElement.Instance;
    public static ChainElement OrderByDescending => OrderByDescendingChainElement.Instance;
    public static ChainElement ThenBy => ThenByChainElement.Instance;
    public static ChainElement ThenByDescending => ThenByDescendingChainElement.Instance;
    public static ChainElement SelectMany(SourceType sourceType) => new SelectManyChainElement(sourceType);
    public static readonly ToValueChainElement ToArray = new ToValueChainElement(ToValueType.ToArray);
    public static readonly ToValueChainElement ToHashSet = new ToValueChainElement(ToValueType.ToHashSet);
    public static readonly ToValueChainElement ToDictionary = new ToValueChainElement(ToValueType.ToDictionary);
    public static readonly ToValueChainElement First = new ToValueChainElement(ToValueType.First);
    public static readonly ToValueChainElement FirstOrDefault = new ToValueChainElement(ToValueType.FirstOrDefault);
    public static ToListChainElement ToList => ToListChainElement.Instance;
    public static EnumerableChainElement Enumerable => EnumerableChainElement.Instance;

    public static readonly IComparer<ChainElement> Comparer = Comparer<ChainElement>.Create((x1, x2) => {
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
    FirstOrDefault
}

public abstract record class TerminalChainElement : ChainElement { }

public sealed record ToValueChainElement(ToValueType Type) : TerminalChainElement {
}

public sealed record EnumerableChainElement : TerminalChainElement {
    public static readonly EnumerableChainElement Instance = new EnumerableChainElement();
    EnumerableChainElement() { }
}

public sealed record ToListChainElement : TerminalChainElement {
    public static readonly ToListChainElement Instance = new ToListChainElement();
    ToListChainElement() { }
}

public abstract record class IntermediateChainElement : ChainElement { 
    internal abstract string Type { get; }
}

public sealed record WhereChainElement : IntermediateChainElement {
    public static readonly WhereChainElement Instance = new WhereChainElement();
    WhereChainElement() { }
    internal override string Type => "Where";
}

public sealed record TakeWhileChainElement : IntermediateChainElement {
    public static readonly TakeWhileChainElement Instance = new TakeWhileChainElement();
    TakeWhileChainElement() { }
    internal override string Type => "TakeWhile";
}

public sealed record SkipWhileChainElement : IntermediateChainElement {
    public static readonly SkipWhileChainElement Instance = new SkipWhileChainElement();
    SkipWhileChainElement() { }
    internal override string Type => "SkipWhile";
}

public sealed record SelectChainElement : IntermediateChainElement {
    public static readonly SelectChainElement Instance = new SelectChainElement();
    SelectChainElement() { }
    internal override string Type => "Select";
}

public sealed record OrderByChainElement : IntermediateChainElement {
    public static readonly OrderByChainElement Instance = new OrderByChainElement();
    OrderByChainElement() { }
    internal override string Type => "OrderBy";
}

public sealed record OrderByDescendingChainElement : IntermediateChainElement {
    public static readonly OrderByDescendingChainElement Instance = new OrderByDescendingChainElement();
    OrderByDescendingChainElement() { }
    internal override string Type => "OrderByDescending";
}

public sealed record ThenByChainElement : IntermediateChainElement {
    public static readonly ThenByChainElement Instance = new ThenByChainElement();
    ThenByChainElement() { }
    internal override string Type => "ThenBy";
}

public sealed record ThenByDescendingChainElement : IntermediateChainElement {
    public static readonly ThenByDescendingChainElement Instance = new ThenByDescendingChainElement();
    ThenByDescendingChainElement() { }
    internal override string Type => "ThenByDescending";
}

public sealed record SelectManyChainElement(SourceType SourceType) : IntermediateChainElement {
    internal override string Type => "SelectMany " + SourceType;
}
