using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Uno.ProjectFormat;

namespace Outracks.Fuse.Live
{
	using IO;

	public class ProjectWatcher
	{
		public static ProjectWatcher Create(Fusion.IDocument document, IFileSystem fileSystem, IScheduler scheduler = null)
		{
			scheduler = scheduler ?? Scheduler.Default;

			var projectSnapshots =
				document.FilePath.NotNone()
					.Select(projectPath =>
						fileSystem
							.Watch(projectPath)
							.StartWith(Unit.Default)
							.CatchAndRetry(delay: TimeSpan.FromSeconds(1), scheduler: scheduler)
							.Throttle(TimeSpan.FromSeconds(1.0 / 30.0), scheduler)
							.SelectSome(_ => Try(() => Project.Load(projectPath.NativePath)))
							.CombineLatest(Observable.Interval(TimeSpan.FromSeconds(2), scheduler).StartWith(0))
							.SelectSome(projectAndTime => Try(() => CreateSnapshot(projectAndTime.Item1, projectPath.ContainingDirectory))))
					.Switch()
					.Replay(1).RefCount();

			return new ProjectWatcher()
			{
				Name = projectSnapshots.Select(p => p.Name).DistinctUntilChanged().Replay(1).RefCount(),
				UxFiles = projectSnapshots.Select(p => p.UxFiles).DistinctUntilSetChanged().Replay(1).RefCount(),
				BundleFiles = projectSnapshots.Select(p => p.BundleFiles).DistinctUntilSetChanged().Replay(1).RefCount(),
				FuseJsFiles = projectSnapshots.Select(p => p.FuseJsFiles).DistinctUntilSetChanged().Replay(1).RefCount(),
				BuildOutputDirectory = projectSnapshots.Select(p => p.BuildOutputDirectory).DistinctUntilChanged().Replay(1).RefCount(),
				PackageReferences = projectSnapshots.Select(p => p.PackageReferences).DistinctUntilSetChanged().Replay(1).RefCount(),
				ProjectReferences = projectSnapshots.Select(p => p.ProjectReferences).DistinctUntilSetChanged().Replay(1).RefCount()
            };
		}

		public static ProjectSnapshot CreateSnapshot(AbsoluteFilePath projectPath)
		{
			return CreateSnapshot(Project.Load(projectPath.NativePath), projectPath.ContainingDirectory);
		}

		public static ProjectSnapshot CreateSnapshot(Project project, AbsoluteDirectoryPath root)
		{
			project.InvalidateItems();
			return new ProjectSnapshot(
				name: project.Name,
				uxFiles:
					project.UXFiles
						.Select(f => root / RelativeFilePath.Parse(f.UnixPath))
						.ToImmutableHashSet(),
				bundleFiles:
					project.BundleFiles
						.Select(f => root / RelativeFilePath.Parse(f.UnixPath))
						.ToImmutableHashSet(),
				fuseJsFiles:
					project.FuseJSFiles
						.Select(f => root / RelativeFilePath.Parse(f.UnixPath))
						.ToImmutableHashSet(),
				buildOutputDirectory: 
					AbsoluteDirectoryPath.TryParse(project.BuildDirectory).Or(root / new DirectoryName("build")),
				packageReferences: project.PackageReferences.ToImmutableHashSet(),
				projectReferences: project.ProjectReferences.ToImmutableHashSet()
			);
		}

		static Optional<T> Try<T>(Func<T> func)
		{
			try
			{
				return func();
			}
			catch (Exception)
			{
				// TODO: report this somewhere
				return Optional.None();
			}
		}

		public IObservable<string> Name { get; private set; }

		public IObservable<IImmutableSet<AbsoluteFilePath>> UxFiles { get; private set; }

		public IObservable<IImmutableSet<AbsoluteFilePath>> BundleFiles { get; private set; }

		public IObservable<IImmutableSet<AbsoluteFilePath>> FuseJsFiles { get; private set; }

		public IObservable<AbsoluteDirectoryPath> BuildOutputDirectory { get; private set; }

		public IObservable<IImmutableSet<PackageReference>> PackageReferences { get; private set; }

		public IObservable<IImmutableSet<ProjectReference>> ProjectReferences { get; private set; }
			
		ProjectWatcher() { }

	}

	public sealed class ProjectSnapshot
	{
		public readonly string Name;

		public readonly IImmutableSet<AbsoluteFilePath> UxFiles;

		public readonly IImmutableSet<AbsoluteFilePath> BundleFiles;

		public readonly IImmutableSet<AbsoluteFilePath> FuseJsFiles;

		public readonly AbsoluteDirectoryPath BuildOutputDirectory;

		public readonly IImmutableSet<PackageReference> PackageReferences;

		public readonly IImmutableSet<ProjectReference> ProjectReferences;

		public ProjectSnapshot(string name, IImmutableSet<AbsoluteFilePath> uxFiles, IImmutableSet<AbsoluteFilePath> bundleFiles, IImmutableSet<AbsoluteFilePath> fuseJsFiles, AbsoluteDirectoryPath buildOutputDirectory, IImmutableSet<PackageReference> packageReferences, IImmutableSet<ProjectReference> projectReferences)
		{
			Name = name;
			UxFiles = uxFiles;
			BundleFiles = bundleFiles;
			FuseJsFiles = fuseJsFiles;
			BuildOutputDirectory = buildOutputDirectory;
			PackageReferences = packageReferences;
			ProjectReferences = projectReferences;
		}
	}

}