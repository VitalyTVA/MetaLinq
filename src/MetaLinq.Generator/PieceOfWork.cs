namespace MetaLinq.Generator;

public enum ResultType { ToArray, OrderBy }
public record PieceOfWork(EmitContext[] Contexts, bool SameType, bool SameSize) {
    public int LastLevel => Contexts.Last().Level;
    public int TopLevel = Contexts.First().Level;
    public ResultType ResultType 
        => Contexts.Last().Node is OrderByNode or OrderByDescendingNode or ThenByNode or ThenByDescendingNode 
        ? ResultType.OrderBy 
        : ResultType.ToArray; 
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
        PieceOfWork CreateAndReset() {
            var result = new PieceOfWork(current.ToArray(), sameType, sameSize);
            sameType = true;
            sameSize = true;
            current.Clear();
            return result;
        };
        bool IsOrderBy() 
            => current.LastOrDefault()?.Node is 
            OrderByNode or OrderByDescendingNode or ThenByNode or ThenByDescendingNode;
        for(int i = 0; i < contexts.Count; i++) {
            var context = contexts[i];
            var nextContext = i < contexts.Count - 1 ? contexts[i + 1] : null;
            switch(context.Node) {
                case SelectNode:
                    if(IsOrderBy())
                        yield return CreateAndReset();
                    sameType = false;
                    current.Add(context);
                    break;
                case SelectManyNode:
                    if(IsOrderBy())
                        yield return CreateAndReset();
                    sameType = false;
                    sameSize = false;
                    current.Add(context);
                    break;
                case WhereNode:
                    if(IsOrderBy())
                        yield return CreateAndReset();
                    sameSize = false;
                    current.Add(context);
                    break;
                case OrderByNode or OrderByDescendingNode:
                    if(!sameSize)
                        yield return CreateAndReset();
                    current.Add(context);
                    break;
                case ThenByNode or ThenByDescendingNode:
                    current.Add(context);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
        if(current.Any())
            yield return CreateAndReset();
    }
}