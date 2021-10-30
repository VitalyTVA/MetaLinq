using MetaLinq.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;


namespace MetaLinq.Tests.Unit {
    [TestFixture]
    public class GenerationTests {
        [Test]
        public void Test() {
            var source = @"namespace Test {
            partial class Example {
                public void Method(int arg) { }
            }
        }";
            string generatedCode = GenerateCode(source);
        }
        static string GenerateCode(string source) {
            var references = new[] {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            };
            Compilation inputCompilation = CSharpCompilation.Create("MyCompilation",
                                                                    new[] { CSharpSyntaxTree.ParseText(source) },
                                                                    references,
                                                                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            MetaLinqGenerator generator = new ();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            GeneratorDriverRunResult runResult = driver.GetRunResult();
            GeneratorRunResult generatorResult = runResult.Results[0];

            var generatedCode = generatorResult.GeneratedSources[1].SourceText.ToString();
            return generatedCode;
        }
    }
}
