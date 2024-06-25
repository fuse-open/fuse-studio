using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Outracks.Diagnostics;
using Outracks.IPC;

namespace Outracks.Tests.Pipes
{
	public class UnixSocketStreamTests
	{
		private PipeName _pipeName;

		[SetUp]
		public void SetUp()
		{
			_pipeName = new PipeName(Guid.NewGuid());
		}

		[TearDown]
		public void TearDown()
		{
			if (!Platform.IsMac)
			{
				return;
			}
			File.Delete(Path.Combine(UnixSocketStream.SocketDirectory, _pipeName.ToString()));
		}

		[Test]
		[RestrictTo(OS.Mac)]
		public void SettingUpServerThenClientWorks()
		{
			var host = Task.Run(() => { new UnixSocketStream(_pipeName, SocketUsage.Host); });
			var client = Task.Delay(TimeSpan.FromSeconds(1))
				.ContinueWith(_ => { new UnixSocketStream(_pipeName, SocketUsage.Client); });

			Task.WaitAll(
				AwaitOrTimeOut(TimeSpan.FromSeconds(3), host, "host"),
				AwaitOrTimeOut(TimeSpan.FromSeconds(3), client, "client"));
		}

		[Test]
		[RestrictTo(OS.Mac)]
		public void SettingUpClientThenServerWorks()
		{
			var client = Task.Run(() => { new UnixSocketStream(_pipeName, SocketUsage.Client); });
			var host = Task.Delay(TimeSpan.FromSeconds(1))
				.ContinueWith(_ => new UnixSocketStream(_pipeName, SocketUsage.Host));

			Task.WaitAll(
				AwaitOrTimeOut(TimeSpan.FromSeconds(3), host, "host"),
				AwaitOrTimeOut(TimeSpan.FromSeconds(3), client, "client"));
		}

		[Test]
		[Timeout(10000)]
		[RestrictTo(OS.Mac)]
		public async Task CanCommunicate()
		{
			var message = new byte[] {0x01, 0x02, 0x03};
			var receive = Task.Run(() =>
			{
				var host = new UnixSocketStream(_pipeName, SocketUsage.Host);
				ReadFromHostAndAssertMessageEquals(host, message);
			});
			var sender = new UnixSocketStream(_pipeName, SocketUsage.Client);
			sender.Write(message, 0, message.Length);
			await AwaitOrTimeOut(TimeSpan.FromSeconds(3), receive, "receive");
		}

		[Test]
		[RestrictTo(OS.Mac)]
		//Caracterization test written after the fact, assuming this is the correct behavior
		public void SettingUpClientWithoutServerBlocks()
		{
			var returned = false;
			Task.Run(() =>
			{
				new UnixSocketStream(_pipeName, SocketUsage.Client);
				returned = true;
			});
			Thread.Sleep(TimeSpan.FromSeconds(1));
			if (returned)
			{
				Assert.Fail("Sender didn't block waiting for server");
			}
		}

		[Test]
		[Timeout(10000)]
		[RestrictTo(OS.Mac)]
		public async Task ClosingClientMakesHostReadZeroBytes()
		{
			var message = new byte[] {0x01, 0x02, 0x03};
			var hostTask = Task.Run(() => { return new UnixSocketStream(_pipeName, SocketUsage.Host); });
			var client = new UnixSocketStream(_pipeName, SocketUsage.Client);
			var host = await hostTask;
			var readTask = Task.Run(() => { ReadFromHostAndAssertMessageEquals(host, message); });
			client.Write(message, 0, message.Length);
			await readTask;
			client.Close();
			var buffer = new byte[1];
			Assert.AreEqual(0, host.Read(buffer, 0, 1));
		}

		[Test]
		[Timeout(10000)]
		[RestrictTo(OS.Mac)]
		public async Task ClosingHostMakesClientWritingThrow()
		{
			var message = new byte[] {0x01, 0x02, 0x03};
			var hostTask = Task.Run(() => { return new UnixSocketStream(_pipeName, SocketUsage.Host); });
			var client = new UnixSocketStream(_pipeName, SocketUsage.Client);
			var host = await hostTask;
			var readTask = Task.Run(() => { ReadFromHostAndAssertMessageEquals(host, message); });
			client.Write(message, 0, message.Length);
			await readTask;
			host.Close();
			var buffer = new byte[1];
			Assert.Throws<IOException>(() => client.Write(buffer, 0, 1));
		}

		[Test]
		[Timeout(10000)]
		[RestrictTo(OS.Mac)]
		//Characterization test
		public void CrashingClientMakesHostReadZeroBytes()
		{
			using (var p = RunTestProcess("CrashingClientMakesHostReadZeroBytes " + _pipeName))
			{
				var message = new byte[] {0x01};
				var host = new UnixSocketStream(_pipeName, SocketUsage.Host);
				ReadFromHostAndAssertMessageEquals(host, message);
				p.Kill();
				var buffer = new byte[1];
				var count = host.Read(buffer, 0, 1);
				Assert.AreEqual(0, count);
			}
		}

		[Test]
		[Timeout(10000)]
		[RestrictTo(OS.Mac)]
		//Characterization test
		public void CrashingHostMakesClientWritingThrow()
		{
			using (var p = RunTestProcess("CrashingHostMakesClientWritingThrow " + _pipeName))
			{
				var client = new UnixSocketStream(_pipeName, SocketUsage.Client);
				var message = new byte[] {0x01};
				client.Write(message, 0, 1);
				p.Kill();
				p.WaitForExit();
				Assert.Throws<IOException>(() => client.Write(message, 0, 1));
			}
		}

		static Process RunTestProcess(string argument)
		{
			return Helpers.RunTestProcess(argument, Assembly.GetExecutingAssembly());

		}

		[Test]
		[RestrictTo(OS.Mac)]
		[Timeout(10000)]
		public async Task HasCorrectCapabilities()
		{
            var hostTask = Task.Run(() => { return new UnixSocketStream(_pipeName, SocketUsage.Host); });
			var client =  new UnixSocketStream(_pipeName, SocketUsage.Client);
			var host = await hostTask;

            Assert.IsTrue(host.CanWrite);
            Assert.IsTrue(client.CanWrite);

            Assert.IsTrue(host.CanRead);
            Assert.IsTrue(client.CanRead);

            Assert.IsFalse(host.CanSeek);
            Assert.IsFalse(client.CanSeek);

            Assert.Throws<NotSupportedException>(() =>
            {
                var l = host.Length;
            });

            Assert.Throws<NotSupportedException>(() =>
            {
                var l = client.Length;
            });

            Assert.Throws<NotSupportedException>(() =>
            {
                var p = host.Position;
            });

            Assert.Throws<NotSupportedException>(() =>
            {
                var p = client.Position;
            });

            Assert.Throws<NotSupportedException>(() =>
            {
                host.Position = 1;
            });

            Assert.Throws<NotSupportedException>(() =>
            {
                client.Position = 1;
            });

            Assert.Throws<NotSupportedException>(() =>
            {
                host.Seek(1, SeekOrigin.Begin);
            });

            Assert.Throws<NotSupportedException>(() =>
            {
                host.Seek(1, SeekOrigin.Begin);
            });

		}

		private static void ReadFromHostAndAssertMessageEquals(Stream host, byte[] message)
		{
			var count = message.Length;
			var buffer = new byte[count];
			var offset = 0;
			while (offset < count)
			{
				offset += host.Read(buffer, offset, count);
			}
			Assert.AreEqual(message, buffer);
		}


		public async Task AwaitOrTimeOut(TimeSpan timeOut, Task task, string tag)
		{
			if (await Task.WhenAny(task, Task.Delay(timeOut)) != task)
			{
				Assert.Fail("Timed out after " + timeOut + " waiting for task '" + tag + "'");
			}
		}
	}
}