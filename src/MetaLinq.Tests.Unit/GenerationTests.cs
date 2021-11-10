using MetaLinq.Generator;
using MetaLinq.Tests;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MetaLinqTests.Unit {
    [TestFixture]
    public class GenerationTests {
        #region where
        [Test]
        public void Array_Where_ToArray() {
            AssertGeneration(
                "Data[] __() => Data.Array(10).Where(x => x.Int < 5).ToArray();",
                Get0To4DataAssert(),
                new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                        new StructMethod("ToArray")
                    }, implementsIEnumerable: false)
                }
            );
        }
        [Test]
        public void ArrayVariable_Where_ToArray() {
            AssertGeneration(
                "Data[] __() { var data = Data.Array(10); return data.Where(x => x.Int < 5).ToArray(); }",
                Get0To4DataAssert(),
                new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                        new StructMethod("ToArray")
                    }, implementsIEnumerable: false)
                }
            );
        }
        [Test]
        public void ListField_Where_ToArray() {
            AssertGeneration(
                "Data[] __() => dataField.Where(x => x.Int < 5).ToArray();",
                Get0To4DataAssert(),
                new[] {
                    new MetaLinqMethodInfo(SourceType.List, "Where", new[] {
                        new StructMethod("ToArray")
                    }, implementsIEnumerable: false)
                },
                additionalClassCode: "static List<Data> dataField = Data.List(10);"
            );
        }
        [Test]
        public void ListParameter_Where_ToArray() {
            AssertGeneration(
                "Data[] __() => GetData(Data.List(10));",
                Get0To4DataAssert(),
                new[] {
                    new MetaLinqMethodInfo(SourceType.List, "Where", new[] {
                        new StructMethod("ToArray")
                    }, implementsIEnumerable: false)
                },
                additionalClassCode: "static Data[] GetData(List<Data> list) => list.Where(x => x.Int < 5).ToArray();"
            );
        }
        [Test]
        public void ArrayProperty_Where_ToArray() {
            AssertGeneration(
                "Data[] __() => DataProperty.Where(x => x.Int < 5).ToArray();",
                Get0To4DataAssert(),
                new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                        new StructMethod("ToArray")
                    }, implementsIEnumerable: false)
                },
                additionalClassCode: "static Data[] DataProperty => Data.Array(10);"
            );
        }

        [Test]
        public void List_Where_ToArray() {
            AssertGeneration(
                "Data[] __() => Data.List(10).Where(x => x.Int < 5).ToArray();",
                Get0To4DataAssert(),
                new[] {
                    new MetaLinqMethodInfo(SourceType.List, "Where", new[] {
                        new StructMethod("ToArray")
                    }, implementsIEnumerable: false)
                }
            );
        }
        [Test]
        public void Array_Where_StandardToArray() {
            AssertGeneration(
                "Data[] __() => System.Linq.Enumerable.ToArray(Data.Array(10).Where(x => x.Int < 5));",
                Get0To4DataAssert(),
                new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Where", new StructMethod[] {
                        new StructMethod("GetEnumerator")
                    }, implementsIEnumerable: true)
                }
            );
        }
        [Test]
        public void List_Where_StandardToArray() {
            AssertGeneration(
                "Data[] __() => System.Linq.Enumerable.ToArray(Data.List(10).Where(x => x.Int < 5));",
                Get0To4DataAssert(),
                new[] {
                    new MetaLinqMethodInfo(SourceType.List, "Where", new StructMethod[] {
                        new StructMethod("GetEnumerator")
                    }, implementsIEnumerable: true)
                }
            );
        }
        [Test]
        public void ArrayAndList_Where_ToArray_And_StandardToArray_AndForeach() {
            AssertGeneration(
                new (string code, Action<Data[]> assert)[] {
                    (
                        "Data[] __() => Data.Array(10).Where(x => x.Int < 5).ToArray();",
                        Get0To4DataAssert()
                    ),
                    (
                        "Data[] __() => System.Linq.Enumerable.ToArray(Data.Array(10).Where(x => x.Int < 5));",
                        Get0To4DataAssert()
                    ),
                     (
                        "Data[] __()  { List<Data> result = new(); foreach(var item in Data.Array(10).Where(x => x.Int < 5)) result.Add(item); return result.ToArray(); }",
                        Get0To4DataAssert()
                    ),
                    (
                        "Data[] __() => Data.List(10).Where(x => x.Int < 5).ToArray();",
                        Get0To4DataAssert()
                    ),
                    (
                        "Data[] __() => System.Linq.Enumerable.ToArray(Data.List(10).Where(x => x.Int < 5));",
                        Get0To4DataAssert()
                    ),
                     (
                        "Data[] __()  { List<Data> result = new(); foreach(var item in Data.List(10).Where(x => x.Int < 5)) result.Add(item); return result.ToArray(); }",
                        Get0To4DataAssert()
                    ),
                },
                new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                        new StructMethod("ToArray"),
                        new StructMethod("GetEnumerator")
                    }, implementsIEnumerable: true),
                    new MetaLinqMethodInfo(SourceType.List, "Where", new[] {
                        new StructMethod("ToArray"),
                        new StructMethod("GetEnumerator")
                    }, implementsIEnumerable: true)

                }
            );
        }
        [Test]
        public void Array_Where_ToArray_Standard() {
            AssertGeneration(
                "Data[] __() => Data.Array(10).Where(x => x.Int < 5).ToArray();",
                Get0To4DataAssert(),
                new MetaLinqMethodInfo[0],
                addMetaLinqUsing: false,
                addStadardLinqUsing: true
            );
        }
        [Test]
        public void List_Where_ToArray_Standard() {
            AssertGeneration(
                "Data[] __() => Data.List(10).Where(x => x.Int < 5).ToArray();",
                Get0To4DataAssert(),
                new MetaLinqMethodInfo[0],
                addMetaLinqUsing: false,
                addStadardLinqUsing: true
            );
        }
        #endregion

        #region select
        [Test]
        public void Array_Select_ToArray() {
            AssertGeneration(
                "int[] __() => Data.Array(5).Select(x => x.Int).ToArray();",
                Get0To4IntAssert(),
                new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Select", new[] {
                        new StructMethod("ToArray")
                    }, implementsIEnumerable: false)
                }
            );
        }
        [Test]
        public void List_Select_ToArray() {
            AssertGeneration(
                "int[] __() => Data.List(5).Select(x => x.Int).ToArray();",
                Get0To4IntAssert(),
                new[] {
                    new MetaLinqMethodInfo(SourceType.List, "Select", new[] {
                        new StructMethod("ToArray")
                    }, implementsIEnumerable: false)
                }
            );
        }
        [Test]
        public void Array_Select_StandardToArray() {
            AssertGeneration(
                "int[] __() => System.Linq.Enumerable.ToArray(Data.Array(5).Select(x => x.Int));",
                Get0To4IntAssert(),
                new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Select", new StructMethod[] {
                        new StructMethod("GetEnumerator")
                    }, implementsIEnumerable: true)
                }
            );
        }
        [Test]
        public void List_Select_StandardToArray() {
            AssertGeneration(
                "int[] __() => System.Linq.Enumerable.ToArray(Data.List(5).Select(x => x.Int));",
                Get0To4IntAssert(),
                new[] {
                    new MetaLinqMethodInfo(SourceType.List, "Select", new StructMethod[] {
                        new StructMethod("GetEnumerator")
                    }, implementsIEnumerable: true)
                }
            );
        }
        [Test]
        public void Array_Select_Foreach() {
            AssertGeneration(
                "int[] __()  { List<int> result = new(); foreach(var item in Data.Array(5).Select(x => x.Int)) result.Add(item); return result.ToArray(); }",
                Get0To4IntAssert(),
                new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Select", new StructMethod[] {
                        new StructMethod("GetEnumerator")
                    }, implementsIEnumerable: true)
                }
            );
        }

        [Test]
        public void Array_SelectAndWhere_ToArray() {
            AssertGeneration(
                new (string code, Action<Data[]> assert)[] {
                    (
                        "Data[] __() => Data.Array(5).Select(x => x).ToArray();",
                        Get0To4DataAssert()
                    ),
                    (
                        "Data[] __() => Data.Array(10).Where(x => x.Int < 5).ToArray();",
                        Get0To4DataAssert()
                    )
                },
                new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Where", new[] {
                        new StructMethod("ToArray")
                    }, implementsIEnumerable: false),
                    new MetaLinqMethodInfo(SourceType.Array, "Select", new[] {
                        new StructMethod("ToArray")
                    }, implementsIEnumerable: false),
                }
            );
        }
        #endregion

        #region select and where
        [Test]
        public void Array_Select_Where_ToArray() {
            AssertGeneration(
                "int[] __() => Data.Array(10).Select(x => x.Int).Where(x => x < 5).ToArray();",
                Get0To4IntAssert(),
                new[] {
                    new MetaLinqMethodInfo(SourceType.Array, "Select", new[] {
                        new StructMethod("Where", new[] {
                            new StructMethod("ToArray")
                        })
                    }, implementsIEnumerable: false)
                }
            );
        }
        //List_Select_Where_ToArray
        //Array_Where_Select_ToArray
        //Enumerators
        //Where_Select
        //Long mixed chains with
        //terminal nodes on different levels
        #endregion

        #region skip
        [Test]
        public void Array() {
            AssertGeneration(
                "Data[] __() => Data.Array(5);",
                Get0To4DataAssert(),
                new MetaLinqMethodInfo[0]
            );
        }
        [Test]
        public void EnumerableToArray() {
            AssertGeneration(
                "Data[] __() => Enumerable.ToArray(Data.Array(5));",
                Get0To4DataAssert(),
                new MetaLinqMethodInfo[0]
            );
        }
        #endregion

        static Action<Data[]> Get0To4DataAssert() {
            return (Data[] result) => {
                CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, result.Select(x => x.Int).ToArray());
            };
        }
        static Action<int[]> Get0To4IntAssert() {
            return (int[] result) => {
                CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, result.ToArray());
            };
        }
        class StructMethod {
            public readonly string Name;
            public readonly StructMethod[] ResultMethods;
            public readonly bool ImplementsIEnumerable;
            public StructMethod(string name, StructMethod[]? resultMethods = null, bool implementsIEnumerable = false) {
                Name = name;
                ResultMethods = resultMethods ?? new StructMethod[0];
                ImplementsIEnumerable = implementsIEnumerable;
            }
            public override bool Equals(object? obj) {
                return obj is StructMethod method &&
                       Name == method.Name &&
                       ImplementsIEnumerable == method.ImplementsIEnumerable &&
                       StructuralComparisons.StructuralEqualityComparer.Equals(ResultMethods, method.ResultMethods);
            }
            public override int GetHashCode() {
                throw new NotImplementedException();
            }
            public override string ToString() {
                var methods = string.Join(", ", ResultMethods.Select(x => x.ToString()));
                return $"Name: {Name}, IEnumerable: {ImplementsIEnumerable}, Methods: [ {methods} ]";
            }
        }
        enum SourceType { Array, List }
        sealed class MetaLinqMethodInfo : StructMethod {
            public readonly SourceType SourceType;

            public MetaLinqMethodInfo(SourceType sourceType, string name, StructMethod[] resultMethods, bool implementsIEnumerable)
                : base(name, resultMethods, implementsIEnumerable) {
                SourceType = sourceType;
            }
            public override bool Equals(object? obj) {
                return base.Equals(obj) && obj is MetaLinqMethodInfo info &&
                       SourceType == info.SourceType;
            }
            public override string ToString() {
                return $"SourceType: {SourceType}, {base.ToString()}";
            }
            public override int GetHashCode() {
                throw new NotImplementedException();
            }
        }


        static void AssertGeneration<T>(string code, Action<T> assert, MetaLinqMethodInfo[] methods, bool addMetaLinqUsing = true, bool addStadardLinqUsing = true, string? additionalClassCode = null)
            where T : class {
            AssertGeneration(new[] { (code, assert) }, methods, addMetaLinqUsing, addStadardLinqUsing, additionalClassCode);
        }
        static void AssertGeneration<T>((string code, Action<T> assert)[] cases, MetaLinqMethodInfo[] methods, bool addMetaLinqUsing = true, bool addStadardLinqUsing = true, string? additionalClassCode = null)
            where T : class {
            var refLocation = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
            var references = new[] {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(refLocation, "netstandard.dll")),
                MetadataReference.CreateFromFile(Path.Combine(refLocation, "System.Linq.dll")),
                MetadataReference.CreateFromFile(Path.Combine(refLocation, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(refLocation, "System.Buffers.dll")),
                MetadataReference.CreateFromFile(Path.Combine(refLocation, "System.Collections.dll")),
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
{additionalClassCode}
{executeMethodsCode}
}}
";


            Compilation inputCompilation = CSharpCompilation.Create("MyCompilation",
                                                                    new[] { CSharpSyntaxTree.ParseText(source) },
                                                                    references,
                                                                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
            MetaLinqGenerator generator = new();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);
            GeneratorDriverRunResult runResult = driver.GetRunResult();
            CollectionAssert.IsEmpty(runResult.Diagnostics);
            GeneratorRunResult generatorResult = runResult.Results[0];
            var generatedCode = generatorResult.GeneratedSources.Select(x => x.SourceText.ToString());

            var location = Path.Combine(Path.GetDirectoryName(typeof(GenerationTests).Assembly.Location)!, "Generated");
            if(!Directory.Exists(location))
                Directory.CreateDirectory(location);
            var dllPath = Path.Combine(location, NUnit.Framework.TestContext.CurrentContext.Test.Name + ".dll");
            var emitResult = outputCompilation.Emit(dllPath);
            if(!emitResult.Success) {
                foreach(var code in generatedCode) {
                    var split = code.Split(Environment.NewLine);
                    int line = 1;
                    foreach(var item in split) {
                        Debug.Write(line);
                        Debug.WriteLine(item);
                        line++;
                    }
                }
                foreach(var item in emitResult.Diagnostics) {
                    Debug.WriteLine(item);
                }
            }
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

            AssertGeneratedClasses(methods, assembly, executorType);
        }

        static void AssertGeneratedClasses(MetaLinqMethodInfo[] methods, Assembly assembly, Type executorType) {
            var extensionsType = assembly.GetType("MetaLinq.MetaEnumerable")!;

            if(!methods.Any()) {
                Assert.Null(extensionsType);
                return;
            }

            var expectedGeneratedTypes = new HashSet<Type>();
            Assert.False(extensionsType.IsPublic);
            var allGeneratedTypes = assembly.GetTypes()
                .Where(x => x != extensionsType && x != executorType && !x.IsNested && !typeof(Attribute).IsAssignableFrom(x))
                .SelectMany(x => {
                    var nested = Extensions.Flatten(x.GetNestedTypes(), x => x.GetNestedTypes().Where(x => x.Name != CodeGenerationTraits.EnumeratorTypeName));
                    CollectionAssert.IsNotEmpty(nested);
                    return nested;
                })
                .ToArray();
            var actualMethods = extensionsType
                .GetMethods()
                .Where(x => x.DeclaringType == extensionsType)
                .Select(x => {
                    Assert.False(x.ReturnType.IsPublic);
                    bool implementsIEnumerable = x.ReturnType.GetInterfaces().Where(x => x.Name.Contains("IEnumerable")).Count() == 2;
                    expectedGeneratedTypes.Add(x.ReturnType.GetGenericTypeDefinition());
                    var sourceType = x.GetParameters()[0].ParameterType.Name switch {
                        "TSource[]" => SourceType.Array,
                        "List`1" => SourceType.List,
                        _ => throw new InvalidOperationException()
                    };
                    Assert.AreEqual(CodeGenerationTraits.RootStaticTypePrefix + sourceType.ToString() + "`1", x.ReturnType.DeclaringType!.Name);
                    Assert.NotNull(x.ReturnType.GetCustomAttribute(typeof(IsReadOnlyAttribute)));
                    return new MetaLinqMethodInfo(
                        sourceType,
                        x.Name,
                        x.ReturnType
                            .GetMethods()
                            .Where(y => y.DeclaringType == x.ReturnType)
                            .Select(y => CollectMethods(expectedGeneratedTypes, y))
                            .ToArray(),
                        implementsIEnumerable: implementsIEnumerable
                    );
                })
                .OrderBy(x => x.SourceType)
                .ToArray();
            CollectionAssert.AreEqual(methods, actualMethods);
            CollectionAssert.AreEquivalent(expectedGeneratedTypes.ToArray(), allGeneratedTypes);
        }
        static StructMethod CollectMethods(HashSet<Type> expectedGeneratedTypes, MethodInfo method) {
            //TODO uncomment all here
            bool implementsIEnumerable = false;// x.ReturnType.GetInterfaces().Where(x => x.Name.Contains("IEnumerable")).Count() == 2;
            if(method is { ReturnType: { IsValueType: true, IsNested: true } } && method.ReturnType.Name != CodeGenerationTraits.EnumeratorTypeName) {
                expectedGeneratedTypes.Add(method.ReturnType.IsGenericType ? method.ReturnType.GetGenericTypeDefinition() : method.ReturnType);
                Assert.True(method.ReturnType.IsNestedPublic);
                Assert.NotNull(method.ReturnType.GetCustomAttribute(typeof(IsReadOnlyAttribute)));
                return new StructMethod(
                    method.Name,
                    method.ReturnType
                        .GetMethods()
                        .Where(y => y.DeclaringType == method.ReturnType)
                        .Select(y => CollectMethods(expectedGeneratedTypes, y))
                        .ToArray(),
                    implementsIEnumerable: implementsIEnumerable
                );
            } else {
                return new StructMethod(method.Name);
            }
        }
    }
}
