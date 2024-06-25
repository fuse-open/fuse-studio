using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Outracks.Fuse.Protocol
{
	class ClientInfo
	{
		public readonly TcpClient Socket;
		public readonly NetworkStream Stream;
		public const int BufferSize = 4096;
		public byte[] Buffer = new byte[BufferSize];
		public List<byte> TempCache = new List<byte>();

		public ClientInfo(TcpClient socket)
		{
			Socket = socket;
			Stream = socket.GetStream();
		}
	}

	public class Message
	{
		public readonly string MessageType;
		public readonly string Payload;

		public Message(string messageType, string payload)
		{
			MessageType = messageType;
			Payload = payload;
		}
	}

	public interface IMessageConnection : IDisposable
	{
		IObservable<Message> IncomingMessages { get; }
		IObserver<Message> OutgoingMessages { get; }

		IObservable<DateTime> Disconnected { get; }
		void StartRead();
		void Close();
	}

	public class LocalSocketClient : IMessageConnection
	{
		readonly Subject<Message> _incomingMessages = new Subject<Message>();
		readonly Subject<Message> _outgoingMessages = new Subject<Message>();
		readonly ConcurrentQueue<Message> _messageQueue = new ConcurrentQueue<Message>();
		readonly AutoResetEvent _messageIncomming = new AutoResetEvent(false);
		readonly object _closeMutex = new object();

		bool _isClosed = false;
		readonly IDisposable _outgoingMessagesSub;

		public IObservable<Message> IncomingMessages
		{
			get { return _incomingMessages; }
		}

		public IObserver<Message> OutgoingMessages
		{
			get { return _outgoingMessages; }
		}

		readonly ReplaySubject<DateTime> _disconnected = new ReplaySubject<DateTime>(1);
		public IObservable<DateTime> Disconnected { get { return _disconnected; } }

		readonly ClientInfo _clientInfo;
		readonly Action<LocalSocketClient> _close;

		public LocalSocketClient(TcpClient client)
			: this(client, _ => { })
		{ }

		internal LocalSocketClient(TcpClient client, Action<LocalSocketClient> close)
		{
			_clientInfo = new ClientInfo(client);
			_close = close;
			_outgoingMessagesSub = _outgoingMessages.Subscribe(EnqueueMessage);
			var pollAndSendThread = new Thread(PollAndSendThread) { IsBackground = true, Name = "PollAndSend local socket client" };
			pollAndSendThread.Start();
		}

		public void EnqueueMessage(Message message)
		{
			var handler = _clientInfo != null ? _clientInfo.Socket : null;
			if (handler == null || !handler.Connected)
				return;

			_messageQueue.Enqueue(message);
			_messageIncomming.Set();
		}

		void PollAndSendThread()
		{
			while (!_isClosed)
			{
				_messageIncomming.WaitOne();

				Message message;
				while (_messageQueue.TryDequeue(out message))
				{
					ReportFactory.FallbackReport.Trace("Sent message[" + message.MessageType + "]: " + message.Payload);
					var payloadInBytes = Encoding.UTF8.GetBytes(message.Payload);
					var messageTypeInBytes = Encoding.UTF8.GetBytes(message.MessageType + "\n");

					var lengthInText = payloadInBytes.Length.ToString(CultureInfo.InvariantCulture);
					var lengthInBytes = Encoding.UTF8.GetBytes(lengthInText + "\n");
					var finalBytes = messageTypeInBytes.Concat(lengthInBytes.Concat(payloadInBytes)).ToArray();

					try
					{
						_clientInfo.Stream.Write(finalBytes, 0, finalBytes.Length);
					}
					catch (Exception)
					{
						Close();
					}
				}
			}
		}

		public void Close()
		{
			lock (_closeMutex)
			{
				if (_isClosed)
					return;

				_isClosed = true;
			}

			_outgoingMessagesSub.Dispose();
			_messageIncomming.Set();
			Task.Run(
				() =>
				{
					while (!_messageQueue.IsEmpty)
					{
						Thread.Sleep(1);
					}
				}).Wait(TimeSpan.FromSeconds(5));


			_incomingMessages.OnCompleted();
			_incomingMessages.Dispose();

			var isConnected = _clientInfo.Socket.Connected;
			if (isConnected)
				_clientInfo.Socket.Close();

			_disconnected.OnNext(DateTime.Now);
			_disconnected.Dispose();

			_close(this);
		}

		public void StartRead()
		{
			try
			{
				_clientInfo.Stream.BeginRead(_clientInfo.Buffer, 0, ClientInfo.BufferSize, Read, _clientInfo);
			}
			catch (Exception)
			{
				Close();
			}
		}

		void Read(IAsyncResult ar)
		{
			var clientInfo = (ClientInfo)ar.AsyncState;
			var handler = clientInfo.Socket;

			if (!handler.Connected)
				return;

			try
			{
				var bytesRead = clientInfo.Stream.EndRead(ar);
				if (bytesRead > 0)
				{
					clientInfo.TempCache.AddRange(clientInfo.Buffer.Take(bytesRead));
					ParseChunks(clientInfo);
					StartRead();
				}
				else
				{
					Close();
				}
			}
			catch (Exception)
			{
				Close();
			}
		}

		void ParseChunks(ClientInfo clientInfo)
		{
			try
			{
				int messageSize;
				string messageType;

				var messageTypeLength = ParseMessageType(clientInfo.TempCache, 0, out messageType) + 1;
				var messageSizeLength = ParseMessageSize(clientInfo.TempCache, messageTypeLength, out messageSize) + 1;

				if (messageSizeLength > 0 && clientInfo.TempCache.Count >= messageTypeLength + messageSizeLength + messageSize)
				{
					clientInfo.TempCache.RemoveRange(0, messageTypeLength + messageSizeLength);
					var message = Encoding.UTF8.GetString(clientInfo.TempCache.Take(messageSize).ToArray());
					clientInfo.TempCache.RemoveRange(0, messageSize);

					ReportFactory.FallbackReport.Trace("Received message[" + messageType + "]: " + message);
					_incomingMessages.OnNext(new Message(messageType, message));

					if (clientInfo.TempCache.Count > 0)
						ParseChunks(clientInfo);
				}
			}
			catch (Exception)
			{
				clientInfo.TempCache.Clear();
			}
		}

		int ParseMessageType(IReadOnlyList<byte> data, int offset, out string messageType)
		{
			messageType = "";
			var firstNewLine = data.Skip(offset).IndexOf((byte)'\n');
			if(firstNewLine > 0)
				messageType = Encoding.UTF8.GetString(data.Take(firstNewLine).ToArray());

			return firstNewLine - offset;
		}

		int ParseMessageSize(IReadOnlyList<byte> data, int offset, out int messageSize)
		{
			for (var i = offset; i < data.Count; ++i)
			{
				if (data[i] != '\n') continue;

				if (!int.TryParse(Encoding.UTF8.GetString(data.Skip(offset).Take(i - offset).ToArray()), out messageSize))
					throw new InvalidOperationException("This should never happen. unless the client sends invalid data.");

				return i - offset;
			}

			messageSize = 0;
			return -1;
		}

		public void Dispose()
		{
			Close();
		}
	}

	static class ReportExtensions
	{
		public static void Trace(this IReport report, object o, ReportTo destination = ReportTo.Log)
		{
#if TRACE_NETWORK
			report.Info(o, destination);
#endif
		}
	}
}