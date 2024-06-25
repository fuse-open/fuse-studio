using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Xml.Linq;
using Fuse.Preview;
using Outracks.Fusion;
using Outracks.IO;
using Outracks.Simulator;
using Outracks.Simulator.Protocol;

namespace Outracks.Fuse.Live
{
	public class LiveDocument : IDocument, IDocumentLike
	{
		public static LiveDocument Open(
			AbsoluteFilePath path,
			IFileSystem fs,
			IObservable<ILookup<ObjectIdentifier, ObjectIdentifier>> metadata,
			BehaviorSubject<Dictionary<ObjectIdentifier, IElement>> idToElement,
			IObserver<IBinaryMessage> mutations,
			IScheduler scheduler)
		{
			IDocument<byte[]> fileOnDisk = new FileWatchingDocument(fs, path);


			var parsingErrors = new BehaviorSubject<Optional<Exception>>(Optional.None());

			var invalidated = new Subject<Unit>();

			var root = new LiveElement(
				file: path,
				metadata: metadata,
				isReadOnly: fileOnDisk.ErrorsDuringLoading.Or(parsingErrors)
					.Select(e => e.HasValue)
					.DistinctUntilChanged()
					.Replay(1).RefCount(),
				invalidated: invalidated,
				mutations: mutations,
				getElement: id =>
					idToElement.Value.TryGetValue(id).Or(Element.Empty));

			Optional<XElement> xElementForSaving = Optional.None<XElement>();

			var allElements = root.Subtree().Replay(1);

			var source = new ReplaySubject<SourceFragment>(1);

			return new LiveDocument
			{
				_garbage = Disposable.Combine(

					// Save on internal changes
					invalidated
						.Select(_ =>
						{
							if (xElementForSaving.HasValue)
							{
								var sourceFragment = SourceFragment.FromXml(xElementForSaving.Value);
								source.OnNext(sourceFragment);
								return Optional.Some(sourceFragment);
							}
							return Optional.None();
						})
						.NotNone()
						.Throttle(TimeSpan.FromSeconds(0.5), scheduler)
						.Select(sourceFragment =>
							Observable.FromAsync(async () =>
								await fileOnDisk.Save(sourceFragment.ToBytes())))
						.Concat()
						.Subscribe(),

					// Load on external changes
					fileOnDisk.ExternalChanges
						.ObserveOn(Application.MainThread)
						.Subscribe(reload =>
							{
								var sourceFragment = SourceFragment.FromBytes(reload);
								source.OnNext(sourceFragment);

								try
								{
									var simulatorWasFaulted = parsingErrors.Value.HasValue;

									var newDocument = sourceFragment.ToXml();
									parsingErrors.OnNext(Optional.None());

									xElementForSaving = Optional.None();
									Console.WriteLine("Reloading " + path + " from disk...");
									root.UpdateFrom(newDocument); // no known reasons to throw
									xElementForSaving = Optional.Some(newDocument);

									// hack to clear errors from the simulator,
									// since UpdateFrom() doesn't know that the simulator
									// have failed and will try to emit incremental updates
									if (simulatorWasFaulted)
										mutations.OnNext(new ReifyRequired());

									NagScreen.Update();
								}
								catch (Exception e)
								{
									parsingErrors.OnNext(e);

									// hack to get errors from the simulator
									mutations.OnNext(new ReifyRequired());
								}
							}),

					// Share subscription to Eleements
					allElements.Connect(),

					// Dispose fileOnDisk when disposed
					fileOnDisk),

				FilePath = Observable.Return(path),

				Errors = fileOnDisk.Errors.Or(parsingErrors),

				SimulatorIdPrefix = path.NativePath,

				Source = source,

				Root = root,
				Elements = allElements,

				_root = root,
				_path = path,
				_idToElement = idToElement,
			};
		}

		IDisposable _garbage;

		LiveElement _root;
		AbsoluteFilePath _path;
		BehaviorSubject<Dictionary<ObjectIdentifier, IElement>> _idToElement;

		public void UpdateSimulatorIds()
		{
			var dict = _idToElement.Value;
			foreach (var kvp in dict.ToArray())
			{
				if (kvp.Key.Document == SimulatorIdPrefix)
					dict.Remove(kvp.Key);
			}
			_root.UpdateElementIds(_path.NativePath, 0, dict);
			_idToElement.OnNext(dict);
		}

		public IObservable<Optional<Exception>> Errors { get; private set; }
		public IObservable<AbsoluteFilePath> FilePath { get; private set; }

		public string SimulatorIdPrefix { get; private set; }

		public IElement Root { get; private set; }

		public IObservable<IEnumerable<IElement>> Elements { get; private set; }

		public IObservable<SourceFragment> Source { get; private set; }

		public void Dispose()
		{
			_garbage.Dispose();
		}
	}
}
