namespace MetaLinq.Generator;

public enum ResultType { ToArray, OrderBy }
public record PieceOfWork(EmitContext[] Contexts, bool SameType, bool SameSize, ResultType ResultType) {
    public override string ToString() {
        return $"SameType: {SameType}, SameSize: {SameSize}, ResultType: {ResultType}, Nodes: [{string.Join(", ", Contexts.Select(x => x.Node.Type))}]";
    }
}

public static class PieceOfWorkExtensions {
    public static PieceOfWork[] GetPieces(this EmitContext context) => context.GetPiecesCore().ToArray();

    static IEnumerable<PieceOfWork> GetPiecesCore(this EmitContext lastContext) {
        var contexts = lastContext.GetReversedContexts();
        List<EmitContext> current = new();
        bool sameType = true;
        bool sameSize = true;
        foreach(EmitContext context in contexts) {
            switch(context.Node) {
                case SelectNode:
                    sameType = false;
                    current.Add(context);
                    break;
                case SelectManyNode:
                    sameType = false;
                    sameSize = false;
                    current.Add(context);
                    break;
                case WhereNode:
                    sameSize = false;
                    current.Add(context);
                    break;
                case OrderByNode or OrderByDescendingNode:
                    if(sameSize) {
                        current.Add(context);
                        yield return new PieceOfWork(current.ToArray(), sameType, sameSize, ResultType.OrderBy);
                    } else {
                        yield return new PieceOfWork(current.ToArray(), sameType, sameSize, ResultType.ToArray);
                        yield return new PieceOfWork(new[] { context }, true, true, ResultType.OrderBy);
                    }
                    sameType = true;
                    sameSize = true;
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