using System;
using System.Collections.Generic;
using System.Linq;

namespace MetaLinq.Tests {
    public class Data {
        public static Data[] Array(int count) {
            return IEnumerable(count).ToArray();
        }
        public static List<Data> List(int count) {
            return IEnumerable(count).ToList();
        }
        public static IEnumerable<Data> IEnumerable(int count) {
            return Enumerable.Range(0, count).Select(x => new Data { Int = x });
        }

        public int Int { get; set; }
    }
}
