namespace MetaLinq.Generator;

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

public enum TerminalNodeType { ToArray, ToList, Enumerable }

public sealed class TerminalNode : LinqNode {
    public static readonly TerminalNode ToArray = new(TerminalNodeType.ToArray);
    public static readonly TerminalNode ToList = new (TerminalNodeType.ToList);
    public static readonly TerminalNode Enumerable = new (TerminalNodeType.Enumerable);
    public readonly TerminalNodeType Type;
    TerminalNode(TerminalNodeType type) {
        Type = type;
    }
    public override string ToString() {
        return $"-{Type}";
    }
}

public enum NodeType {
    Where,
    Select,
    SelectMany,
    Terminal,
}
public record struct NodeKey(NodeType type, TerminalNodeType? terminal, SourceType? source) : IComparable<NodeKey> {
    public static NodeKey Simple(NodeType type) => new NodeKey(type, null, null);
    public static NodeKey Terminal(TerminalNodeType type) => new NodeKey(NodeType.Terminal, type, null);
    public static NodeKey SelectMany(SourceType type) => new NodeKey(NodeType.SelectMany, null, type);

    public int CompareTo(NodeKey other) {
        int typeResult = Comparer<NodeType>.Default.Compare(type, other.type);
        if(typeResult != 0)
            return typeResult;
        int terminalResult = Extensions.CompareNullable(terminal, other.terminal);
        if(terminalResult != 0)
            return terminalResult;
        int sourceResult = Extensions.CompareNullable(source, other.source);
        if(sourceResult != 0)
            return sourceResult;
        return 0;
    }
}

public abstract class IntermediateNode : LinqNode {
    readonly Dictionary<NodeKey, LinqNode> Nodes = new();

    public IntermediateNode? AddElement(ChainElement element) {
        switch(element) {
            case WhereChainElement:
                return (IntermediateNode)Nodes.GetOrAdd(NodeKey.Simple(NodeType.Where), static () => new WhereNode());
            case SelectChainElement:
                return (IntermediateNode)Nodes.GetOrAdd(NodeKey.Simple(NodeType.Select), static () => new SelectNode());
            case SelectManyChainElement selectManyNode:
                return (IntermediateNode)Nodes.GetOrAdd(NodeKey.SelectMany(selectManyNode.SourceType), () => new SelectManyNode(selectManyNode.SourceType));
            case ToArrayChainElement:
                Nodes.GetOrAdd(NodeKey.Terminal(TerminalNodeType.ToArray) , static () => TerminalNode.ToArray);
                return null;
            case ToListChainElement:
                Nodes.GetOrAdd(NodeKey.Terminal(TerminalNodeType.ToList), static () => TerminalNode.ToList);
                return null;
            default:
                throw new InvalidOperationException();
        }
    }
    public void AddEnumerableNode() {
        Nodes.GetOrAdd(NodeKey.Terminal(TerminalNodeType.Enumerable), static () => TerminalNode.Enumerable);
    }
    public IEnumerable<LinqNode> GetNodes() {
        return Nodes
            .OrderBy(x => x.Key)
            .Select(x => x.Value);
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
    protected abstract string Type { get; }
}

public sealed class RootNode : IntermediateNode {
    public RootNode() { }
    protected override string Type => "Root";
}

public sealed class WhereNode : IntermediateNode {
    public WhereNode() { }
    protected override string Type => "Where";
}
public sealed class SelectNode : IntermediateNode {
    public SelectNode() { }
    protected override string Type => "Select";
}
public sealed class SelectManyNode : IntermediateNode {
    public readonly SourceType SourceType;
    public SelectManyNode(SourceType sourceType) {
        SourceType = sourceType;
    }
    protected override string Type => "SelectMany " + SourceType;
}
