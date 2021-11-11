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

public enum SourceType { List, Array }

public enum ChainElement { Where, Select, ToArray, ToList }
public abstract class LinqNode {
}

public enum TerminalNodeType { ToArray, ToList, Enumerable }

public sealed class TerminalNode : LinqNode {
    public static readonly TerminalNode ToArray = new TerminalNode(TerminalNodeType.ToArray);
    public static readonly TerminalNode ToList = new TerminalNode(TerminalNodeType.ToList);
    public static readonly TerminalNode Enumerable = new TerminalNode(TerminalNodeType.Enumerable);
    public readonly TerminalNodeType Type;
    TerminalNode(TerminalNodeType type) {
        Type = type;
    }
    public override string ToString() {
        return $"-{Type}";
    }
}

public abstract class IntermediateNode : LinqNode {
    enum NodeType {
        Where,
        Select,
        Terminal,
    }
    readonly Dictionary<(NodeType type, TerminalNodeType? terminal), LinqNode> Nodes = new();
    static (NodeType type, TerminalNodeType? terminal) WhereKey => (NodeType.Where, null);
    static (NodeType type, TerminalNodeType? terminal) SelectKey => (NodeType.Select, null);
    static (NodeType type, TerminalNodeType? terminal) ToArrayKey => (NodeType.Terminal, TerminalNodeType.ToArray);
    static (NodeType type, TerminalNodeType? terminal) ToListKey => (NodeType.Terminal, TerminalNodeType.ToList);
    static (NodeType type, TerminalNodeType? terminal) EnumeralbeKey => (NodeType.Terminal, TerminalNodeType.Enumerable);


    public IntermediateNode? AddElement(ChainElement element) {
        switch(element) {
            case ChainElement.Where:
                return (IntermediateNode)Nodes.GetOrAdd(WhereKey, static () => new WhereNode());
            case ChainElement.Select:
                return (IntermediateNode)Nodes.GetOrAdd(SelectKey, static () => new SelectNode());
            case ChainElement.ToArray:
                Nodes.GetOrAdd(ToArrayKey, static () => TerminalNode.ToArray);
                return null;
            case ChainElement.ToList:
                Nodes.GetOrAdd(ToListKey, static () => TerminalNode.ToList);
                return null;
            default:
                throw new InvalidOperationException();
        }
    }
    public void AddEnumerableNode() {
        Nodes.GetOrAdd(EnumeralbeKey, static () => TerminalNode.Enumerable);
    }
    public IEnumerable<LinqNode> GetNodes() {
        return Nodes
            .OrderBy(x => x.Key.type)
            .ThenBy(x => x.Key.terminal)
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
