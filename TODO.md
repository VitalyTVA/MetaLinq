TakeWhile inside selectmany loop - break won't work

simplify senseless chains like OrderBy(x => x.Long).ThenBy(x => x.Int).ToArray
 
BuilderType.UnknownSizeOrderBy => - KILL

several orderby levels (keySelector path)

reduce large array usage (work pieces [Trivial -> UnknownSize/UnknownType/UnknownSizaAndType -> Array/OrderByArray] - repeat)

ArrayPool rent/return tests

Add max capacity to large array builder (like in LargeArrayBuilder from net6)

see how large array builder it works without dispose int net6 (dispose on ToArray???)

#Posponed0

#Posponed1

emit SelectMany body loop via for or foreach for List<T>??  for(int i1 = 0; i1 < len1; i1++) { (now there is duplicate code with root loop emit code)

Dispose/Reset/IEnumerator.Current implementation in struct enumerator (before/after enumeration, after dispose)

rewrite list where enumerator implementation based on List<T>.GetEnumerator public no alloc method instead of for(int i = 0; i < len; i++)

tests for incomplete (non-compilable) LINQ statements tests like "ints.Select(static x => )"

remove #nullable disable/#nullable restore from generation

implement efficient ToArray/ToList direct implementation: "Data[] __() => Data.Array(10).ToArray();"

ExactSizeOrderByArrayBuilder.ToArray overload for List<T> (or generate it fo each type with public indexer public T[int index]). This way we can use indexer without virtual calls

# Posponed2

arguments null checks (source, predicate, selector, etc.)

multitargeting (use bitoperations.log2 when targeting net5; get rid of record IsExternalInit;)

ListLayout struct in different frameworks

diagnostics error if no reference to MetaLinq.dll (INamedTypeSymbol for MetaLinq.Enumerable not found in SyntaxContextReceiver)

ArrayPool<TKey>.Shared.Return(sortKeys, clearArray: false); - should clear when T is not a primitive value type 

# Postponed3

Regression query optimizer for large queries


# Tests

check state inside array pools after unit-test via reflection (verify all arrays returned)

unit-test Flatten method

rewrite and unit-test LargeArrayBuilder

rewrite and unit-test CodeBuilder

randoms tests - generate chains, source arrays, shuffle data, etc.

test that sort performed every time we call OrderBy.TArray for the same instance of OrderByEn (keys not cached). Can't write such test now - var s = source.OrderBy(x => x.Int); s.ToArray(); chains not supported

# Ideas

linq to rented pools??? source.Where().ToRentedArray(ArrayPool<T>.Shared)

# Housekeeping

make tests internals visible to assembly and mark all classes in generator private

make editor.config

mark generated methods readonly

use CallerMemberExpression in Assert and Guard

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
