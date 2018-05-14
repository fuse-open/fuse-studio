using System;
using Uno;
using Uno.Collections;
using System.IO;
using Uno.Net;
using Uno.Net.Sockets;
using Uno.Threading;
using Uno.Collections;
using Uno.Diagnostics;

namespace Outracks.Simulator
{
	using UnoHost;
	using Protocol;
	
	public interface ISimulatorClient : IDisposable
	{
		ConcurrentQueue<IBinaryMessage> IncommingMessages { get; }

		void Send(IBinaryMessage message);

		bool IsOnline { get; }
	}

	public class OfflineSimulatorClient : ISimulatorClient
	{
		readonly ConcurrentQueue<IBinaryMessage> _messagesFromClient = new ConcurrentQueue<IBinaryMessage>();
		readonly ConcurrentQueue<IBinaryMessage> _messagesToClient = new ConcurrentQueue<IBinaryMessage>();

		public OfflineSimulatorClient(params IBinaryMessage[] initialMessages)
		{
			foreach (var msg in initialMessages)
				_messagesToClient.Enqueue(msg);
		}

		public ConcurrentQueue<IBinaryMessage> IncommingMessages
		{
			get { return _messagesToClient; }
		}

		public void Send(IBinaryMessage message)
		{
			_messagesFromClient.Enqueue(message);
		}

		public void Dispose()
		{
			
		}

		public bool IsOnline
		{
			get
			{
				return false;
			}
		}
	}

	public class FailedToConnectToEndPoint : Exception
	{
		public FailedToConnectToEndPoint(IPEndPoint endpoint, Exception e)
			: base(endpoint.ToString() + ": " + e.Message)
		{ }
	}

	public class FailedToConnectToSimulator : Exception
	{
		public readonly ImmutableList<Exception> InnerExceptions;

		public FailedToConnectToSimulator(IEnumerable<Exception> innerExceptions)
			: base("Failed to connect to simulator host: " + innerExceptions.ToIndentedLines())
		{
			InnerExceptions = innerExceptions.ToImmutableList();
		}
	}

	public static class ToIndentedLinesExtension
	{
		public static string ToIndentedLines(this IEnumerable<Exception> innerExceptions)
		{
			var s = "";
			foreach (var e in innerExceptions)
				s += "    " + e.Message + "\n";
			return s;
		}
	}

	public static class ConnectToFirstRespondingEndpoint
	{
		public static Task<Socket> Execute(IEnumerable<IPEndPoint> simulatorEndpoints)
		{
			var isNotConnected = new AutoResetEvent(true);
			var socketTasks = new List<Task<Socket>>();
			foreach (var endpoint in simulatorEndpoints)
				socketTasks.Add(Tasks.Run<Socket>(new ConnectToEndpointClosure(endpoint, isNotConnected).Execute));

			return Tasks.WaitForFirstResult<Socket>(socketTasks, OnNoResult);
		}

		static Socket OnNoResult(IEnumerable<Exception> exceptions)
		{
			throw new FailedToConnectToSimulator(exceptions); // TODO: misplaced information
		}
	}

	class ConnectToEndpointClosure
	{
		readonly IPEndPoint _endpoint;
		readonly EventWaitHandle _isNotConnected;

		public ConnectToEndpointClosure(IPEndPoint endpoint, EventWaitHandle isNotConnected)
		{
			_endpoint = endpoint;
			_isNotConnected = isNotConnected;
		}

		public Socket Execute()
		{
			try
			{
				var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.Connect(_endpoint);

				if (_isNotConnected.WaitOne(0) == false)
				{
					socket.Dispose();
					throw new Exception("Connection already established");
				}

				return socket;
			}
			catch (Exception e)
			{
				throw new FailedToConnectToEndPoint(_endpoint, e);// TODO: misplaced information
			}
		}
	}

	
	public class SimulatorClient : ISimulatorClient
	{
		readonly Socket _socket;
		readonly NetworkStream _stream;
		readonly BinaryWriter _writer;
		readonly BinaryReader _reader;
		readonly ConcurrentQueue<IBinaryMessage> _messagesFromClient = new ConcurrentQueue<IBinaryMessage>();
		readonly ConcurrentQueue<IBinaryMessage> _messagesToClient = new ConcurrentQueue<IBinaryMessage>();
		readonly Thread _readWorker;
		readonly Thread _writeWorker;
		//readonly IDisposable _alsoReceieveMessagesFromPipe;

		public ConcurrentQueue<IBinaryMessage> IncommingMessages 
		{ 
			get { return _messagesToClient; } 
		}

		public void Send(IBinaryMessage message)
		{
			_messagesFromClient.Enqueue(message);
		}

		public SimulatorClient(Socket socket)
		{
			_socket = socket;
			_stream = new NetworkStream(_socket);
			_writer = new BinaryWriter(_stream);
			_reader = new BinaryReader(_stream);
			
			_readWorker = new Thread(ReadLoop);
			_writeWorker = new Thread(WriteLoop);

			if defined(DotNet)
			{
				_readWorker.IsBackground = true;
				_readWorker.Name = "Read from " + _socket.RemoteEndPoint;
				_writeWorker.IsBackground = true;
				_writeWorker.Name = "Write to " + _socket.RemoteEndPoint;

			}

			_readWorker.Start();
			_writeWorker.Start();
		}

		// no volative in UNO, boo :(
		bool _running = true;

		void ReadLoop()
		{
			try
			{
				while (_running)
				{
					while (_socket.Poll(0, SelectMode.Read))
						_messagesToClient.Enqueue(BinaryMessage.ReadFrom(_reader));

					Thread.Sleep(10);
				}
			}
			catch(Exception e)
			{
				_messagesToClient.Enqueue(new Error(ExceptionInfo.Capture(e)));
			}
		}

		void WriteLoop()
		{
			try
			{
				while (_running)
				{
					IBinaryMessage message;
					while (_messagesFromClient.TryDequeue(out message))
						message.WriteTo(_writer);

					Thread.Sleep(10);
				}
			}
			catch (Exception e)
			{
				_messagesToClient.Enqueue(new Error(ExceptionInfo.Capture(e)));
				_running = false;
			}
		}

		public void Dispose()
		{
			_running = false;
			
			//_alsoReceieveMessagesFromPipe.Dispose();

			_readWorker.Join();
			_writeWorker.Join();

			_stream.Dispose();
			try
			{
				_socket.Shutdown(SocketShutdown.Both);
				_socket.Close();
			}
			catch (Exception e)
			{
				debug_log(e.Message);
			}
		}

		public bool IsOnline
		{
			get
			{
				return true;
			}
		}

	}

}
