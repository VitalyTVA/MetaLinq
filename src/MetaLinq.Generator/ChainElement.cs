namespace MetaLinq.Generator;

public enum SourceType { List, Array, CustomEnumerable }

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
    public static readonly ChainElement ToArray = new ToInstanceChainElement(ToInstanceChainElementType.ToArray);
    public static readonly ChainElement ToList = new ToInstanceChainElement(ToInstanceChainElementType.ToList);
    public static readonly ChainElement ToHashSet = new ToInstanceChainElement(ToInstanceChainElementType.ToHashSet);
    public static readonly ChainElement ToDictionary = new ToInstanceChainElement(ToInstanceChainElementType.ToDictionary);
    public static ChainElement Enumerable => EnumerableChainElement.Instance;

    public static readonly IComparer<ChainElement> Comparer = Comparer<ChainElement>.Create((x1, x2) => {
        var typeComparison = Comparer<string>.Default.Compare(x1.GetType().Name, x2.GetType().Name);
        if(typeComparison != 0)
            return typeComparison;
        return Comparer<string>.Default.Compare(x1.ToString(), x2.ToString());
    });
}

public enum ToInstanceChainElementType { ToArray, ToList, ToHashSet, ToDictionary }
public sealed record ToInstanceChainElement(ToInstanceChainElementType Type) : ChainElement {
}

public sealed record EnumerableChainElement : ChainElement {
    public static readonly EnumerableChainElement Instance = new EnumerableChainElement();
    EnumerableChainElement() { }
}

public sealed record WhereChainElement : ChainElement {
    public static readonly WhereChainElement Instance = new WhereChainElement();
    WhereChainElement() { }
}

public sealed record TakeWhileChainElement : ChainElement {
    public static readonly TakeWhileChainElement Instance = new TakeWhileChainElement();
    TakeWhileChainElement() { }
}

public sealed record SkipWhileChainElement : ChainElement {
    public static readonly SkipWhileChainElement Instance = new SkipWhileChainElement();
    SkipWhileChainElement() { }
}

public sealed record SelectChainElement : ChainElement {
    public static readonly SelectChainElement Instance = new SelectChainElement();
    SelectChainElement() { }
}

public sealed record OrderByChainElement : ChainElement {
    public static readonly OrderByChainElement Instance = new OrderByChainElement();
    OrderByChainElement() { }
}

public sealed record OrderByDescendingChainElement : ChainElement {
    public static readonly OrderByDescendingChainElement Instance = new OrderByDescendingChainElement();
    OrderByDescendingChainElement() { }
}

public sealed record ThenByChainElement : ChainElement {
    public static readonly ThenByChainElement Instance = new ThenByChainElement();
    ThenByChainElement() { }
}

public sealed record ThenByDescendingChainElement : ChainElement {
    public static readonly ThenByDescendingChainElement Instance = new ThenByDescendingChainElement();
    ThenByDescendingChainElement() { }
}

public sealed record SelectManyChainElement(SourceType SourceType) : ChainElement {
}
