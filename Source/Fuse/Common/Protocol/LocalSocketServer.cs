using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Threading;
using Outracks.IPC;

namespace Outracks.Fuse.Protocol
{
	public class LocalSocketServer : IDisposable
	{
		readonly IReport _report;
		readonly Thread _listenerThread;
		readonly BehaviorSubject<ImmutableList<LocalSocketClient>> _clients =
			new BehaviorSubject<ImmutableList<LocalSocketClient>>(ImmutableList<LocalSocketClient>.Empty);
		readonly AutoResetEvent _hasStoppedListening = new AutoResetEvent(false);

		TcpListener _listener;
		volatile bool _continueListening;
		volatile bool _isClosing;
		readonly object _closeMutex = new object();

		public readonly Subject<LocalSocketClient> ClientConnected = new Subject<LocalSocketClient>();
		public readonly Subject<LocalSocketClient> ClientDisconnected = new Subject<LocalSocketClient>();

		public LocalSocketServer(IReport report)
		{
			_report = report;
			_listenerThread = new Thread(Listen) { Name = "Local socket server", IsBackground = true };
		}

		public void Host(int port)
		{
			var address = IPAddress.Loopback;
			var endpoint = new IPEndPoint(address, port);
			_listener = new TcpListener(endpoint);

			if (Environment.OSVersion.Platform != PlatformID.Unix &&
			    Environment.OSVersion.Platform != PlatformID.MacOSX)
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

			try
			{
				_listener.Start(20);

				_continueListening = true;
				_listenerThread.Start(_listener);
			}
			catch (Exception)
			{
				Close();
				throw;
			}
		}

		void Listen(object param)
		{
			try
			{
				var listener = (TcpListener) param;
				while (_continueListening)
				{
					if (!listener.Pending())
					{
						Thread.Sleep(50);
						continue;
					}

					try
					{
						var client = new LocalSocketClient(listener.AcceptTcpClient(), CloseClient);

						_clients.OnNext(_clients.Value.Add(client));

						try
						{
							ClientConnected.OnNext(client);
						}
						catch (Exception e)
						{
							// TODO Log
							_report.Exception("Failed to handle client connection", e);
							continue;
						}						
					}
					catch (InvalidOperationException e)
					{
						// TODO: Log
						_report.Exception("", e);
						continue;
					}
					catch (SocketException e)
					{
						// TODO: Log
						_report.Exception("", e);
						continue;
					}
				}
			}
			catch (Exception e)
			{
				// TODO: Log
				_report.Exception("", e);
				Close();
			}
			finally
			{
				_hasStoppedListening.Set();
			}
		}

		public void Dispose()
		{
			Close();
		}

		void Close()
		{
			lock (_closeMutex)
			{
				if (_isClosing)
					return;

				_isClosing = true;
			}

			_continueListening = false;
			_listener.Stop();

			var clients = _clients.Value.ToArray();
			foreach (var clientInfo in clients)
			{
				clientInfo.Close();
			}

			ClientConnected.Dispose();
			ClientDisconnected.Dispose();
			_hasStoppedListening.WaitOne(500);
		}

		void CloseClient(LocalSocketClient client)
		{
			client.Close();
			_clients.Update(l => l.Remove(client));

			ClientDisconnected.OnNext(client);
		}
	}
}
