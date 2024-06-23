using Uno;
using Uno.Net;
using Uno.Net.Sockets;
using Uno.Collections;
using Outracks.Simulator;
using Uno.Threading;
using Outracks;
using System.IO;
using Outracks.Simulator.Protocol;
using Outracks.Simulator.Bytecode;
using Outracks.UnoHost;

namespace Fuse.Simulator
{
	static class ConnectToFirstRespondingEndpoint
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
			throw new AggregateException(exceptions.ToArray()); // TODO: misplaced information
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
				throw e;// TODO: misplaced information
			}
		}
	}

	interface ISimulatorClient : IDisposable
	{
		Uno.Threading.ConcurrentQueue<IBinaryMessage> IncommingMessages { get; }
		void Send(IBinaryMessage message);
		ISimulatorClient Clone();
	}

	class OfflineClient : ISimulatorClient
	{
		readonly Uno.Threading.ConcurrentQueue<IBinaryMessage> messages = new Uno.Threading.ConcurrentQueue<IBinaryMessage>();
		readonly BytecodeCache _bytecodeCache;
		public OfflineClient(BytecodeCache bytecodeCache)
		{
			_bytecodeCache = bytecodeCache;
			messages.Enqueue(new BytecodeGenerated(bytecodeCache.Bytecode));
			for(var i = 0;i < bytecodeCache.BytecodeUpdates.Length;++i)
			{
				messages.Enqueue(bytecodeCache.BytecodeUpdates[i]);
			}
		}

		public void Send(IBinaryMessage message)
		{
		}

		public ISimulatorClient Clone()
		{
			return new OfflineClient(_bytecodeCache);
		}

		public Uno.Threading.ConcurrentQueue<IBinaryMessage> IncommingMessages
		{
			get { return messages; }
		}

		public void Dispose()
		{
		}
	}

	class SimulatorClient : ISimulatorClient
	{
		readonly Socket _socket;
		readonly NetworkStream _stream;
		readonly BinaryWriter _writer;
		readonly BinaryReader _reader;
		readonly Uno.Threading.ConcurrentQueue<IBinaryMessage> _messagesFromClient = new Uno.Threading.ConcurrentQueue<IBinaryMessage>();
		readonly Uno.Threading.ConcurrentQueue<IBinaryMessage> _messagesToClient = new Uno.Threading.ConcurrentQueue<IBinaryMessage>();
		readonly Thread _readWorker;
		readonly Thread _writeWorker;
		readonly EndPoint _endpoint;

		public Uno.Threading.ConcurrentQueue<IBinaryMessage> IncommingMessages 
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
			_endpoint = _socket.RemoteEndPoint;
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

		public ISimulatorClient Clone()
		{
			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			Logger.Info("Cloning connection: " + _endpoint);
			socket.Connect(_endpoint);
			return new SimulatorClient(socket);
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
				Logger.Error(e.Message);
			}
		}
	}
}
