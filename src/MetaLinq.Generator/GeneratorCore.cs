using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MetaLinq.Generator {
    static class MetaLinqGeneratorCore {
        public static void Execute(GeneratorExecutionContext context) {
            //Debugger.Launch();
            if(context.SyntaxContextReceiver is not SyntaxContextReceiver receiver)
                return;

            Compilation compilation = context.Compilation;

            StringBuilder source = new();
            CodeBuilder sourceBuilder = new(source);

            context.AddSource("MetaLinq.cs", SourceText.From(
@"using System;
using System.Buffers;
namespace MetaLinq {
    public static class MetaEnumerable {
        public static ArrayWhereEnumerable<TSource> Where<TSource>(this TSource[] source, Func<TSource, bool> predicate)
            => new ArrayWhereEnumerable<TSource>(source, predicate);
    }
    public struct ArrayWhereEnumerable<T> {
        public readonly T[] source;
        public readonly Func<T, bool> predicate;
        public ArrayWhereEnumerable(T[] source, Func<T, bool> predicate) {
            this.source = source;
            this.predicate = predicate;
        }
        public T[] ToArray() {
            using var result = new LargeArrayBuilder<T>(ArrayPool<T>.Shared, false);
            var len = source.Length;
            for(int i = 0; i < len; i++) {
                var item = source[i];
                if(predicate(item)) {
                    result.Add(item);
                }
            }
            return result.ToArray();
        }
    }
}", Encoding.UTF8));

            //foreach(ClassDeclarationSyntax classSyntax in receiver.ClassSyntaxes) {
            //    if(context.CancellationToken.IsCancellationRequested)
            //        break;

            //    string classSource = source.ToString();
            //    source.Clear();
            //    context.AddSource("test.cs", SourceText.From(classSource, Encoding.UTF8));
            //}
        }
    }
}
