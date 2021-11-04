using MetaLinq.Generator;
using MetaLinq.Tests;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MetaLinqTests.Unit {
    [TestFixture]
    public class GenerationTests {
        [Test]
        public void Array_Where_ToArray() {
            AssertGeneration(
                "Data[] __() => Data.Array(10).Where(x => x.Int < 5).ToArray();",
                (Data[] result) => {
                    CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, result.Select(x => x.Int).ToArray());
                },
                new[] {
                    new MetaLinqMethodInfo("Where", new[] { 
                        new StructMethod("ToArray")
                    }, implementsIEnumerable: false)
                }
            );
        }
        [Test]
        public void Array_Where_StandardToArray() {
            AssertGeneration(
                "Data[] __() => System.Linq.Enumerable.ToArray(Data.Array(10).Where(x => x.Int < 5));",
                (Data[] result) => {
                    CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, result.Select(x => x.Int).ToArray());
                },
                new[] {
                    new MetaLinqMethodInfo("Where", new StructMethod[] {
                        new StructMethod("GetEnumerator")
                    }, implementsIEnumerable: true)
                }
            );
        }
        [Test]
        public void Array_Where_ToArray_And_StandardToArray_AndForeach() {
            AssertGeneration(
                new (string code, Action<Data[]> assert)[] { 
                    (
                        "Data[] __() => Data.Array(10).Where(x => x.Int < 5).ToArray();",
                        result => {
                            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, result.Select(x => x.Int).ToArray());
                        }
                    ),
                    (
                        "Data[] __() => System.Linq.Enumerable.ToArray(Data.Array(10).Where(x => x.Int < 5));",
                        (Data[] result) => {
                            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, result.Select(x => x.Int).ToArray());
                        }
                    ),
                     (
                        "Data[] __()  { List<Data> result = new(); foreach(var item in Data.Array(10).Where(x => x.Int < 5)) result.Add(item); return result.ToArray(); }",
                        (Data[] result) => {
                            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, result.Select(x => x.Int).ToArray());
                        }
                    ),               },
                new[] {
                    new MetaLinqMethodInfo("Where", new[] {
                        new StructMethod("ToArray"),
                        new StructMethod("GetEnumerator")
                    }, implementsIEnumerable: true)
                }
            );
        }
        [Test]
        public void Array() {
            AssertGeneration(
                "Data[] __() => Data.Array(5);",
                (Data[] result) => {
                    CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, result.Select(x => x.Int).ToArray());
                },
                new MetaLinqMethodInfo[0]
            );
        }
        [Test]
        public void Array_Where_ToArray_Standard() {
            AssertGeneration(
                "Data[] __() => Data.Array(10).Where(x => x.Int < 5).ToArray();",
                (Data[] result) => {
                    CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, result.Select(x => x.Int).ToArray());
                },
                new MetaLinqMethodInfo[0],
                addMetaLinqUsing: false,
                addStadardLinqUsing: true
            );
        }
        record StructMethod(string Name);
        sealed class MetaLinqMethodInfo {
            public readonly string Name;
            public readonly StructMethod[] ResultMethods;
            public readonly bool ImplementsIEnumerable;

            public MetaLinqMethodInfo(string name, StructMethod[] resultMethods, bool implementsIEnumerable) {
                Name = name;
                ResultMethods = resultMethods;
                ImplementsIEnumerable = implementsIEnumerable;
            }
            public override bool Equals(object? obj) {
                return obj is MetaLinqMethodInfo info &&
                       Name == info.Name &&
                       ImplementsIEnumerable == info.ImplementsIEnumerable &&
                       StructuralComparisons.StructuralEqualityComparer.Equals(ResultMethods, info.ResultMethods);
            }
            public override string ToString() {
                var methods = string.Join(", ", ResultMethods.Select(x => x.Name));
                return $"Name: {Name}, IEnumerable: {ImplementsIEnumerable}, Methods: {methods}";
            }
            public override int GetHashCode() {
                throw new NotImplementedException();
            }
        }


        static void AssertGeneration<T>(string code, Action<T> assert, MetaLinqMethodInfo[] methods, bool addMetaLinqUsing = true, bool addStadardLinqUsing = true)
            where T : class {
            AssertGeneration(new[] { (code, assert) }, methods, addMetaLinqUsing, addStadardLinqUsing);
        }
        static void AssertGeneration<T>((string code, Action<T> assert)[] cases, MetaLinqMethodInfo[] methods, bool addMetaLinqUsing = true, bool addStadardLinqUsing = true)
            where T : class {
            var refLocation = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
            var references = new[] {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(refLocation, "netstandard.dll")),
                MetadataReference.CreateFromFile(Path.Combine(refLocation, "System.Linq.dll")),
                MetadataReference.CreateFromFile(Path.Combine(refLocation, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(refLocation, "System.Buffers.dll")),
                MetadataReference.CreateFromFile(typeof(Data).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(MetaLinq.MetaEnumerable).Assembly.Location),
            };

            var executeMethodsCode = string.Join(Environment.NewLine, cases.Select((x, i) => "static " + x.code.Replace("__", "Execute" + i)));

            var source =
$@"
{(addMetaLinqUsing ? "using MetaLinq;" : null)}
{(addStadardLinqUsing ? "using System.Linq;" : null)}
using MetaLinq.Tests;
using System.Collections.Generic;
public static class Executor {{
{executeMethodsCode}
}}
";


            Compilation inputCompilation = CSharpCompilation.Create("MyCompilation",
                                                                    new[] { CSharpSyntaxTree.ParseText(source) },
                                                                    references,
                                                                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            MetaLinqGenerator generator = new ();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);
            GeneratorDriverRunResult runResult = driver.GetRunResult();
            GeneratorRunResult generatorResult = runResult.Results[0];
            var generatedCode = generatorResult.GeneratedSources;

            var location = Path.Combine(Path.GetDirectoryName(typeof(GenerationTests).Assembly.Location)!, "Generated");
            if(!Directory.Exists(location))
                Directory.CreateDirectory(location);
            var dllPath = Path.Combine(location, NUnit.Framework.TestContext.CurrentContext.Test.Name + ".dll");
            var emitResult = outputCompilation.Emit(dllPath);
            CollectionAssert.IsEmpty(emitResult.Diagnostics.Where(x => x.Severity != DiagnosticSeverity.Hidden).ToArray());
            Assert.True(emitResult.Success);

            var assembly = Assembly.LoadFile(dllPath);
            var executorType = assembly.GetType("Executor")!;
            var executeMethods = executorType
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(x => x.Name.StartsWith("Execute"))
                .OrderBy(x => x.Name)
                .ToArray();
            for(int i = 0; i < executeMethods.Length; i++) {
                var result = (T)executeMethods[i].Invoke(null, null)!;
                cases[i].assert(result);
            }
            

            var extensionsType = assembly.GetType("MetaLinq.MetaEnumerable")!;

            var allGeneratedTypes = assembly.GetTypes().Where(x => x != extensionsType && x != executorType && !x.IsNested).ToArray();

            var expectedGeneratedTypes = new HashSet<Type>();

            Assert.False(extensionsType.IsPublic);
            var actualMethods = extensionsType
                .GetMethods()
                .Where(x => x.DeclaringType == extensionsType)
                .Select(x => {
                    Assert.False(x.ReturnType.IsPublic);
                    bool implementsIEnumerable = x.ReturnType.GetInterfaces().Where(x => x.Name.Contains("IEnumerable")).Count() == 2;
                    expectedGeneratedTypes.Add(x.ReturnType.GetGenericTypeDefinition());
                    return new MetaLinqMethodInfo(
                        x.Name, 
                        x.ReturnType
                            .GetMethods()
                            .Where(y => y.DeclaringType == x.ReturnType)
                            .Select(y => new StructMethod(y.Name))
                            .ToArray(),
                        implementsIEnumerable: implementsIEnumerable
                    );
                })
                .ToArray();
            CollectionAssert.AreEqual(methods, actualMethods);
            CollectionAssert.AreEquivalent(expectedGeneratedTypes.ToArray(), allGeneratedTypes);
        }
    }
}
