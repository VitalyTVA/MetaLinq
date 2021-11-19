using MetaLinq.Internal;

namespace MetaLinqTests.Unit;

public class BaseFixture {
    [TearDown]
    public void CrealTrace() {
        TestTrace.Clear();
    }
}