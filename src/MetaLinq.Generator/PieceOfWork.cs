namespace MetaLinq.Generator;

public enum LoopType { Forward, Backward, Sort }
public record PieceOfWork(EmitContext[] Contexts, bool KnownSize, LoopType LoopType) {
    public Level LastLevel => Contexts.LastOrDefault()?.Level ?? Level.MinusOne;
    public Level TopLevel => Contexts.FirstOrDefault()?.Level ?? Level.MinusOne;
    //public bool SameSize => Contexts.Any() && Contexts.All(x => x.Node is not (WhereNode or SkipWhileNode or TakeWhileNode or SelectManyChainElement));
    public bool KnownType => Contexts.All(x => x.Element is not (SelectNode or SelectManyNode or OfTypeNode or CastNode));
    public override string ToString() {
        return $"KnownType: {KnownType}, KnownSize: {KnownSize}, LoopType: {LoopType}, Nodes: [{string.Join(", ", Contexts.Select(x => x.Element.Type))}]";
    }
}

public static class PieceOfWorkExtensions {
    public static bool IsForwardLoop(this LoopType loopType) => loopType != LoopType.Backward;

    public static IReadOnlyList<PieceOfWork> GetPieces(this EmitContext context, SourceType sourceType, ToValueType toValueType) {
        var result = context.GetPiecesCore(sourceType).ToList();
        if(toValueType is ToValueType.First or ToValueType.FirstOrDefault or ToValueType.Last or ToValueType.LastOrDefault or ToValueType.Any
            && !result.First().Contexts.Any()
            /*&& result.Count == 2*/) { //TODO check if == 2 needed
            result.RemoveAt(0);
            result[0] = result[0] with { KnownSize = false };
        }
        if(toValueType is ToValueType.Last or ToValueType.LastOrDefault
            && result.Count == 1
            && sourceType.HasIndexer() 
            && result.Single().LoopType == LoopType.Forward
            && !result.Single().Contexts.Any(x => x.Element is SelectManyNode selectManyNode && !selectManyNode.SourceType.HasIndexer() || x.Element is TakeWhileNode or SkipWhileNode)
        ) {
            result[0] = result[0] with { LoopType =  LoopType.Backward };
        }
        if(toValueType is ToValueType.Any
            && result.Last().LoopType is LoopType.Sort) {
            //if(result[0].Contexts.Length != 1)
            //    throw new InvalidOperationException();
            result[result.Count - 1] = result[result.Count - 1] with {
                LoopType = LoopType.Forward,
                Contexts = result[result.Count - 1].Contexts
                    .Select(x => x.Element is OrderByNode or ThenByNode ? x with { Element = IdentityNode.Instance } : x)
                    .ToArray()
            };
        }
        return result.AsReadOnly();
    }

    static IEnumerable<PieceOfWork> GetPiecesCore(this EmitContext lastContext, SourceType sourceType) {
        var contexts = lastContext.GetReversedContexts().ToList();
        List<EmitContext> current = new();
        bool sameSize = sourceType.HasCount();
        PieceOfWork CreateAndReset() {
            var loopType = current.LastOrDefault()?.Element is OrderByNode or ThenByNode
                ? LoopType.Sort
                : LoopType.Forward;
            var result = new PieceOfWork(current.ToArray(), sameSize, loopType);
            sameSize = true;
            current.Clear();
            return result;
        };
        bool IsOrderBy() 
            => current.LastOrDefault()?.Element is
            OrderByNode or ThenByNode;
        for(int i = 0; i < contexts.Count; i++) {
            var context = contexts[i];
            var nextContext = i < contexts.Count - 1 ? contexts[i + 1] : null;
            switch(context.Element) {
                case SelectNode or CastNode:
                    if(IsOrderBy())
                        yield return CreateAndReset();
                    current.Add(context);
                    break;
                case SelectManyNode or OfTypeNode:
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
                case OrderByNode:
                    if(!sameSize)
                        yield return CreateAndReset();
                    current.Add(context);
                    break;
                case ThenByNode:
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