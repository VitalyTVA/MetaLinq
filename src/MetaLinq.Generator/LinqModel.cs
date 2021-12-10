﻿namespace MetaLinq.Generator;

public class LinqModel {
    readonly Dictionary<SourceType, RootNode> trees = new();

    public void AddChain(SourceType source, IEnumerable<ChainElement> chain) {
        IntermediateNode? node = trees.GetOrAdd(source, () => new RootNode());
        foreach(var item in chain) {
            if(node == null)
                throw new InvalidOperationException();
            node = node.AddElement(item);
        }
        if(node != null)
            node.AddEnumerableNode();
    }
    public IEnumerable<(SourceType, RootNode)> GetTrees() {
        return trees.OrderBy(x => x.Key).Select(x => (x.Key, x.Value));
    }
    public sealed override string ToString() {
        var sb = new StringBuilder();
        var code = new CodeBuilder(sb);
        foreach(var (source, node) in GetTrees()) {
            code.AppendLine(source.ToString());
            code.Tab.AppendMultipleLines(node.ToString());
        }
        var result = sb.ToString();
        if(result.EndsWith(Environment.NewLine)) {
            result = result.Substring(0, result.LastIndexOf(Environment.NewLine));
        }
        return result;
    }
}

public abstract class LinqNode {
}

public enum TerminalNodeType { 
    ToArray, 
    ToList, 
    ToHashSet, 
    ToDictionary, 
    First,
    FirstOrDefault,
    Enumerable 
}

public sealed class TerminalNode : LinqNode {
    public static readonly TerminalNode ToArray = new(TerminalNodeType.ToArray);
    public static readonly TerminalNode ToList = new (TerminalNodeType.ToList);
    public static readonly TerminalNode ToHashSet = new (TerminalNodeType.ToHashSet);
    public static readonly TerminalNode ToDictionary = new (TerminalNodeType.ToDictionary);
    public static readonly TerminalNode First = new (TerminalNodeType.First);
    public static readonly TerminalNode FirstOrDefault = new(TerminalNodeType.FirstOrDefault);
    public static readonly TerminalNode Enumerable = new(TerminalNodeType.Enumerable);
    public readonly TerminalNodeType Type;
    TerminalNode(TerminalNodeType type) {
        Type = type;
    }
    public override string ToString() {
        return $"-{Type}";
    }
}

public abstract class IntermediateNode : LinqNode {
    readonly Dictionary<ChainElement, LinqNode> Nodes = new(/*Extensions.CreatequalityComparer<ChainElement>(x => x.GetHashCode(), (x1, x2) => {
        var typeComparison = EqualityComparer<Type>.Default.Equals(x1.GetType(), x2.GetType());
        if(!typeComparison) 
            return typeComparison;
        return EqualityComparer<string>.Default.Equals(x1.ToString(), x2.ToString());
    })*/);

    public IntermediateNode? AddElement(ChainElement element) {
        IntermediateNode? Add<T>(Func<T> create) where T : LinqNode 
            => Nodes.GetOrAdd(element, create) as IntermediateNode;
        switch(element) {
            case WhereChainElement:
                return Add(static () => new WhereNode());
            case TakeWhileChainElement:
                return Add(static () => new TakeWhileNode());
            case SkipWhileChainElement:
                return Add(static () => new SkipWhileNode());
            case SelectChainElement:
                return Add(static () => new SelectNode());
            case OrderByChainElement:
                return Add(static () => new OrderByNode());
            case OrderByDescendingChainElement:
                return Add(static () => new OrderByDescendingNode());
            case ThenByChainElement:
                return Add(static () => new ThenByNode());
            case ThenByDescendingChainElement:
                return Add(static () => new ThenByDescendingNode());
            case SelectManyChainElement selectManyNode:
                return Add(() => new SelectManyNode(selectManyNode.SourceType));
            case ToValueChainElement { Type: ToValueChainElementType.ToArray }:
                return Add(static () => TerminalNode.ToArray);
            case ToValueChainElement { Type: ToValueChainElementType.ToList }:
                return Add(static () => TerminalNode.ToList);
            case ToValueChainElement { Type: ToValueChainElementType.ToHashSet }:
                return Add(static () => TerminalNode.ToHashSet);
            case ToValueChainElement { Type: ToValueChainElementType.ToDictionary }:
                return Add(static () => TerminalNode.ToDictionary);
            case ToValueChainElement { Type: ToValueChainElementType.First }:
                return Add(static () => TerminalNode.First);
            case ToValueChainElement { Type: ToValueChainElementType.FirstOrDefault }:
                return Add(static () => TerminalNode.FirstOrDefault);
            default:
                throw new InvalidOperationException();
        }
    }
    public void AddEnumerableNode() {
        Nodes.GetOrAdd(ChainElement.Enumerable, static () => TerminalNode.Enumerable);
    }
    public IEnumerable<LinqNode> GetNodes() {
        IEnumerable<KeyValuePair<ChainElement, LinqNode>> pairs = Nodes;
        if(Nodes.ContainsKey(ChainElement.ToList) && !Nodes.ContainsKey(ChainElement.ToArray))
            pairs = pairs.Concat(new[] { new KeyValuePair<ChainElement, LinqNode>(ChainElement.ToArray, TerminalNode.ToArray) });
        var nodes = pairs
            .OrderBy(x => x.Key, ChainElement.Comparer)
            .Select(x => x.Value);
        return nodes;
    }
    public sealed override string ToString() {
        var sb = new StringBuilder();
        var code = new CodeBuilder(sb);
        code.AppendLine(Type);
        foreach(var item in GetNodes()) {
            code.Tab.AppendMultipleLines(item.ToString());
        }
        return sb.ToString();
    }
    protected internal abstract string Type { get; }
}

public sealed class RootNode : IntermediateNode {
    public RootNode() { }
    protected internal override string Type => "Root";
}

public sealed class WhereNode : IntermediateNode {
    public WhereNode() { }
    protected internal override string Type => "Where";
}

public sealed class TakeWhileNode : IntermediateNode {
    public TakeWhileNode() { }
    protected internal override string Type => "TakeWhile";
}

public sealed class SkipWhileNode : IntermediateNode {
    public SkipWhileNode() { }
    protected internal override string Type => "SkipWhile";
}

public sealed class SelectNode : IntermediateNode {
    public SelectNode() { }
    protected internal override string Type => "Select";
}
public sealed class OrderByNode : IntermediateNode {
    public OrderByNode() { }
    protected internal override string Type => "OrderBy";
}
public sealed class OrderByDescendingNode : IntermediateNode {
    public OrderByDescendingNode() { }
    protected internal override string Type => "OrderByDescending";
}
public sealed class ThenByNode : IntermediateNode {
    public ThenByNode() { }
    protected internal override string Type => "ThenBy";
}
public sealed class ThenByDescendingNode : IntermediateNode {
    public ThenByDescendingNode() { }
    protected internal override string Type => "ThenByDescending";
}

public sealed class SelectManyNode : IntermediateNode {
    public readonly SourceType SourceType;
    public SelectManyNode(SourceType sourceType) {
        SourceType = sourceType;
    }
    protected internal override string Type => "SelectMany " + SourceType;
}
