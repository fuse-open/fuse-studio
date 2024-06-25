using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Outracks.IO;

namespace Outracks.Fuse.Studio {
	static class SketchWatchExtensions
	{
		public static IObservable<AbsoluteFilePath> WhenChangedOrStarted(this IObservable<AbsoluteFilePath> path, IFileSystem fileSystem)
		{
			return path
				.Select(fileSystem.Watch)
				.CatchAndRetry(TimeSpan.FromSeconds(1))
				.Switch()
				.StartWith(Unit.Default)
				.CombineLatest(path, (_, p) => p)
				.Throttle(TimeSpan.FromSeconds(0.1));
		}

		public static IObservable<IEnumerable<AbsoluteFilePath>> WhenAnyChangedOrStarted(this IObservable<IEnumerable<AbsoluteFilePath>> paths, IFileSystem fileSystem)
		{
			return paths
				.Select(ps =>
					ps.Select(p =>
							fileSystem.Watch(p)
								.CatchAndRetry(TimeSpan.FromSeconds(1)))
						.Merge())
				.Switch()
				.StartWith(Unit.Default)
				.CombineLatest(paths, (_, p) => p)
				.Throttle(TimeSpan.FromSeconds(0.1));
		}
	}
}