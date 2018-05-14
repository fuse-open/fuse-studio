using System;
using System.Threading.Tasks;
using Outracks.Fuse.Protocol;

namespace Outracks.Fuse
{
	public static class ConnectAndGreet
	{
		/// <exception cref="FailedToConnectToDaemon" />
		/// <exception cref="FailedToGreetDaemon" />
		public static async Task<IMessagingService> ConnectAsync(
			this IFuseLauncher spawner,
			string identifier)
		{
			var client = await spawner.ConnectAsync();

			try
			{
				await SendHelloRequestAsync(client, identifier, spawner.GetDaemonKey());
				return client;
			}
			catch (Exception e)
			{
				throw new FailedToGreetDaemon(e);
			}
		}

		/// <exception cref="FailedToConnectToDaemon" />
		/// <exception cref="FailedToGreetDaemon" />
		public static IMessagingService Connect(
			this IFuseLauncher spawner,
			string identifier,
			TimeSpan timeout)
		{
			var client = spawner.Connect();

			try
			{
				client.SendHelloRequest(identifier, timeout, spawner.GetDaemonKey());
				return client;
			}
			catch (Exception e)
			{
				throw new FailedToGreetDaemon(e);
			}
		}

		/// <exception cref="FailedToSpawnDaemon" />
		/// <exception cref="FailedToConnectToDaemon" />
		/// <exception cref="FailedToGreetDaemon" />
		public static async Task<IMessagingService> ConnectOrSpawnAsync(
			this IFuseLauncher spawner,
			string identifier)
		{
			var client = await spawner.ConnectOrSpawnAsync();

			try
			{
				await client.SendHelloRequestAsync(identifier, spawner.GetDaemonKey());
				return client;
			}
			catch (Exception e)
			{
				throw new FailedToGreetDaemon(e);
			}
		}


		/// <exception cref="FailedToSpawnDaemon" />
		/// <exception cref="FailedToConnectToDaemon" />
		/// <exception cref="FailedToGreetDaemon" />
		public static IMessagingService ConnectOrSpawn(
			this IFuseLauncher spawner,
			string identifier,
			TimeSpan timeout)
		{
			var client = spawner.ConnectOrSpawn();

			try
			{
				SendHelloRequest(client, identifier, timeout, spawner.GetDaemonKey());
				return client;
			}
			catch (Exception e)
			{
				throw new FailedToGreetDaemon(e);
			}
		}
		
		/// <exception cref="RequestFailed"></exception>
		/// <exception cref="TimeoutException"></exception>
		static void SendHelloRequest(
			this IMessagingService client,
			string identifier,
			TimeSpan timeout,
			DaemonKey daemonKey)
		{
			client
				.SendHelloRequestAsync(identifier, daemonKey)
				.GetResultAndUnpackExceptions(timeout)
				.OrThrow(new TimeoutException("Daemon did not respond to handshake (Timeout = " + timeout + ")"));
		}

		/// <exception cref="RequestFailed"></exception>
		/// <exception cref="TimeoutException"></exception>
		static async Task<HelloResponse> SendHelloRequestAsync(
			this IMessagingService client,
			string identifier,
			DaemonKey daemonKey)
		{
			return await client.Request(
				new HelloRequest
				{
					Identifier = identifier,
					DaemonKey = daemonKey.Serialize()
				});
		}

	}
}