namespace MetaLinq.Generator;

public enum SourceType { List, Array }

public abstract record ChainElement {
    public static ChainElement Where => WhereChainElement.Instance;
    public static ChainElement Select => SelectChainElement.Instance;
    public static ChainElement OrderBy => OrderByChainElement.Instance;
    public static ChainElement OrderByDescending => OrderByDescendingChainElement.Instance;
    public static ChainElement SelectMany(SourceType sourceType) => new SelectManyChainElement(sourceType);
    public static ChainElement ToArray => ToArrayChainElement.Instance;
    public static ChainElement ToList => ToListChainElement.Instance;
}

public sealed record ToArrayChainElement : ChainElement {
    public static readonly ToArrayChainElement Instance = new ToArrayChainElement();
    ToArrayChainElement() { }
}

public sealed record ToListChainElement : ChainElement {
    public static readonly ToListChainElement Instance = new ToListChainElement();
    ToListChainElement() { }
}

public sealed record WhereChainElement : ChainElement {
    public static readonly WhereChainElement Instance = new WhereChainElement();
    WhereChainElement() { }
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

public sealed record SelectManyChainElement(SourceType SourceType) : ChainElement {
}
