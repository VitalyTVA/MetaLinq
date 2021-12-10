using System.Diagnostics;

namespace MetaLinq.Internal;

public static class TestTrace {
#if DEBUG
    public static int LargeArrayBuilderCreatedCount { get; private set; }
    public static int LargeArrayBuilderDisposedCount { get; private set; }
    public static int ArrayCreatedCount { get; private set; }
    public static int DictionaryCreatedCount { get; private set; }
    public static int DictionaryWithCapacityCreatedCount { get; private set; }
#endif
    [Conditional("DEBUG")]
    public static void ArrayCreated() {
#if DEBUG
        ArrayCreatedCount++;
#endif
    }
    [Conditional("DEBUG")]
    public static void DictionaryCreated() {
#if DEBUG
        DictionaryCreatedCount++;
#endif
    }
    [Conditional("DEBUG")]
    public static void DictionaryWithCapacityCreated() {
#if DEBUG
        DictionaryWithCapacityCreatedCount++;
#endif
    }
    [Conditional("DEBUG")]
    public static void LargeArrayBuilderCreated() {
#if DEBUG
        LargeArrayBuilderCreatedCount++;
#endif
    }
    [Conditional("DEBUG")]
    public static void LargeArrayBuilderDisposed() {
#if DEBUG
        LargeArrayBuilderDisposedCount++;
#endif
    }
    [Conditional("DEBUG")]
    public static void Clear() {
#if DEBUG
        LargeArrayBuilderCreatedCount = 0;
        LargeArrayBuilderDisposedCount = 0;
        ArrayCreatedCount = 0;
        DictionaryCreatedCount = 0;
        DictionaryWithCapacityCreatedCount = 0;
#endif
    }
    [Conditional("DEBUG")]
    public static void VerifyEmpty() {
#if DEBUG
        Debug.Assert(LargeArrayBuilderCreatedCount == 0);
        Debug.Assert(LargeArrayBuilderDisposedCount == 0);
        Debug.Assert(ArrayCreatedCount == 0);
        Debug.Assert(DictionaryCreatedCount == 0);
        Debug.Assert(DictionaryWithCapacityCreatedCount == 0);
#endif
    }

}
