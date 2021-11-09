kill all MetaEnumerable_Generated stuff

SmallArray no alloc helper to pass several parameters to method

diagnostics error if no reference to MetaLinq.dll (INamedTypeSymbol for MetaLinq.Enumerable not found in SyntaxContextReceiver)

rewrite and unit-test LargeArrayBuilder

rewrite and unit-test CodeBuilder

implement efficient ToArray/ToList direct implementation: "Data[] __() => Data.Array(10).ToArray();"

rewrite array where enumerator implementation like new List<T>.Enumerator

rewrite list where enumerator implementation based on List<T>.GetEnumerator public no alloc method instead of for(int i = 0; i < len; i++)

get rid of record IsExternalInit;

Dispose/Reset/IEnumerator.Current implementation in struct enumerator (before/after enumeration, after dispose)

unit-test Current implementation in struct enumerator

remove #nullable disable/#nullable restore from generation

make editor.config

make tests internals visible to assembly and mark all classes in generato private

~~mark generated structs readonly~~

~~nullable annotations in all projects~~

~~warnings as errors in all projects~~

~~no alloc GetEnumerator to use in foreach (duck-typing)~~