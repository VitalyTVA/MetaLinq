using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
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

            (bool toArray, bool iEnumerable)? ArrayWhereInfo = default;
            (bool toArray, bool iEnumerable)? ListWhereInfo = default;

            foreach(var (sourceType, tree) in receiver.Model.GetTrees()) {
                var where = (WhereNode)tree.GetNodes().Single();
                var terminals = where.GetNodes().Cast<TerminalNode>();

                bool toArray1 = terminals.SingleOrDefault(x => x.Type == TerminalNodeType.ToArray) != null;
                bool iEnumerable1 = terminals.SingleOrDefault(x => x.Type == TerminalNodeType.Enumerable) != null;

                if(sourceType == SourceType.Array)
                    ArrayWhereInfo = (toArray1, iEnumerable1);
                else if(sourceType == SourceType.List)
                    ListWhereInfo = (toArray1, iEnumerable1);
                else
                    throw new InvalidOperationException();
            }

            Compilation compilation = context.Compilation;

            StringBuilder source = new();
            CodeBuilder builder = new(source);

            builder.AppendMultipleLines(
@"using System;
using System.Collections;
using System.Collections.Generic;
using System.Buffers;
namespace MetaLinq {
    static class MetaEnumerable {"
            );

            if(ArrayWhereInfo != null) { 
                builder.AppendMultipleLines(
@"        public static ArrayWhereEnumerable<TSource> Where<TSource>(this TSource[] source, Func<TSource, bool> predicate)
            => new ArrayWhereEnumerable<TSource>(source, predicate);"
                );                
            }
            if(ListWhereInfo != null) { 
                builder.AppendMultipleLines(
@"        public static ListWhereEnumerable<TSource> Where<TSource>(this List<TSource> source, Func<TSource, bool> predicate)
            => new ListWhereEnumerable<TSource>(source, predicate);"
                );                
            }


            builder.AppendLine("   }");
            if(ArrayWhereInfo is (bool toArray, bool iEnumerable)) {
                builder.AppendMultipleLines(
    $@"
    struct ArrayWhereEnumerable<T> {(iEnumerable ? ": IEnumerable<T>" : null)} {{
        public readonly T[] source;
        public readonly Func<T, bool> predicate;
        public ArrayWhereEnumerable(T[] source, Func<T, bool> predicate) {{
            this.source = source;
            this.predicate = predicate;
        }}");
                if(toArray)
                    builder.AppendMultipleLines(
@"        public T[] ToArray() {
            using var result = new LargeArrayBuilder<T>(ArrayPool<T>.Shared, false);
            var len = source.Length;
            for(int i = 0; i < len; i++) {
                var item = source[i];
                if(predicate(item)) {
                    result.Add(item);
                }
            }
            return result.ToArray();
        }");
                if(iEnumerable)
                    builder.AppendMultipleLines(
@"        public struct Enumerator {
            ArrayWhereEnumerable<T> source;
            int index;
            public Enumerator(ArrayWhereEnumerable<T> source) {
                this.source = source;
                index = -1;
            }
            public T Current => source.source[index];
            public bool MoveNext() {
                var len = source.source.Length;
                while(true) {
                    index++;
                    if(index >= len)
                        break;
                    if(source.predicate(source.source[index]))
                        return true;
                }
                return false;
            }
        }
        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            var len = source.Length;
            for(int i = 0; i < len; i++) {
                var item = source[i];
                if(predicate(item)) {
                    yield return item;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }
    ");

                builder.AppendLine("}");
            }
            if(ListWhereInfo is (bool toArray_, bool iEnumerable_)) {
                builder.AppendMultipleLines(
    $@"
    struct ListWhereEnumerable<T> {(iEnumerable_ ? ": IEnumerable<T>" : null)} {{
        public readonly List<T> source;
        public readonly Func<T, bool> predicate;
        public ListWhereEnumerable(List<T> source, Func<T, bool> predicate) {{
            this.source = source;
            this.predicate = predicate;
        }}");
                if(toArray_)
                    builder.AppendMultipleLines(
@"        public T[] ToArray() {
            using var result = new LargeArrayBuilder<T>(ArrayPool<T>.Shared, false);
            foreach(var item in source) {
                if(predicate(item)) {
                    result.Add(item);
                }
            }
            return result.ToArray();
        }");
                if(iEnumerable_)
                    builder.AppendMultipleLines(
@"        public struct Enumerator {
            ListWhereEnumerable<T> source;
            int index;
            public Enumerator(ListWhereEnumerable<T> source) {
                this.source = source;
                index = -1;
            }
            public T Current => source.source[index];
            public bool MoveNext() {
                var len = source.source.Count;
                while(true) {
                    index++;
                    if(index >= len)
                        break;
                    if(source.predicate(source.source[index]))
                        return true;
                }
                return false;
            }
        }
        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            var len = source.Count;
            for(int i = 0; i < len; i++) {
                var item = source[i];
                if(predicate(item)) {
                    yield return item;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }
    ");

                builder.AppendLine("}");
            }
            builder.AppendLine("}");
            context.AddSource("MetaLinq.cs", SourceText.From(source.ToString(), Encoding.UTF8));

            //foreach(ClassDeclarationSyntax classSyntax in receiver.ClassSyntaxes) {
            //    if(context.CancellationToken.IsCancellationRequested)
            //        break;
            //}
        }
    }
}
