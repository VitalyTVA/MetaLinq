using MetaLinqTests.Memory;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace MetaLinqTests.Memory {
    class Program {
        static void Main(string[] args) {
            //Debugger.Launch();
            var type = typeof(Tests);
            var tests = type
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(x => x.GetCustomAttribute(typeof(TestAttribute)) != null)
                .ToList();
            foreach(var method in tests) {
                Console.WriteLine(method.Name);
                try {
                    method.Invoke(null, null);
                } catch(Exception e) {
                    Console.WriteLine("Error");
                    Console.WriteLine(e.InnerException!.Message);
                }
            }
            Console.WriteLine("Finished");
            //Console.ReadLine();
        }
    }
}
