namespace MetaLinq.Generator;

public enum ResultType { ToValue, OrderBy }
public record PieceOfWork(EmitContext[] Contexts, bool SameSize) {
    public Level LastLevel => Contexts.LastOrDefault()?.Level ?? Level.MinusOne;
    public Level TopLevel => Contexts.FirstOrDefault()?.Level ?? Level.MinusOne;
    //public bool SameSize => Contexts.Any() && Contexts.All(x => x.Node is not (WhereNode or SkipWhileNode or TakeWhileNode or SelectManyChainElement));
    public bool SameType => Contexts.All(x => x.Element is not (SelectChainElement or SelectManyChainElement));
    public ResultType ResultType 
        => Contexts.LastOrDefault()?.Element is OrderByChainElement or ThenByChainElement
        ? ResultType.OrderBy 
        : ResultType.ToValue; 
    public override string ToString() {
        return $"SameType: {SameType}, SameSize: {SameSize}, ResultType: {ResultType}, Nodes: [{string.Join(", ", Contexts.Select(x => x.Element.Type))}]";
    }
}

public static class PieceOfWorkExtensions {
    public static IReadOnlyList<PieceOfWork> GetPieces(this EmitContext context, SourceType sourceType, ToValueType toValueType) {
        var result = context.GetPiecesCore(sourceType).ToList();
        if(toValueType is ToValueType.First or ToValueType.FirstOrDefault
            && !result.First().Contexts.Any()
            /*&& result.Count == 2*/) {
            result.RemoveAt(0);
            result[0] = result[0] with { SameSize = false };
        }
        return result.AsReadOnly();
    }

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
            => current.LastOrDefault()?.Element is
            OrderByChainElement or ThenByChainElement;
        for(int i = 0; i < contexts.Count; i++) {
            var context = contexts[i];
            var nextContext = i < contexts.Count - 1 ? contexts[i + 1] : null;
            switch(context.Element) {
                case SelectChainElement:
                    if(IsOrderBy())
                        yield return CreateAndReset();
                    current.Add(context);
                    break;
                case SelectManyChainElement:
                    if(IsOrderBy())
                        yield return CreateAndReset();
                    sameSize = false;
                    current.Add(context);
                    break;
                case WhereChainElement or SkipWhileChainElement or TakeWhileChainElement:
                    if(IsOrderBy())
                        yield return CreateAndReset();
                    sameSize = false;
                    current.Add(context);
                    break;
                case OrderByChainElement:
                    if(!sameSize)
                        yield return CreateAndReset();
                    current.Add(context);
                    break;
                case ThenByChainElement:
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