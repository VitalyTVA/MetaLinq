namespace MetaLinq.Generator;

public class LinqModel {
    readonly Dictionary<SourceType, RootNode> trees = new();

    public void AddChain(SourceType source, IEnumerable<ChainElement> chain) {
        TreeNode? node = trees.GetOrAdd(source, () => new RootNode());
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

public abstract class TerminalNodeBase : LinqNode {
}

public sealed class TerminalNode : TerminalNodeBase {
    public readonly TerminalChainElement Element;

    public TerminalNode(TerminalChainElement element) {
        Element = element;
    }
    public override string ToString() {
        return Element switch { 
            ToValueChainElement toValueChainElement => "-" + toValueChainElement.Type,
            EnumerableChainElement => "-Enumerable",
            ToListChainElement => "-ToList",
            _ => throw new NotImplementedException(), 
        };
    }
}

public abstract class TreeNode : LinqNode {
    readonly Dictionary<ChainElement, LinqNode> Nodes = new();

    public IntermediateNode? AddElement(ChainElement element) {
        IntermediateNode? Add<T>(Func<T> create) where T : LinqNode 
            => Nodes.GetOrAdd(element, create) as IntermediateNode;
        switch(element) {
            case IntermediateChainElement intermediateChainElement:
                return Add(() => new IntermediateNode(intermediateChainElement));
            case TerminalChainElement terminalElement:
                return Add(() => new TerminalNode(terminalElement));
            default:
                throw new InvalidOperationException();
        }
    }
    public void AddEnumerableNode() {
        Nodes.GetOrAdd(ChainElement.Enumerable, static () => new TerminalNode(ChainElement.Enumerable));
    }
    public IEnumerable<LinqNode> GetNodes() {
        IEnumerable<KeyValuePair<ChainElement, LinqNode>> pairs = Nodes;
        if(Nodes.ContainsKey(ChainElement.ToList) && !Nodes.ContainsKey(ChainElement.ToArray))
            pairs = pairs.Concat(new[] { new KeyValuePair<ChainElement, LinqNode>(ChainElement.ToArray, new TerminalNode(ChainElement.ToArray)) });
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
    protected abstract string Type { get; }
}

public sealed class RootNode : TreeNode {
    public RootNode() { }
    protected override string Type => "Root";
}

public sealed class IntermediateNode : TreeNode {
    public readonly IntermediateChainElement Element;
    public IntermediateNode(IntermediateChainElement element) {
        Element = element;
    }
    protected override string Type  => Element.Type;
}