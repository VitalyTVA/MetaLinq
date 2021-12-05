using System.Runtime.CompilerServices;

namespace MetaLinq.Internal;

public static class DictionaryHelper {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Dictionary<TKey, TSource> ArrayToDictionary<TKey, TSource>(TSource[] array, Func<TSource, TKey> keySelector) {
        var len = array.Length;
        var dictionary = new Dictionary<TKey, TSource>(len);
        for(var i = 0; i < len; i++) {
            var item = array[i];
            dictionary.Add(keySelector(item), item);
        }
        return dictionary;
    }
}
