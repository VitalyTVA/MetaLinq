using JetBrains.dotMemoryUnit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetaLinqTests.Memory {
	public static class MemoryTestHelper {
		static bool initialized;
		static void Init() {
			if(initialized)
				return;
			initialized = true;
			Warmup();
			Warmup();
			WarmupAndCollectGarbageInstances();
		}
		//static readonly object syncRoot = new object();
		public static void Warmup() {
			var memoryCheckPoint = dotMemory.Check();
			var memoryCheckPoint2 = dotMemory.Check(memory => {
				var t = memory.GetTrafficFrom(memoryCheckPoint);
			});
		}
		public static void WarmupAndCollectGarbageInstances() {
			var memoryCheckPoint = dotMemory.Check();
			var memoryCheckPoint2 = dotMemory.Check(memory => {
				var traffic = memory.GetTrafficFrom(memoryCheckPoint);
				GarbageInstances = GetTrafficWithoutGarbageTypes(traffic)
					.ToDictionary(type => type.TypeFullyQualifiedName, type => type.AllocatedMemoryInfo.ObjectsCount);
			});
		}
		public static void AssertDifference(Action action, (string type, int count)[] expected) {
            //lock(syncRoot) {
				Init();
				action();
				var memoryCheckPoint = dotMemory.Check();
				action();
				var memoryCheckPoint2 = dotMemory.Check(memory => {
					var traffic = memory.GetTrafficFrom(memoryCheckPoint);
					var allocated = GetTrafficWithoutGarbageTypes(traffic)
						.Select(type => {
							int objectsCount = type.AllocatedMemoryInfo.ObjectsCount;
							if(GarbageInstances.TryGetValue(type.TypeFullyQualifiedName, out int value))
								objectsCount -= value;
							if(objectsCount < 0)
								throw new InvalidOperationException();
							return (type: type.TypeFullyQualifiedName, count: objectsCount);
						})
						.Where(x => x.count != 0)
						.OrderBy(x => x.type)
						.ToArray();
					expected = expected ?? new (string type, int count)[0];
					foreach(var item in expected) {
						CollectionAssert.DoesNotContain(GarbageTypes, item.type);
						Assert.False(GarbageTypePieces.Any(x => item.type.Contains(x)));
					}
					CollectionAssert.AreEqual(expected.OrderBy(x => x.type).ToArray(), allocated);
				});
			//}
		}

        private static IEnumerable<TypeTrafficInfo> GetTrafficWithoutGarbageTypes(Traffic traffic) {
            return traffic.GroupByType()
                                .Where(type => {
                                    return !GarbageTypePieces.Any(piece => type.TypeFullyQualifiedName.Contains(piece))
                                        && !GarbageTypes.Contains(type.TypeFullyQualifiedName);
                                });
        }

		static Dictionary<string, int> GarbageInstances;

        static readonly string[] GarbageTypes = new[] {
			"System.Threading.ContextCallback",
			"System.Threading.Thread",
			"System.Char[]",
			"System.Collections.Generic.List`1[System.Object]",
			"System.Threading.RegisteredWaitHandleSafe",
			"System.Runtime.Remoting.Messaging.CallContextSecurityData",
			"System.Runtime.Remoting.Messaging.LogicalCallContext",
			"System.Threading.ExecutionContext",
			"System.Diagnostics.ProcessWaitHandle",
            "Microsoft.Win32.SafeHandles.SafeProcessHandle",
            "Microsoft.Win32.SafeHandles.SafeWaitHandle",
            "System.Object",
            "System.Runtime.Remoting.Messaging.Message",
            "System.RuntimeMethodInfoStub",
            "System.Reflection.RuntimeMethodInfo",
            "System.Reflection.RuntimeMethodInfo[]",
			"System.Runtime.Remoting.Messaging.ArgMapper",
			"System.Runtime.Diagnostics.EventTraceActivity",
			"System.ServiceModel.Dispatcher.PrimitiveOperationFormatter+PrimitiveRequestBodyWriter",
			"System.ServiceModel.Channels.ActionHeader+DictionaryActionHeader",
			"System.ServiceModel.Channels.BodyWriterMessage",
			"System.ServiceModel.Channels.MessageHeaders",
			"System.ServiceModel.Channels.MessageHeaders+Header[]",
			"System.Xml.UniqueId",
			"System.Byte[]",
			"System.Object[]",
			"System.Object",
			"System.Collections.DictionaryEntry",
			"System.Threading._ThreadPoolWaitOrTimerCallback",
			"System.Threading.RegisteredWaitHandle",
			"System.Attribute[]",
			"System.ServiceModel.Channels.MessageIDHeader",
			"System.ServiceModel.Channels.MessageProperties",
			"System.ServiceModel.Channels.ReplyToHeader",
			"System.Runtime.Remoting.Messaging.ReturnMessage",
			"System.Runtime.Remoting.Messaging.MRMDictionary",
			"System.Runtime.Remoting.Messaging.MessageDictionaryEnumerator",
			"System.Collections.Generic.List`1+Enumerator[System.ServiceModel.Dispatcher.ClientOperation]",
			"Microsoft.VisualStudio.Diagnostics.ServiceModelSink.CsmNotify+OutBufferHolder",
			"System.ServiceModel.Channels.XmlObjectSerializerHeader",
			"System.Uri",
			"System.String",
			"Microsoft.VisualStudio.Diagnostics.ServiceModelSink.EnabledCorrolationState",
			"System.Xml.XmlDictionaryString",
			"System.Xml.XmlDictionaryString[]",
			"System.Runtime.Serialization.DataContractSerializer",
			"System.Xml.XmlDictionary",
			"System.Collections.Generic.Dictionary`2[System.String,System.Xml.XmlDictionaryString]",
			"System.Collections.Generic.Dictionary`2+Entry[System.String,System.Xml.XmlDictionaryString][]",
			"System.Runtime.Serialization.XmlWriterDelegator",
			"System.Net.SecurityBuffer",
			"System.Net.SecurityBuffer[]",
			"System.Net.SecurityBufferDescriptor",
			"System.Net.SecurityBufferStruct[]",
			"System.Runtime.InteropServices.GCHandle[]",
			"System.ServiceModel.Channels.RelatesToHeader",
			"System.ServiceModel.Channels.MessagePatterns+PatternMessage",
			"System.ServiceModel.Security.SecurityMessageProperty",
			"System.ServiceModel.Channels.ServiceChannelProxy+SingleReturnMessage",
			"System.ServiceModel.Channels.ServiceChannelProxy+SingleReturnMessage+PropertyDictionary",
			"System.Collections.Concurrent.ConcurrentStack`1+Node[System.Object]",
			"System.SByte[]",
			"System.Runtime.CompilerServices.GCHeapHash",
		};
        static readonly string[] GarbageTypePieces = new[] {
            "JetBrains",
			"System.Collections.Hashtable",

		};
    }
}
