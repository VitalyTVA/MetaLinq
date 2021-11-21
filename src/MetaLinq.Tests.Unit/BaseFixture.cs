﻿using MetaLinq.Internal;

namespace MetaLinqTests.Unit;

public class BaseFixture {
    [SetUp]
    public virtual void SetUp() {
        TestTrace.VerifyEmpty();
    }
    [TearDown]
    public virtual void TearDown() {
        Assert.AreEqual(TestTrace.LargeArrayBuilderCreatedCount, TestTrace.LargeArrayBuilderDisposedCount);
        TestTrace.Clear();
    }
}