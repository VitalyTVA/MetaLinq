using MetaLinq.Generator;
using MetaLinq.Tests;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MetaLinqTests.Unit {
    [TestFixture]
    public class GenerationTests {
        [Test]
        public void Test() {
            var source =
@"
using MetaLinq;
using MetaLinq.Tests;
public static class Executer {
public static Data[] Execute() => Data.Array(10).Where(x => x.Int < 5).ToArray();
}
";
            string generatedCode = GenerateCode(source);
        }
        static string GenerateCode(string source) {
            var refLocation = Path.GetDirectoryName(typeof(object).Assembly.Location);
            var references = new[] {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                //MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(refLocation, "netstandard.dll")),
                MetadataReference.CreateFromFile(Path.Combine(refLocation, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(refLocation, "System.Buffers.dll")),
                MetadataReference.CreateFromFile(typeof(Data).Assembly.Location),
                //MetadataReference.CreateFromFile(typeof(System.Buffers.ArrayPool<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(MetaLinq.Enumerable).Assembly.Location),
            };
            Compilation inputCompilation = CSharpCompilation.Create("MyCompilation",
                                                                    new[] { CSharpSyntaxTree.ParseText(source) },
                                                                    references,
                                                                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            MetaLinqGenerator generator = new ();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            var location = Path.Combine(Path.GetDirectoryName(typeof(GenerationTests).Assembly.Location), "Generated");
            if(!Directory.Exists(location))
                Directory.CreateDirectory(location);
            var dllPath = Path.Combine(location, "test.dll");
            var emitResult = outputCompilation.Emit(dllPath);
            CollectionAssert.IsEmpty(emitResult.Diagnostics);
            Assert.True(emitResult.Success);
            var assembly = Assembly.LoadFile(dllPath);
            var result = (Data[])assembly.GetType("Executer").GetMethod("Execute").Invoke(null, null);
            Assert.AreEqual(5, result.Length);
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, result.Select(x => x.Int).ToArray());

            GeneratorDriverRunResult runResult = driver.GetRunResult();
            GeneratorRunResult generatorResult = runResult.Results[0];

            var generatedCode = generatorResult.GeneratedSources[0].SourceText.ToString();
            return generatedCode;
        }
    }
}
