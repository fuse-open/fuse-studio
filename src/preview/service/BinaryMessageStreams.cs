using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using Outracks.IPC;

namespace Outracks
{
	public static class BinaryMessageStreams
	{
		public static IDisposable BeginWritingMessages(this PipeName pipe, string processName, Action<Exception> onWriteError, params IObservable<IBinaryMessage>[] messages)
		{
			var ctSource = new CancellationTokenSource();
			var queue = new DispatcherQueue<IBinaryMessage>();
			var writeThread = new Thread(() =>
			{
				try
				{
					using (var stream = Pipe.Connect(pipe).Result)
					using (var writer = new BinaryWriter(stream))
					{
						queue.Dispatch(message => message.WriteTo(writer), ctSource.Token);
					}
				}
				catch (Exception e)
				{
					onWriteError(e);
				}
			})
			{
				Name = "Write binary messages to " + processName,
				IsBackground = true,
			};

			writeThread.Start();

			return Disposable.Combine(
				Disposable.Create(ctSource.Cancel),
				messages.Merge().Subscribe(queue.Enqueue));
		}

		public static IDisposable BeginWritingMessages(this Stream stream, string processName, Action<Exception> onWriteError, params IObservable<IBinaryMessage>[] messages)
		{
			var ctSource = new CancellationTokenSource();
			var queue = new DispatcherQueue<IBinaryMessage>();
			var writeThread = new Thread(() =>
			{
				try
				{
					using (var writer = new BinaryWriter(stream))
					{
						queue.Dispatch(message => message.WriteTo(writer), ctSource.Token);
					}
				}
				catch (Exception e)
				{
					onWriteError(e);
				}
			})
			{
				Name = "Write binary messages to " + processName,
				IsBackground = true,
			};

			writeThread.Start();

			return Disposable.Combine(
				Disposable.Create(ctSource.Cancel),
				messages.Merge().Subscribe(queue.Enqueue));
		}


		public static IConnectableObservable<IBinaryMessage> ReadMessages(this PipeName pipe, string processName)
		{
			return Observable
				.Defer(() => Pipe.Host(pipe).ToObservable())
				.Select(p => p.ReadMessages(processName))
				.Concat()
				.Publish();
		}

		public static IObservable<IBinaryMessage> ReadMessages(this Stream stream, string processName)
		{
			return new BinaryReader(stream).ReadMessages(processName).RefCount();
		}

		static IConnectableObservable<IBinaryMessage> ReadMessages(this BinaryReader reader, string processName)
		{
			return Observable.Create<IBinaryMessage>(listener =>
			{
				try
				{
					var ctSource = new CancellationTokenSource();

					var readThread = new Thread(() =>
						{
							try
							{
								try
								{
									using (reader)
									{
										while (!ctSource.IsCancellationRequested)
										{
											var msg = BinaryMessage.ReadFrom(reader);
											listener.OnNext(msg);
										}
									}
								}
								catch (EndOfStreamException)
								{
									// Expected exception so swallow it
								}
								catch (IOException e)
								{
									int forciblyClosedConnection = -2146232800;
									if (e.HResult != forciblyClosedConnection)
										throw;
								}
							}
							catch (Exception e)
							{
								listener.OnError(e);
							}
							finally
							{
								listener.OnCompleted();
							}
						})
					{
						Name = "Read binary messages from " + processName,
						IsBackground = true
					};

					readThread.Start();

					return Disposable.Create(() => ctSource.Cancel());
				}
				catch (Exception e)
				{
					listener.OnError(e);

					return Disposable.Empty;
				}
			})
			.Publish();
		}

	}
}
