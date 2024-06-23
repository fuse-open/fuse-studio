using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Outracks;
using Outracks.IO;
using Outracks.Simulator.Bytecode;
using Uno.Build.FuseJS;
using Uno.Configuration;

namespace Fuse.Preview
{
	class AssetsWatcher : IDisposable
	{
		readonly IFileSystem _fileSystem;
		readonly IScheduler _scheduler;
		readonly FileSender<ProjectDependency> _dependencyFileSender;
		readonly FileSender<AbsoluteFilePath> _bundleFileSender;
		readonly ReifyerLogAdapter _logAdapter = new ReifyerLogAdapter();
		readonly Lazy<Transpiler> _transpiler;
		readonly Subject<string> _logMessages = new Subject<string>();

		public IObservable<string> LogMessages
		{
			get
			{
				return _logAdapter.Events.Select(x => x.ToString())
					.Merge(_logMessages.Select(s => s + '\n'));
			}
		}

		public AssetsWatcher(IFileSystem fileSystem, IObservable<AbsoluteDirectoryPath> projectRootDirectory, IScheduler scheduler)
		{
			_fileSystem = fileSystem;
			_scheduler = scheduler;
			_dependencyFileSender = FileSourceSender.Create(fileSystem);
			_bundleFileSender = BundleFileSender.Create(fileSystem, projectRootDirectory);
			_transpiler = new Lazy<Transpiler>(() => new Transpiler(_logAdapter.Log, UnoConfig.Current));
		}

		public IObservable<CoalesceEntry> UpdateChangedDependencies(IObservable<IImmutableSet<ProjectDependency>> dependencies)
		{
			return WatchSet(
				dependencies,
				onItemAdded: projDep =>
				{
					var path = AbsoluteFilePath.Parse(projDep.Path);
					return Watch(path)
						.Select(data => _dependencyFileSender.CreateMessages(data.WithMetadata(projDep)))
						.Switch();
				});
		}

		public IObservable<CoalesceEntry> UpdateChangedBundleFiles(IObservable<IImmutableSet<AbsoluteFilePath>> bundleFiles)
		{
			return WatchSet(
				bundleFiles,
				onItemAdded: bundleFile =>
				{
					return Watch(bundleFile)
						.Select(d => _bundleFileSender.CreateMessages(d))
						.Switch();
				});
		}

		public IObservable<CoalesceEntry> UpdateChangedFuseJsFiles(IObservable<IImmutableSet<AbsoluteFilePath>> fuseJsFiles)
		{
			return WatchSet(
				fuseJsFiles,
				onItemAdded: bundleFile =>
				{
					return Watch(bundleFile)
						.Select(TranspileJs)
						.NotNone()
						.Select(d => _bundleFileSender.CreateMessages(d))
						.Switch();
				});
		}

		Optional<FileDataWithMetadata<AbsoluteFilePath>> TranspileJs(FileDataWithMetadata<AbsoluteFilePath> jsFile)
		{
			string output;
			if (_transpiler.Value.TryTranspile(jsFile.Metadata.NativePath, Encoding.UTF8.GetString(jsFile.Data), out output))
			{
				// Bundle transpiled code with the original source file metadata
				return FileDataWithMetadata.Create(jsFile.Metadata, Encoding.UTF8.GetBytes(output));
			}
			else
			{
				_logAdapter.Error(jsFile.Metadata);

				// Don't propagate result
				return Optional.None();
			}
		}

		IObservable<TOut> WatchSet<T, TOut>(IObservable<IEnumerable<T>> sets, Func<T, IObservable<TOut>> onItemAdded)
		{
			return sets
				.CachePerElement(
					data => data,
					(data) =>
					{
						var disposable = new BehaviorSubject<Optional<IDisposable>>(Optional.None());
						var proxy = new Subject<TOut>();
						var changes = Observable.Create<TOut>(
							observer =>
							{
								var dis = proxy.Subscribe(observer);

								if(!disposable.Value.HasValue)
									disposable.OnNext(Optional.Some(onItemAdded(data).Subscribe(proxy)));

								return dis;
							});

						return new
						{
							changes,
							dispose = disposable
						};
					}, v => v.dispose.Value.Do(d => d.Dispose()))
				.Select(p => p.Select(v => v.changes).Merge())
				.Switch();
		}

		IObservable<FileDataWithMetadata<AbsoluteFilePath>> Watch(AbsoluteFilePath path)
		{
			return _fileSystem
				.Watch(path)
				.CatchAndRetry(delay: TimeSpan.FromSeconds(1), scheduler: _scheduler)
				.Throttle(TimeSpan.FromSeconds(1.0 / 30.0), _scheduler)
				.StartWith(Unit.Default)
				.Select(_ => path)
				.DiffFileContent(_fileSystem)
				.CatchAndRetry(TimeSpan.FromSeconds(20),
					e =>
					{
						_logMessages.OnNext("Failed to load '" + path.NativePath + "': " + (e.InnerException != null ? e.InnerException.Message : e.Message));
					});
		}

		public void Dispose()
		{
			if (_transpiler.IsValueCreated)
				_transpiler.Value.Dispose();
		}
	}
}
