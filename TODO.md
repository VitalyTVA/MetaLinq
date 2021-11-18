allocate resulting array at once if count is known (list/array + selector/where)

emit SelectMany body loop via for or foreach for List<T>??  for(int i1 = 0; i1 < len1; i1++) { (now there is duplicate code with root loop emit code)

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

unit-test Flatten method

remove #nullable disable/#nullable restore from generation

make editor.config

make tests internals visible to assembly and mark all classes in generato private

incomplete (non-compilable) LINQ statements tests like "ints.Select(static x => )"

arguments null checks (source, predicate, selector, etc.)

mark methods readonly

ListLayout struct in different frameworks

nested selectmany memory test (array + enumerator)

randoms tests - generate chains, source arrays, shuffle data, etc.

check state inside array pools after unit-test via reflection (verify all arrays returned)

#OrderBy

OrderBy(x => x) special case for array of primitive types

unit-test sorthelper (doubles with nans, nullables, etc)

order by enumerator generation

# Done

~~mark generated structs readonly~~

~~nullable annotations in all projects~~

~~warnings as errors in all projects~~

~~no alloc GetEnumerator to use in foreach (duck-typing)~~

~~apply all spike tests to generated code~~

~~tests that selector/predicate is always called exactly once for each element (no code like current2 => current1[i1])~~
