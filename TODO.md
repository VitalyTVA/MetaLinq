allocate resulting array at once if count is known (list/array + where)

#Posponed0

#Posponed1

emit SelectMany body loop via for or foreach for List<T>??  for(int i1 = 0; i1 < len1; i1++) { (now there is duplicate code with root loop emit code)

Dispose/Reset/IEnumerator.Current implementation in struct enumerator (before/after enumeration, after dispose)

rewrite list where enumerator implementation based on List<T>.GetEnumerator public no alloc method instead of for(int i = 0; i < len; i++)

tests for incomplete (non-compilable) LINQ statements tests like "ints.Select(static x => )"

remove #nullable disable/#nullable restore from generation

implement efficient ToArray/ToList direct implementation: "Data[] __() => Data.Array(10).ToArray();"

#Posponed2

arguments null checks (source, predicate, selector, etc.)

multitargeting (use bitoperations.log2 when targeting net5; get rid of record IsExternalInit;)

ListLayout struct in different frameworks

diagnostics error if no reference to MetaLinq.dll (INamedTypeSymbol for MetaLinq.Enumerable not found in SyntaxContextReceiver)

#Tests

check state inside array pools after unit-test via reflection (verify all arrays returned)

unit-test Flatten method

rewrite and unit-test LargeArrayBuilder

rewrite and unit-test CodeBuilder

randoms tests - generate chains, source arrays, shuffle data, etc.

#Ideas

linq to rented pools??? source.Where().ToRentedArray(ArrayPool<T>.Shared)

#Housekeeping

make tests internals visible to assembly and mark all classes in generator private

make editor.config

mark generated methods readonly

#OrderBy

OrderBy(x => x) special case for array of primitive types

unit-test sorthelper (doubles with nans, nullables, etc)

order by enumerator generation

#Ideas

SmallArray no alloc helper to pass several parameters to method void ParamsMethod<T, TArray>(TArray params) where TArray : ICollection<T> {}

# Done

~~nested selectmany memory test (array + enumerator)~~

~~mark generated structs readonly~~

~~nullable annotations in all projects~~

~~warnings as errors in all projects~~

~~no alloc GetEnumerator to use in foreach (duck-typing)~~

~~apply all spike tests to generated code~~

~~tests that selector/predicate is always called exactly once for each element (no code like current2 => current1[i1])~~
