using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using NUnit.Framework;
using Outracks.IPC;

namespace Outracks.UnoHost.Common.Tests
{
	[TestFixture]
	public class BinaryMessageStreamsTest
	{
		[Test]
		public void DataAndTypeAreReadWriteInvariant()
		{
			var pipe = PipeName.New();

			var outbox = new [] 
			{ 
				BinaryMessage.Compose("Elm1", writer => { }),
				BinaryMessage.Compose("Elm1", writer => { }),
				BinaryMessage.Compose("Elm2", writer => writer.Write(new byte[] { 13, 37 })),
				BinaryMessage.Compose("Elm3", writer => writer.Write(new byte[] { 19, 11 })),
				BinaryMessage.Compose("Elm4", writer => { })
			};

			pipe.BeginWritingMessages("Test", ex => Assert.Fail("Write failed: " + ex.Message), outbox.ToObservable());

			var inbox = pipe.ReadMessages("test").RefCount().ToEnumerable().Take(outbox.Length).ToArray();

			Assert.AreEqual(outbox.Length, inbox.Length);

			for (int i = 0; i < outbox.Length; i++)
			{
				Assert.AreEqual(outbox[i].Type, inbox[i].Type);
				CollectionAssert.AreEqual(outbox[i].DumpBytes(), inbox[i].DumpBytes());
			}
		}
		
	}

	static class Helpers
	{
		public static byte[] DumpBytes(this IBinaryMessage message)
		{
			using (var buffer = new MemoryStream())
			using (var writer = new BinaryWriter(buffer))
			{

				message.WriteDataTo(writer);
				return buffer.ToArray();
			}
		}
	}
}
