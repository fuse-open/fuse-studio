using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Outracks.Fuse.Live
{
	using IO;

	interface IDocument<T> : IDisposable
	{
		IObservable<Optional<Exception>> Errors { get; }

		IObservable<Optional<Exception>> ErrorsDuringLoading { get; }

		/// <summary>
		/// This observable emits the latest contents of the document.
		/// Upon subscription this observable replays one elements.
		/// </summary>
		IObservable<T> Contents { get; }

		/// <summary>
		/// This observable emits the latest contents read from disk, ignoring changes saved by invoking Save().
		/// Upon subscription this observable replays one elements.
		/// </summary>
		IObservable<T> ExternalChanges { get; }

		Task Save(T contents);
	}

	class FileWatchingDocument : IDocument<byte[]>
	{
		public static readonly TimeSpan PreLogRetryInterval = TimeSpan.FromMilliseconds(100);
		public static readonly TimeSpan PostLogRetryInterval = TimeSpan.FromMilliseconds(1000);
		public static readonly TimeSpan RetryErrorMessageDelay = TimeSpan.FromSeconds(3);

		readonly IDisposable _garbage;
		readonly IFileSystem _fs;
		readonly AbsoluteFilePath _path;
		readonly BehaviorSubject<Optional<byte[]>> _lastKnownChanges = new BehaviorSubject<Optional<byte[]>>(Optional.None());
		readonly BehaviorSubject<Optional<Exception>> _errorsDuringSaving = new BehaviorSubject<Optional<Exception>>(Optional.None());
		readonly BehaviorSubject<Optional<Exception>> _errorsDuringLoading = new BehaviorSubject<Optional<Exception>>(Optional.None());
		readonly IScheduler _scheduler;

		public FileWatchingDocument(IFileSystem fs, AbsoluteFilePath path, IScheduler scheduler = null)
		{
			_fs = fs;
			_path = path;
			_scheduler = scheduler ?? Scheduler.Default;

			var contents =
				fs.Watch(path)
				.StartWith(Unit.Default)
				.CatchAndRetry(delay: TimeSpan.FromSeconds(1), scheduler: _scheduler)
				.Throttle(TimeSpan.FromSeconds(1.0 / 30.0), _scheduler)
				.Select(
					notifyTime => Observable.DeferAsync(
						async token => Observable.Return(
							await ReadAllBytesAndRetryOnError(token))))
				.Switch()
				.Replay(1);

			var externalChanges = contents
				.Where(bytes => _lastKnownChanges.Value.Select(bytes.SequenceEqual).Or(false) == false)
				.Do(bytes => _lastKnownChanges.OnNext(bytes))
				.Replay(1);

			_garbage = Disposable.Combine(
				externalChanges.Connect(),
				contents.Connect());

			Contents = contents;
			ExternalChanges = externalChanges;
		}

		public IObservable<Optional<Exception>> Errors
		{
			get { return _errorsDuringSaving.Merge(_errorsDuringLoading);  }
		}

		public IObservable<Optional<Exception>> ErrorsDuringLoading
		{
			get { return _errorsDuringLoading; }
		}

		public IObservable<byte[]> Contents
		{
			get; private set;
		}

		public IObservable<byte[]> ExternalChanges
		{
			get; private set;
		}

		public async Task Save(byte[] contents)
		{
			_lastKnownChanges.OnNext(contents);
			try
			{
				using (var file = _fs.Open(_path, FileMode.Create, FileAccess.Write, FileShare.Read))
				{
					await file.WriteAllBytesAsync(contents);
				}
				_errorsDuringSaving.OnNext(Optional.None());
			}
			catch (Exception e)
			{
				_errorsDuringSaving.OnNext(e);
			}
		}

		public void Dispose()
		{
			_garbage.Dispose();
		}

		private async Task<byte[]> ReadAllBytesAndRetryOnError(CancellationToken token)
		{
			var startTime = _scheduler.Now;
			string lastErrorMessage = null;
			TimeSpan retryInterval = PreLogRetryInterval;
			var tokenObservable = Observable.Create<long>(observer => token.Register(() => observer.OnNext(0)));

			while (true)
			{
				token.ThrowIfCancellationRequested();
				try
				{
					using (var file = _fs.OpenRead(_path))
					{
						var bytes = await file.ReadAllBytesAsync();
						_errorsDuringLoading.OnNext(Optional.None());
						return bytes;
					}
				}
				catch (Exception exception)
				{
					token.ThrowIfCancellationRequested();

					if (_scheduler.Now - startTime > RetryErrorMessageDelay && lastErrorMessage != exception.Message)
					{

						// After 3 seconds we only retry every second, to slow down IO hammering
						retryInterval = PostLogRetryInterval;
						try
						{
							throw new IOException(string.Format(exception.Message.TrimEnd('.') + ". Retrying in background until problem is resolved."), exception);
						}
						catch (Exception wrappedException)
						{
							_errorsDuringLoading.OnNext(wrappedException);
						}
						lastErrorMessage = exception.Message;
					}
				}

				// We're using an Observable.Timer here to make this work with HistoricalScehduler in test.
				await Observable
					.Timer(retryInterval, _scheduler)
					.Merge(tokenObservable).FirstAsync();
			}
		}
	}

}