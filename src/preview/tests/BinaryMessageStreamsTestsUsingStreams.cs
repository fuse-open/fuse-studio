using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Outracks.Tests
{
	public class BinaryMessageStreamsTestsUsingStreams
	{
		static int _forciblyClosedConnection = -2146232800;

		#region ReadMessages

		[Test]
		public async Task CanReadAMessage()
		{
			using (var inbox = StreamWithMessages(IntMessage.Compose(42)))
			{
				Assert.AreEqual(42, await inbox.ReadMessages("name").SelectSome(IntMessage.TryParse).FirstAsync());
			}
		}

		[Test]
		public async Task CompletesAtEndOfStream()
		{
			using (var inbox = StreamWithMessages(IntMessage.Compose(42)))
			{
				int result = -1;
				var completed = new TaskCompletionSource<Exception>();
				using (inbox.ReadMessages("name").SelectSome(IntMessage.TryParse)
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
		}

		[Test]
		public async Task CompletesAtClosedConnection()
		{
			using (var messages = StreamWithMessages(IntMessage.Compose(42)))
			using (var closingStream = new ReadThenThrowStream(messages, new IOException("", _forciblyClosedConnection)))
			{
				int result = -1;
				var completed = new TaskCompletionSource<Exception>();
				using (closingStream.ReadMessages("name").SelectSome(IntMessage.TryParse)
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
		}

		[Test]
		public async Task ErrorsAtOtherExceptions()
		{
			using (var messages = StreamWithMessages(IntMessage.Compose(42)))
			using (var closingStream = new ReadThenThrowStream(messages, new Exception("Alderaan destroyed")))
			{
				int result = -1;
				var completed = new TaskCompletionSource<Exception>();
				using (closingStream.ReadMessages("name").SelectSome(IntMessage.TryParse)
					.Subscribe(
						i => result = i,
						e => completed.SetResult(e),
						() => completed.SetResult(null)))
				{
					var exception = await completed.Task;
					Assert.AreEqual(42, result);
					Assert.AreEqual("Alderaan destroyed", exception.Message);
				}
			}
		}
		#endregion

		#region BeginWritingMessages

		[Test]
		[Timeout(10000)]
		public async Task CanWriteAMessage()
		{
			using (var stream = new MemoryStream())
			{
				Exception exception = null;
				var message = IntMessage.Compose(42);
				var notifyingStream = new NotifyingStream(stream, (int)LengthOf(message));
				using (notifyingStream.BeginWritingMessages("name", e => exception = e, Observable.Return(message)))
				{
					await notifyingStream.Task;
					var msg = IntMessage.TryParse(MessageFromStream(stream));
					Assert.AreEqual(42, msg.Value);
					Assert.IsNull(exception);
				}
			}
		}

		[Test]
		[Timeout(10000)]
		public async Task NotifiesOnWriteError()
		{
			var stream = new CantDoAnythingStream();
			var source = new TaskCompletionSource<Unit>();
			using (stream.BeginWritingMessages("name",
				e =>
				{
					source.SetResult(Unit.Default);
				},
				Observable.Return(IntMessage.Compose(42))))
			{
				await source.Task;
			}
		}

		[Test]
		public void UnsubscribesToMessagesWhenDisposed()
		{
			var disposed = false;
			var messages = Observable.Create<IBinaryMessage>(obs =>
			{
				return Disposable.Create(() => { disposed = true; });
			});
			var write = new MemoryStream().BeginWritingMessages("name", _ => { }, messages);
			write.Dispose();
			Assert.IsTrue(disposed);
		}

		#endregion

		long LengthOf(IBinaryMessage message)
		{
			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream))
			{
				message.WriteTo(writer);
				return stream.Length;
			}
		}

		IBinaryMessage MessageFromStream(MemoryStream stream)
		{
			stream.Seek(0, SeekOrigin.Begin);
			using (var reader = new BinaryReader(stream))
			{
				return BinaryMessage.ReadFrom(reader);
			}
		}

		MemoryStream StreamWithMessages(params IBinaryMessage[] messages)
		{
			var memoryStream = new MemoryStream();
			var writer = new BinaryWriter(memoryStream);
			foreach (var message in messages)
			{
				message.WriteTo(writer);
			}
			memoryStream.Seek(0, SeekOrigin.Begin);
			return memoryStream;
		}
	}

	static class IntMessage
	{
		private const string Name = "Int";

		public static IBinaryMessage Compose(int i)
		{
			return BinaryMessage.Compose(Name, writer => { writer.Write(i); });
		}

		public static Optional<int> TryParse(IBinaryMessage message)
		{
			return message.TryParse(Name, reader =>
				reader.ReadInt32());
		}
	}

	public class CantDoAnythingStream : Stream
	{
		public override int Read(byte[] buffer, int offset, int count) { throw new NotImplementedException(); }
		public override void Write(byte[] buffer, int offset, int count) { throw new NotImplementedException(); }
		public override void Flush() { throw new NotImplementedException(); }
		public override long Seek(long offset, SeekOrigin origin) { throw new NotImplementedException(); }
		public override void SetLength(long value) { throw new NotImplementedException(); }
		public override bool CanRead { get { return false; } }
		public override bool CanSeek { get { return false; } }
		public override bool CanWrite { get { return false; } }
		public override long Length { get { throw new NotImplementedException(); } }
		public override long Position { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
	}

	public class NotifyingStream : CantDoAnythingStream
	{
		private readonly int _notifyWhenBytesWritten;
		private readonly TaskCompletionSource<Unit> _completed = new TaskCompletionSource<Unit>();
		private readonly MemoryStream _stream;

		public Task Task { get { return _completed.Task; }}

		public NotifyingStream(MemoryStream stream, int notifyWhenBytesWritten)
		{
			_stream = stream;
			_notifyWhenBytesWritten = notifyWhenBytesWritten;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_stream.Write(buffer, offset, count);
			if (_stream.Length == _notifyWhenBytesWritten)
			{
				_completed.SetResult(Unit.Default);
			}
			else if (_stream.Length > _notifyWhenBytesWritten)
			{
				throw new Exception("More bytes than " + _notifyWhenBytesWritten + " were written, something is weird in your test.");
			}
		}

		public override bool CanWrite { get { return true; } }
	}

	public class ReadThenThrowStream : CantDoAnythingStream
	{
		private readonly MemoryStream _firstReturnThis;
		private readonly Exception _thenThrowThis;

		public ReadThenThrowStream(MemoryStream firstReturnThis, Exception thenThrowThis)
		{
			_firstReturnThis = firstReturnThis;
			_thenThrowThis = thenThrowThis;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (count > _firstReturnThis.Length - _firstReturnThis.Position)
			{
				throw _thenThrowThis;
			}
			return _firstReturnThis.Read(buffer, offset, count);
		}
		public override bool CanRead { get { return true; } }
	}
}