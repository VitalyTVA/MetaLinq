using JetBrains.dotMemoryUnit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using En = System.Linq.Enumerable;

namespace MetaLinq.Tests.Memory {
    [TestFixture]
    public class Tests {
        [Test]
        [DotMemoryUnit(CollectAllocations = true)]
        public void Test() {
            int[] data = data = En.ToArray(En.Range(0, 5));

            var memoryCheckPoint1 = dotMemory.Check();
            var res = data.Select(static x => x * 10).Where(static x => x % 100 == 0).ToList();

            var memoryCheckPoint2 = dotMemory.Check(memory => {
                var t = memory.GetTrafficFrom(memoryCheckPoint1);
                //Assert.That(memory.GetTrafficFrom(memoryCheckPoint1).Where(obj => obj.Interface.Is<IFoo>()).AllocatedMemory.SizeInBytes,
                //    Is.LessThan(1000));
            });
        }
    }
}
