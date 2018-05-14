using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Threading;
using Outracks;

namespace Fuse.Preview
{
	public interface ISocketServer : IDisposable
	{
		IPEndPoint LocalEndPoint { get; }
	}

	public class SocketServer : ISocketServer
	{
		public static ISocketServer Start(int port, Action<NetworkStream, EndPoint> clientRun)
		{
			var server = new SocketServer(new TcpListener(IPAddress.Any, port), clientRun);
			server.Run();
			return server;
		}

		readonly TcpListener _listener;
		readonly Action<NetworkStream, EndPoint> _clientRun;
		readonly CancellationTokenSource _cancelSource = new CancellationTokenSource();
		readonly AutoResetEvent _doneCanceling = new AutoResetEvent(false);

		public IPEndPoint LocalEndPoint
		{
			get { return (IPEndPoint)_listener.Server.LocalEndPoint; }
		}

		SocketServer(TcpListener listener, Action<NetworkStream, EndPoint> clientRun)
		{
			_listener = listener;
			_clientRun = clientRun;
		}

		void Run()
		{
			if (Environment.OSVersion.Platform != PlatformID.Unix && Environment.OSVersion.Platform != PlatformID.MacOSX)
			{
				// Disable inheritance of server-socket, so child processes (like ADB) won't keep
				// a valid handle to that socket open until the child-process itself dies.
				//
				// This is needed due to the unfortunate combination of Process.Start enabling
				// handle-inheritance without providing an escape-hatch, and BCL deciding to
				// create all handles with inheritance enabled, again without any opt-out option.
				//
				// If this fails, there's little useful we can do.
				if (SocketWin32.SetHandleInformation(_listener.Server.Handle, SocketWin32.HANDLE_FLAGS.INHERIT, SocketWin32.HANDLE_FLAGS.None) == false)
					Debug.Write("Failed to disable handle-inheritance for socket!");
			}

			_listener.Start();

			var thread = new Thread(RunInternal)
			{
				Name = "Listen for connections on " +_listener.LocalEndpoint,
				IsBackground = true
			};
			thread.Start(_cancelSource.Token);			
		}

		void RunInternal(object cancellationToken)
		{
			var cancel = (CancellationToken) cancellationToken;
			var clientsConnected = new BehaviorSubject<ImmutableList<TcpClient>>(ImmutableList<TcpClient>.Empty);

			try
			{
				while (!cancel.IsCancellationRequested)
				{
					if (!_listener.Pending())
					{
						Thread.Sleep(50);
						continue;
					}

					var client = _listener.AcceptTcpClient();

					clientsConnected.OnNext(clientsConnected.Value.Add(client));
					var stream = client.GetStream();
					new Thread(() =>
					{
						try
						{
							_clientRun(stream, client.Client.RemoteEndPoint);
						}
						catch (Exception e)
						{
							ReportFactory.FallbackReport.Warn("Exception from SocketServer client thread : '" + e.Message + "'");
						}
						finally
						{
							client.Close();
							clientsConnected.OnNext(clientsConnected.Value.Remove(client));
						}
					})
					{
						Name = "Socket server client thread",
						IsBackground = true,
					}.Start();
				}
			}
			catch (Exception)
			{	
			}
			finally
			{
				var clients = clientsConnected.Value.ToArray();
				foreach (var client in clients)
				{
					try
					{
						client.Close();
					}
					catch (Exception)
					{						
					}
				}

				_doneCanceling.Set();
			}
		}

		public void Dispose()
		{
			_cancelSource.Cancel();
			_listener.Stop();
			_doneCanceling.WaitOne(5000);
		}
	}
}