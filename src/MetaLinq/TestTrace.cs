using System.Diagnostics;

namespace MetaLinq.Internal;

public static class TestTrace {
#if DEBUG
    public static int LargeArrayBuilderCreatedCount { get; private set; }
    public static int LargeArrayBuilderDisposedCount { get; private set; }
#endif
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
#endif
    }
    [Conditional("DEBUG")]
    public static void VerifyEmpty() {
#if DEBUG
        Debug.Assert(LargeArrayBuilderCreatedCount == 0);
        Debug.Assert(LargeArrayBuilderDisposedCount == 0);
#endif
    }

}
