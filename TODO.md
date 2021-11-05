diagnostics error if no reference to MetaLinq.dll (INamedTypeSymbol for MetaLinq.Enumerable not found in SyntaxContextReceiver)

rewrite and unit-test LargeArrayBuilder

rewrite and unit-test CodeBuilder

implement efficient ToArray/ToList direct implementation: "Data[] __() => Data.Array(10).ToArray();"

rewrite array where enumerator implementation like new List<T>.Enumerator

rewrite list where enumerator implementation based on List<T>.GetEnumerator public no alloc method

get rid of record IsExternalInit;

~~nullable annotations in all projects~~

~~warnings as errors in all projects~~

~~no alloc GetEnumerator to use in foreach (duck-typing)~~