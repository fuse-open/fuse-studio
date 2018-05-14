using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Outracks.Diagnostics;
using Outracks.IPC;

namespace Outracks.Common.Tests
{
	public class BinaryMessageStreamsTestsUsingPipes
	{
		PipeName _pipeName;


		[SetUp]
		public void SetUp()
		{
			_pipeName = new PipeName(Guid.NewGuid());
		}

		[TearDown]
		public void TearDown()
		{
			if (Platform.OperatingSystem != OS.Mac)
			{
				return;
			}
			File.Delete(Path.Combine(UnixSocketStream.SocketDirectory, _pipeName.ToString()));
		}

		#region ReadMessags

		[Test]
		[Timeout(10000)]
		public async Task CanReadAMessage()
		{
			WriteMessagesToCurrentPipe(IntMessage.Compose(42));
			Assert.AreEqual(42, await _pipeName.ReadMessages("name").RefCount().SelectSome(IntMessage.TryParse).FirstAsync());
		}

		[Test]
		[Timeout(10000)]
		public async Task CompletesAtClosedPipe()
		{
			WriteMessagesToCurrentPipe(IntMessage.Compose(42));
			int result = -1;
			var completed = new TaskCompletionSource<Exception>();
			using (_pipeName.ReadMessages("name").RefCount().SelectSome(IntMessage.TryParse)
				.Subscribe(
					i => result = i,
					e => completed.SetResult(e),
					() => completed.SetResult(null)))
			{
				var exception = await completed.Task;
				Assert.AreEqual(42, result);
				Assert.IsNull(exception);
			}
		}

		[Test]
		[Timeout(10000)]
		//Characterization test, not sure if this is the desired behaviour
		public async Task CompletesWhenClientCrashes()
		{
			var messageToSend = GetMessageAsBase64ForUseInTestProcess(IntMessage.Compose(42)); //Paste this in the testprocess if the format changes
			var result = new TaskCompletionSource<int>();
			var completed = new TaskCompletionSource<Exception>();
			var p = RunTestProcess("CompletesWhenClientCrashes " + _pipeName);
			using (_pipeName.ReadMessages("name").RefCount().SelectSome(IntMessage.TryParse)
				.Subscribe(
					i => result.SetResult(i),
					e => completed.SetResult(e),
					() => completed.SetResult(null)))
			{
				await result.Task;
				p.Kill();
				p.WaitForExit();
				Console.WriteLine();
				var exception = await completed.Task;
				Assert.IsNull(exception);
			}
		}

		[Test]
		[Timeout(10000)]
		//Characterization test
		public async Task ErrorsWhenCannotCreateHost()
		{
			var error = new TaskCompletionSource<Exception>();
			var blockThePipe = Pipe.Host(_pipeName);
			await Task.Run(() => Pipe.Connect(_pipeName).Result.Dispose()); //Make sure we got time to block it before continuing
			_pipeName.ReadMessages("name").RefCount().Subscribe(
				i => { },
				e => { error.SetResult(e); },
				() => { }
			);
			var exception = await error.Task;
			if (Platform.OperatingSystem == OS.Mac)
			{
                Assert.IsInstanceOf<SocketException>(exception);
			}
			else
			{
                Assert.IsInstanceOf<IOException>(exception);
			}
		}

		#endregion

		#region BeginWritingMessages

		//Happy day "CanWriteAMessage" is tested through CanCommunicate()

		[Test]
		[Timeout(10000)]
		[Ignore("Times out randomly on AppVeyor")]
		public async Task WritingNotifiesCallbackWhenHostHasClosed()
		{
			var wroteMessage = new TaskCompletionSource<Unit>();
			var hostClosed = new AutoResetEvent(false);
			var exception = new TaskCompletionSource<Exception>();
			var disposed = false;
			var hostTask = Pipe.Host(_pipeName);
			var task = Task.Run(() =>
			{
				using (_pipeName.BeginWritingMessages("name", e => exception.SetResult(e), Observable.Create<IBinaryMessage>(obs =>
				{
					obs.OnNext(IntMessage.Compose(42));
					wroteMessage.SetResult(Unit.Default);
					hostClosed.WaitOne();
					obs.OnNext(IntMessage.Compose(43));
					return Disposable.Create(() => disposed = true);
				}))){}
			});
			var host = await hostTask;
			await wroteMessage.Task;
			using (var reader = new BinaryReader(host))
			{
				var msg = IntMessage.TryParse(BinaryMessage.ReadFrom(reader));
				Assert.AreEqual(42, msg.Value);
			}
			Assert.IsFalse(exception.Task.IsCompleted);
			host.Close();
			hostClosed.Set();
			await task;
			Assert.IsNotNull(await exception.Task);
			Assert.IsTrue(disposed);
		}

		[Test]
		[Timeout(10000)]
		[Ignore("Times out randomly on Travis")]
		public async Task WritingNotifiesCallbackWhenHostHasCrashed()
		{
			var hostCrashed = new AutoResetEvent(false);
			var exception = new TaskCompletionSource<Exception>();
			var disposed = false;
			var p = RunTestProcess("NotifiesCallbackWhenHostCrashes " + _pipeName);
			Thread.Sleep(1000);
			var task = Task.Run(() =>
			{
				using(_pipeName.BeginWritingMessages("name", e => exception.SetResult(e), Observable.Create<IBinaryMessage>(obs =>
				{
					obs.OnNext(IntMessage.Compose(42));
					hostCrashed.WaitOne();
					obs.OnNext(IntMessage.Compose(43));
					return Disposable.Create(() => disposed = true);
				}))){}
			});
			Thread.Sleep(1000); //Give writing thread time to connect, if we kill the host too early, Connect() will block
			Assert.IsFalse(exception.Task.IsCompleted);
			p.Kill();
			p.WaitForExit();
			hostCrashed.Set();
			await task;
			Assert.IsNotNull(await exception.Task);
			Assert.IsTrue(disposed);
		}

		#endregion

		#region EndToEnd

		[Test]
		[Timeout(10000)]
		public async Task CanCommunicate()
		{
			Exception error = null;
			using (_pipeName.BeginWritingMessages("name", e => { error = e; }, Observable.Return(IntMessage.Compose(42))))
			{
				var msg = await _pipeName.ReadMessages("name").RefCount().SelectSome(IntMessage.TryParse).FirstAsync();
				Assert.AreEqual(42, msg);
				Assert.IsNull(error);
			}
		}

		[Test]
		[Timeout(10000)]
		[Ignore("This is currently not the case, maybe it should be?")] //Just wrote this as an experiment, and left it in in case we want it later
		public async Task ReaderCompletesWhenWriterCompletes()
		{
			Exception writeError = null;
			Exception readError = null;
			var result = -1;
			var completed = new TaskCompletionSource<Unit>();
			using (_pipeName.BeginWritingMessages("name", e => { writeError = e; }, Observable.Return(IntMessage.Compose(42))))
			{
				using (_pipeName.ReadMessages("name").RefCount().SelectSome(IntMessage.TryParse)
					.Subscribe(
						i => result = i,
						e => readError = e,
						() => completed.SetResult(Unit.Default)))
				{
					await completed.Task;
					Assert.AreEqual(42, result);
					Assert.IsNull(writeError);
					Assert.IsNull(readError);
				}
			}
		}

		[Test]
		[Timeout(10000)]
		//Characterization test, see individual comments
		public void WhatHappensWhenWriterErrors()
		{
			var readerGotOnError = new TaskCompletionSource<Exception>();
			var readerGotOnCompleted = new TaskCompletionSource<Unit>();
			var writerGotErrorCallback = new TaskCompletionSource<Exception>();
			var disposed = false;

			Assert.Throws<Exception>( //TODO Not sure I like this behavior
				() =>
				{
					using (_pipeName.BeginWritingMessages(
						"name",
						e => { writerGotErrorCallback.SetResult(e); },
						OneMessageThenError(42, new Exception("fail"), () => disposed = true)))
					{
						using (_pipeName.ReadMessages("name").RefCount().SelectSome(IntMessage.TryParse)
							.Subscribe(
								i => { },
								e => readerGotOnError.SetResult(e),
								() => readerGotOnCompleted.SetResult(Unit.Default)))
						{
							Task.WaitAny(readerGotOnError.Task, readerGotOnCompleted.Task, writerGotErrorCallback.Task);
							//TODO If we decide it shouldn't throw on OnError, here we can assert the desired behaviour
							//I'd prefer it if the reader completed at least.
						}
					}
				});
			Assert.IsFalse(disposed); //TODO I'd prefer this to be true
		}
		#endregion

		static string GetMessageAsBase64ForUseInTestProcess(IBinaryMessage msg)
		{
			using (var m = new MemoryStream())
			using (var w = new BinaryWriter(m))
			{
				msg.WriteTo(w);
				m.Seek(0, SeekOrigin.Begin);
				return Convert.ToBase64String(m.GetBuffer(), 0, (int)m.Length);
			}
		}

		static Process RunTestProcess(string argument)
		{
			return Helpers.RunTestProcess(argument, Assembly.GetExecutingAssembly());

		}

		void WriteMessagesToCurrentPipe(params IBinaryMessage[] messages)
		{
			Task.Run(() =>
			{
				using (var stream = Pipe.Connect(_pipeName).Result)
				using (var writer = new BinaryWriter(stream))
				{
					foreach (var message in messages)
					{
						message.WriteTo(writer);
					}
				}
			});
		}

		IObservable<IBinaryMessage> OneMessageThenError(int i, Exception error, Action dispose)
		{
			return Observable.Create<IBinaryMessage>(obs =>
			{
				obs.OnNext(IntMessage.Compose(i));
				obs.OnError(error);
				return dispose;
			});
		}
	}
}
