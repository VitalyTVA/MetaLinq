using System;
using System.Collections.Generic;
using System.Linq;

namespace MetaLinq.Generator {
    //public sealed class SelectNode : IntermediateNode {
    //}
    public static class Extensions {
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueCreator) {
            TValue value;
            if(!dictionary.TryGetValue(key, out value)) {
                value = valueCreator();
                dictionary.Add(key, value);
            }
            return value;
        }
    }
}
