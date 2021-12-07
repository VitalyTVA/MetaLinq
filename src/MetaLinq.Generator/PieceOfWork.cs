namespace MetaLinq.Generator;

public enum ResultType { ToInstance, OrderBy }
public record PieceOfWork(EmitContext[] Contexts, bool SameSize) {
    public int LastLevel => Contexts.LastOrDefault()?.Level ?? -1; //TODO index
    public int TopLevel => Contexts.FirstOrDefault()?.Level ?? -1; //TODO index
    //public bool SameSize => Contexts.Any() && Contexts.All(x => x.Node is not (WhereNode or SkipWhileNode or TakeWhileNode or SelectManyNode));
    public bool SameType => Contexts.All(x => x.Node is not (SelectNode or SelectManyNode));
    public ResultType ResultType 
        => Contexts.LastOrDefault()?.Node is OrderByNode or OrderByDescendingNode or ThenByNode or ThenByDescendingNode 
        ? ResultType.OrderBy 
        : ResultType.ToInstance; 
    public override string ToString() {
        return $"SameType: {SameType}, SameSize: {SameSize}, ResultType: {ResultType}, Nodes: [{string.Join(", ", Contexts.Select(x => x.Node.Type))}]";
    }
}

public static class PieceOfWorkExtensions {
    public static PieceOfWork[] GetPieces(this EmitContext context, SourceType sourceType) 
        => context.GetPiecesCore(sourceType).ToArray();

    static IEnumerable<PieceOfWork> GetPiecesCore(this EmitContext lastContext, SourceType sourceType) {
        var contexts = lastContext.GetReversedContexts().ToList();
        List<EmitContext> current = new();
        bool sameSize = sourceType.HasCount();
        PieceOfWork CreateAndReset() {
            var result = new PieceOfWork(current.ToArray(), sameSize);
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
                    current.Add(context);
                    break;
                case SelectManyNode:
                    if(IsOrderBy())
                        yield return CreateAndReset();
                    sameSize = false;
                    current.Add(context);
                    break;
                case WhereNode or SkipWhileNode or TakeWhileNode:
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