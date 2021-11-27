namespace MetaLinq.Generator;

public enum ResultType { ToArray, OrderBy }
public record PieceOfWork(EmitContext[] Contexts, bool SameType, bool SameSize, ResultType ResultType) {
    public int LastLevel => Contexts.Last().Level;
    public int TopLevel = Contexts.First().Level;
    public override string ToString() {
        return $"SameType: {SameType}, SameSize: {SameSize}, ResultType: {ResultType}, Nodes: [{string.Join(", ", Contexts.Select(x => x.Node.Type))}]";
    }
}

public static class PieceOfWorkExtensions {
    public static PieceOfWork[] GetPieces(this EmitContext context) => context.GetPiecesCore().ToArray();

    static IEnumerable<PieceOfWork> GetPiecesCore(this EmitContext lastContext) {
        var contexts = lastContext.GetReversedContexts().ToList();
        List<EmitContext> current = new();
        bool sameType = true;
        bool sameSize = true;
        for(int i = 0; i < contexts.Count; i++) {
            var context = contexts[i];
            var nextContext = i < contexts.Count - 1 ? contexts[i + 1] : null;
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
                case OrderByNode or OrderByDescendingNode or ThenByNode or ThenByDescendingNode:
                    if(nextContext?.Node is ThenByNode or ThenByDescendingNode) {
                        current.Add(context);
                    } else {
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
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
        if(current.Any())
            yield return new PieceOfWork(current.ToArray(), sameType, sameSize, ResultType.ToArray);
    }
}