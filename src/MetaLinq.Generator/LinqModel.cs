namespace MetaLinq.Generator;

public class LinqModel {
    readonly Dictionary<SourceType, LinqTree> trees = new();

    public void AddChain(SourceType source, IEnumerable<LinqNode> chain) {
        var nonGenericSourceRequired = (chain.First() as IntermediateNode) is OfTypeNode or CastNode;
        if(nonGenericSourceRequired) {
            if(source is SourceType.Array or SourceType.List)
                source = SourceType.IList;
            if(source is SourceType.CustomCollection)
                source = SourceType.ICollection;
        }

        LinqTreeBase? node = trees.GetOrAdd(source, () => new LinqTree());
        foreach(var item in chain) {
            if(node == null)
                throw new InvalidOperationException();
            node = node.AddElement(item);
        }
        if(node != null)
            node.AddEnumerableNode();
    }
    public IEnumerable<(SourceType, LinqTree)> GetTrees() {
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

public abstract class LinqTreeNode {
}

public sealed class LinqTreeTerminalNode : LinqTreeNode {
    public readonly TerminalNode Element;

    public LinqTreeTerminalNode(TerminalNode element) {
        Element = element;
    }
    public override string ToString() {
        return Element switch { 
            ToValueChainElement toValueChainElement => "-" + toValueChainElement.Type,
            EnumerableNode => "-Enumerable",
            ToListNode => "-ToList",
            _ => throw new NotImplementedException(), 
        };
    }
}

public abstract class LinqTreeBase : LinqTreeNode {
    readonly Dictionary<LinqNode, LinqTreeNode> Nodes = new();

    public LinqTreeIntermediateNode? AddElement(LinqNode element) {
        LinqTreeIntermediateNode? Add<T>(Func<T> create) where T : LinqTreeNode 
            => Nodes.GetOrAdd(element, create) as LinqTreeIntermediateNode;
        switch(element) {
            case IntermediateNode intermediateChainElement:
                return Add(() => new LinqTreeIntermediateNode(intermediateChainElement));
            case TerminalNode terminalElement:
                return Add(() => new LinqTreeTerminalNode(terminalElement));
            default:
                throw new InvalidOperationException();
        }
    }
    public void AddEnumerableNode() {
        Nodes.GetOrAdd(LinqNode.Enumerable, static () => new LinqTreeTerminalNode(LinqNode.Enumerable));
    }
    public IEnumerable<LinqTreeNode> GetNodes() {
        IEnumerable<KeyValuePair<LinqNode, LinqTreeNode>> pairs = Nodes;
        if(Nodes.ContainsKey(LinqNode.ToList) && !Nodes.ContainsKey(LinqNode.ToArray))
            pairs = pairs.Concat(new[] { new KeyValuePair<LinqNode, LinqTreeNode>(LinqNode.ToArray, new LinqTreeTerminalNode(LinqNode.ToArray)) });
        var nodes = pairs
            .OrderBy(x => x.Key, LinqNode.Comparer)
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

public sealed class LinqTree : LinqTreeBase {
    public LinqTree() { }
    protected override string Type => "Root";
}

public sealed class LinqTreeIntermediateNode : LinqTreeBase {
    public readonly IntermediateNode Element;
    public LinqTreeIntermediateNode(IntermediateNode element) {
        Element = element;
    }
    protected override string Type  => Element.Type;
}