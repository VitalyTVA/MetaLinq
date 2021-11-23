namespace MetaLinq.Generator;

public enum ResultType { ToArray, OrderBy }
public record PieceOfWork(IntermediateNode[] Nodes, bool SameType, bool SameSize, ResultType ResultType) {
    public override string ToString() {
        return $"SameType: {SameType}, SameSize: {SameSize}, ResultType: {ResultType}, Nodes: [{string.Join(", ", Nodes.Select(x => x.Type))}]";
    }
}

public static class PieceOfWorkExtensions {
    public static PieceOfWork[] GetPieces(this IEnumerable<IntermediateNode> nodes) => nodes.GetPiecesCore().ToArray();

    static IEnumerable<PieceOfWork> GetPiecesCore(this IEnumerable<IntermediateNode> nodes) {
        List<IntermediateNode> current = new();
        bool sameType = true;
        bool sameSize = true;
        foreach(IntermediateNode node in nodes) {
            switch(node) {
                case SelectNode:
                    sameType = false;
                    current.Add(node);
                    break;
                case SelectManyNode:
                    sameType = false;
                    sameSize = false;
                    current.Add(node);
                    break;
                case WhereNode:
                    sameSize = false;
                    current.Add(node);
                    break;
                case OrderByNode or OrderByDescendingNode:
                    if(sameSize) {
                        current.Add(node);
                        yield return new PieceOfWork(current.ToArray(), sameType, sameSize, ResultType.OrderBy);
                    } else {
                        yield return new PieceOfWork(current.ToArray(), sameType, sameSize, ResultType.ToArray);
                        yield return new PieceOfWork(new[] { node }, true, true, ResultType.OrderBy);
                    }
                    current.Clear();
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
        if(current.Any())
            yield return new PieceOfWork(current.ToArray(), sameType, sameSize, ResultType.ToArray);
    }
}