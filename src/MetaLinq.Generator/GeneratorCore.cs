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
            foreach(ClassDeclarationSyntax classSyntax in receiver.ClassSyntaxes) {
                if(context.CancellationToken.IsCancellationRequested)
                    break;

                string classSource = source.ToString();
                source.Clear();
                context.AddSource("test.cs", SourceText.From(classSource, Encoding.UTF8));
            }
        }
    }
}
