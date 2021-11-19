using System.Diagnostics;

namespace MetaLinq.Internal;

public static class TestTrace {
#if DEBUG
    public static int LargeArrayBuilderCreatedCount { get; private set; }
#endif
    [Conditional("DEBUG")]
    public static void LargeArrayBuilderCreated() {
        LargeArrayBuilderCreatedCount++;
    }
    [Conditional("DEBUG")]
    public static void Clear() {
        LargeArrayBuilderCreatedCount = 0;
    }
}
