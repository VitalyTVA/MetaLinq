using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MetaLinq.Generator {
    [Generator]
    public class MetaLinqGenerator : ISourceGenerator {
        public void Initialize(GeneratorInitializationContext context) {
            context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
        }

        public void Execute(GeneratorExecutionContext context) {
            MetaLinqGeneratorCore.Execute(context);
        }
    }

    class SyntaxContextReceiver : ISyntaxContextReceiver {
        readonly List<ClassDeclarationSyntax> classSyntaxes = new();
        public IEnumerable<ClassDeclarationSyntax> ClassSyntaxes { get => classSyntaxes; }

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
            if(context.Node is ClassDeclarationSyntax classDeclarationSyntax) {
                classSyntaxes.Add(classDeclarationSyntax);
            }
        }
    }
}
