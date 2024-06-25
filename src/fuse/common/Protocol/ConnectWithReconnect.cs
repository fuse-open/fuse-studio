using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Outracks.Fuse.Protocol;

namespace Outracks.Fuse
{
	public static class ConnectWithReconnect
	{
		public static IObservable<Optional<IMessagingService>> ConnectRetry(
			this IFuseLauncher spawner,
			string identifier)
		{
			var daemon = new BehaviorSubject<Optional<IMessagingService>>(Optional.None());
			Task.Run(
				() =>
				{
					while (true)
					{
						try
						{
							var daemonConnection = spawner.Connect(
								identifier,
								TimeSpan.FromMinutes(1));

							daemon.OnNext(Optional.Some(daemonConnection));

							var waitForDone = new AutoResetEvent(false);
							daemonConnection.ConnectionLost.Subscribe(d => waitForDone.Set());
							waitForDone.WaitOne();
						}
						catch (Exception)
						{
							Thread.Sleep(TimeSpan.FromSeconds(10));
						}
						finally
						{
							daemon.OnNext(Optional.None());
						}
					}
				});

			return daemon;
		}
	}
}