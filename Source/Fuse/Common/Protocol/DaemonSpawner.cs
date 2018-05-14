using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Outracks.Fuse.Protocol;
using Outracks.IPC;

namespace Outracks.Fuse
{
	public static class DaemonSpawner
	{
		/// <exception cref="FailedToSpawnDaemon" />
		/// <exception cref="FailedToConnectToDaemon" />
		public static Task<IMessagingService> ConnectOrSpawnAsync(this IFuseLauncher fuseLauncher)
		{
			return Task.Run(() => fuseLauncher.ConnectOrSpawn());
		}

		/// <exception cref="FailedToSpawnDaemon" />
		/// <exception cref="FailedToConnectToDaemon" />
		public static IMessagingService ConnectOrSpawn(this IFuseLauncher fuseLauncher)
		{
			try
			{
				return fuseLauncher.Connect();
			}
			catch (FailedToConnectToDaemon)
			{
				return fuseLauncher.ConnectForSure();
			}
		}

		static Optional<bool> IsPortOpen(int port)
		{
			try
			{
				return NetworkHelper.IsPortOpen(port);
			}
			catch (FailedToLoadTcpTable e)
			{
				ReportFactory.FallbackReport.Exception("Failed to load TCP table", e);
				return Optional.None();
			}
		}

		/// <exception cref="FailedToSpawnDaemon" />
		/// <exception cref="FailedToConnectToDaemon" />
		static IMessagingService ConnectForSure(this IFuseLauncher fuseLauncher)
		{
			try
			{
				SpawnDaemon(fuseLauncher);
			}
			catch (Exception e)
			{
				throw new FailedToSpawnDaemon(e);
			}

			return fuseLauncher.Connect();
		}

		static void SpawnDaemon(IFuseLauncher fuseLauncher)
		{
			// TODO: probably inspect the error more
			var server = fuseLauncher.StartFuse("daemon", "-b");
			server.WaitForExit();
			if (server.ExitCode != 0)
				throw new FailedToSpawnDaemon(server.ExitCode);
		}

		public static Task<IMessagingService> ConnectAsync(this IFuseLauncher fuseLauncher)
		{
			return Task.Run(() => fuseLauncher.Connect());
		}

		/// <exception cref="FailedToConnectToDaemon" />
		public static IMessagingService Connect(this IFuseLauncher fuseLauncher)
		{
			try
			{
				var tcpClient = new TcpClient("127.0.0.1", 12122);
				var client = new LocalSocketClient(tcpClient);
				client.StartRead();

				return new Client(
					client,
					new Serializer());
			}
			catch (Exception e)
			{
				throw new FailedToConnectToDaemon(e);
			}
		}

		public static DaemonKey GetDaemonKey(this IFuseLauncher fuseLauncher)
		{
			//var p = _fuseLauncher.Start("daemon", new []{ "--get-key" });

			// Temporary workaround for slow startup.
			return DaemonKey.GetDaemonKey();
		}
	}

	public abstract class DaemonException : Exception
	{
		protected DaemonException(string message, Exception innerException)
			: base(message, innerException)
		{ }

		protected DaemonException(string message)
			: base(message)
		{ }
	}

	public class FailedToConnectToDaemon : DaemonException
	{
		public FailedToConnectToDaemon(Exception innerException)
			: base("Failed to connect to daemon: " + innerException.Message, innerException)
		{ }
	}

	public class FailedToSpawnDaemon : DaemonException
	{
		public FailedToSpawnDaemon(Exception innerException)
			: base("Failed to start daemon: " + innerException.Message, innerException)
		{ }

		public FailedToSpawnDaemon(int exitCode)
			: base("Daemon exited with code " + exitCode)
		{ }
	}

	public class FailedToGreetDaemon : DaemonException
	{
		public FailedToGreetDaemon(Exception innerException)
			: base("Daemon handshake failed: " + innerException.Message, innerException)
		{ }

		public FailedToGreetDaemon(string message)
			: base(message)
		{ }
	}

}