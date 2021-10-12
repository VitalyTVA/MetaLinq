using System;
using MetaLinq;
using System.Collections;
using System.Collections.Generic;

namespace MetaLinq {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            var res = new[] { 1, 2, 3 }.Select(x => x * 10).Where(x => x < 15).ToList();
        }
    }
}
