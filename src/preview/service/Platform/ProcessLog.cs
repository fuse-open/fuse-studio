using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reactive.Linq;
using System.Threading;

namespace Fuse.Preview
{
	class ProcessLog
	{
		static readonly IPlatform Platform = PlatformFactory.Create();

		static bool _readFromCalledBefore = false;
		public static IObservable<string> ReadFrom(IProcess process)
		{
			if (_readFromCalledBefore)
				throw new InvalidOperationException("ProcessLog.ReadFrom is expected to only be called once!");
			_readFromCalledBefore = true;

			var logRead =  Observable.Create<string>(
				observer =>
				{
					var readThread = new Thread(
						() =>
						{
							while (true)
							{
								try
								{
									using (var reader = new BinaryReader(process.OpenStream("log")))
									{
										while (true)
										{
											var text = reader.ReadString();
											observer.OnNext(text);
										}
									}
								}
								catch (Exception)
								{
									Thread.Sleep(16);
								}
							}
						})
					{
						Name = "ProcessLog Read Thread",
						IsBackground = true
					};
					readThread.Start();

					return () => readThread.Abort();
				})
			.Publish()
			.RefCount();

			return logRead;
		}

		static readonly ConcurrentQueue<string> Queue = new ConcurrentQueue<string>();
		static readonly AutoResetEvent Signal = new AutoResetEvent(false);

		public static void StartWriteThread()
		{
			var writeThread = new Thread(
				() =>
				{
					while (true)
					{
						try
						{
							WriteChunks(Platform.CreateStream("log"));
						}
						catch (Exception)
						{
							Thread.Sleep(16);
						}
					}
				})
			{
				Name = "ProcessLog Pump Thread",
				IsBackground = true,
			};

			writeThread.Start();
		}

		static void WriteChunks(Stream stream)
		{
			using (var writer = new BinaryWriter(stream))
			while (true)
			{
				string message;
				while (Queue.TryDequeue(out message))
					writer.Write(message);

				Signal.WaitOne();
			}
		}

		public static void Append(string message)
		{
			Queue.Enqueue(message);
			Signal.Set();
		}
	}
}