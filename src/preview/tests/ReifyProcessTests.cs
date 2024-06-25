using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using Microsoft.Reactive.Testing;
using NSubstitute;
using NUnit.Framework;
using Outracks;
using Outracks.IO;
using Outracks.Simulator.Bytecode;

namespace Fuse.Preview.Tests
{
	[TestFixture]
	class ReifyProcessTests : ReactiveTest
	{
		AssetsWatcher CreateAssetsWatcher(TestScheduler scheduler, Recorded<Notification<byte[]>> []fileChanges)
		{
			var fileSystem = Substitute.For<IFileSystem>();
			SetFilesChanges(fileSystem, Optional.None(), fileChanges, scheduler);
			return new AssetsWatcher(fileSystem, Observable.Return(DirectoryPath.GetTempPath()), scheduler);
		}

		static Recorded<Notification<byte[]>>[] CreateFileChangesForDiff()
		{
			return new[]
			{
				OnNext(0, new byte[]{ }),
				OnNext(TimeSpan.FromSeconds(1).Ticks, Encoding.UTF8.GetBytes("let foo = 0;")),
				OnNext(TimeSpan.FromSeconds(2).Ticks, Encoding.UTF8.GetBytes("let foo = 0;")),
				OnNext(TimeSpan.FromSeconds(3).Ticks, Encoding.UTF8.GetBytes("")),
				OnNext(TimeSpan.FromSeconds(4).Ticks, Encoding.UTF8.GetBytes("let foo = 0;"))
			};
		}

		[Test]
		public void TestBundleFileDiffing()
		{
			var scheduler = new TestScheduler();
			var assetsWatcher = CreateAssetsWatcher(scheduler, CreateFileChangesForDiff());

			var path = AbsoluteFilePath.Parse("/c/Foo.png");

			var whatHappened = assetsWatcher.UpdateChangedBundleFiles(Observable.Return(ImmutableHashSet.Create(path))).Check();
			scheduler.Start();

			Assert.AreEqual(4, whatHappened.Results.Count, "Seems like our file diffing routine is broken, got incorrect number of file changed events.");
			Assert.IsNull(whatHappened.Error, "Did not expect any errors in the observable stream.");
		}

		[Test]
		public void TestDependencyDiffing()
		{
			var scheduler = new TestScheduler();
			var assetsWatcher = CreateAssetsWatcher(scheduler, CreateFileChangesForDiff());

			var path = AbsoluteFilePath.Parse("/c/Foo.png");

			var whatHappened = assetsWatcher.UpdateChangedDependencies(
				Observable.Return(ImmutableHashSet.Create(new ProjectDependency(path.NativePath, "Foo")))).Check();

			scheduler.Start();

			Assert.AreEqual(4, whatHappened.Results.Count, "Seems like our file diffing routine is broken, got incorrect number of file changed events.");
			Assert.IsNull(whatHappened.Error, "Did not expect any errors in the observable stream.");
		}

		[Test]
		public void TestJsFileDiffing()
		{
			var scheduler = new TestScheduler();
			using(var assetsWatcher = CreateAssetsWatcher(scheduler, CreateFileChangesForDiff()))
			{
				var path = AbsoluteFilePath.Parse("/c/Foo.js");

				var whatHappened = assetsWatcher.UpdateChangedFuseJsFiles(
					Observable.Return(ImmutableHashSet.Create(path))).Check();

				scheduler.Start();

				Assert.AreEqual(4, whatHappened.Results.Count, "Seems like our file diffing routine is broken, got incorrect number of file changed events.");
				Assert.IsNull(whatHappened.Error, "Did not expect any errors in the observable stream.");
			}
		}

		[Test]
		public void TestThrottle()
		{
			var scheduler = new TestScheduler();
			var assetsWatcher = CreateAssetsWatcher(scheduler, new[]
			{
				OnNext(TimeSpan.FromMilliseconds(0).Ticks, Encoding.UTF8.GetBytes("a")),
				OnNext(TimeSpan.FromMilliseconds(1).Ticks, Encoding.UTF8.GetBytes("bb")),
				OnNext(TimeSpan.FromMilliseconds(2).Ticks, Encoding.UTF8.GetBytes("ccc")),
				OnNext(TimeSpan.FromMilliseconds(2000).Ticks, Encoding.UTF8.GetBytes("ccc")),
			});

			var path = AbsoluteFilePath.Parse("/c/Foo.js");

			var whatHappened = assetsWatcher.UpdateChangedBundleFiles(Observable.Return(ImmutableHashSet.Create(path))).Check();

			scheduler.Start();

			Assert.AreEqual(2, whatHappened.Results.Count, "Seems like our throttle is broken, got incorrect number of file changed events.\n{0}", string.Join("\n", whatHappened.Results.Select(e => e.CoalesceKey + ": " + e.BlobData.ToString())));
			Assert.IsNull(whatHappened.Error, "Did not expect any errors in the observable stream.");
		}

		[Test]
		public void TestAddingMoreFiles()
		{
			var scheduler = new TestScheduler();

			var foo = AbsoluteFilePath.Parse("/c/Foo.js");
			var bar = AbsoluteFilePath.Parse("/c/Bar.js");
			var lol = AbsoluteFilePath.Parse("/c/lol.js");

			var fileSystem = Substitute.For<IFileSystem>();
			SetFilesChanges(fileSystem, foo, new []
			{
				OnNext(0, Encoding.UTF8.GetBytes("let foo = 0;"))
			}, scheduler);
			SetFilesChanges(fileSystem, bar, new []
			{
				OnNext(0, Encoding.UTF8.GetBytes("let bar = 0;"))
			}, scheduler);
			SetFilesChanges(fileSystem, lol, new []
			{
				OnNext(0, Encoding.UTF8.GetBytes("let lol = 0;")),
				OnNext(TimeSpan.FromSeconds(1).Ticks, Encoding.UTF8.GetBytes("let lol = 1;"))
			}, scheduler);

			var assetsWatcher = new AssetsWatcher(fileSystem, Observable.Return(DirectoryPath.GetTempPath()), scheduler);

			var whatHappened = assetsWatcher.UpdateChangedBundleFiles(
				scheduler.CreateColdObservable(
					OnNext(TimeSpan.FromSeconds(0).Ticks, ImmutableHashSet.Create(foo)),
					OnNext(TimeSpan.FromSeconds(1).Ticks, ImmutableHashSet.Create(foo, bar)),
					OnNext(TimeSpan.FromSeconds(2).Ticks, ImmutableHashSet.Create(foo, bar, lol))
				)).Check();

			scheduler.Start();

			Assert.AreEqual(3, whatHappened.Results.Count, "Seems like adding more files doesn't work, got incorrect number of file changed events.\n{0}", string.Join("\n", whatHappened.Results.Select(e => e.CoalesceKey + ": " + e.BlobData.ToString())));
			Assert.IsNull(whatHappened.Error, "Did not expect any errors in the observable stream.");
		}

		[Test]
		public void TestAddingMoreFiles1()
		{
			var scheduler = new TestScheduler();

			var foo = AbsoluteFilePath.Parse("/c/Foo.js");
			var bar = AbsoluteFilePath.Parse("/c/Bar.js");
			var lol = AbsoluteFilePath.Parse("/c/lol.js");

			var fileSystem = Substitute.For<IFileSystem>();
			SetFilesChanges(fileSystem, foo, new[]
			{
				OnNext(0, Encoding.UTF8.GetBytes("let foo = 0;"))
			}, scheduler);
			SetFilesChanges(fileSystem, bar, new[]
			{
				OnNext(0, Encoding.UTF8.GetBytes("let bar = 0;"))
			}, scheduler);
			SetFilesChanges(fileSystem, lol, new[]
			{
				OnNext(0, Encoding.UTF8.GetBytes("let lol = 0;")),
				OnNext(TimeSpan.FromSeconds(3).Ticks, Encoding.UTF8.GetBytes("let lol = 1;"))
			}, scheduler);

			var assetsWatcher = new AssetsWatcher(fileSystem, Observable.Return(DirectoryPath.GetTempPath()), scheduler);

			var whatHappened = assetsWatcher.UpdateChangedBundleFiles(
				scheduler.CreateColdObservable(
					OnNext(TimeSpan.FromSeconds(0).Ticks, ImmutableHashSet.Create(foo)),
					OnNext(TimeSpan.FromSeconds(1).Ticks, ImmutableHashSet.Create(foo, bar)),
					OnNext(TimeSpan.FromSeconds(2).Ticks, ImmutableHashSet.Create(foo, bar, lol))
				)).Check();

			scheduler.Start();

			Assert.AreEqual(4, whatHappened.Results.Count, "Seems like adding more files doesn't work, got incorrect number of file changed events.\n{0}", string.Join("\n", whatHappened.Results.Select(e => e.CoalesceKey + ": " + e.BlobData.ToString())));
			Assert.IsNull(whatHappened.Error, "Did not expect any errors in the observable stream.");
		}

		[Test]
		public void TestAddingMoreAndRemovingFiles()
		{
			var scheduler = new TestScheduler();

			var foo = AbsoluteFilePath.Parse("/c/Foo.js");
			var bar = AbsoluteFilePath.Parse("/c/Bar.js");
			var lol = AbsoluteFilePath.Parse("/c/lol.js");

			var fileSystem = Substitute.For<IFileSystem>();
			SetFilesChanges(fileSystem, foo, new[]
			{
				OnNext(0, Encoding.UTF8.GetBytes("let foo = 0;")),
				OnNext(TimeSpan.FromSeconds(4).Ticks, Encoding.UTF8.GetBytes("let foo = 40;"))
			}, scheduler);
			SetFilesChanges(fileSystem, bar, new[]
			{
				OnNext(0, Encoding.UTF8.GetBytes("let bar = 0;")),
				OnNext(TimeSpan.FromSeconds(5).Ticks, Encoding.UTF8.GetBytes("let bar = 50;"))
			}, scheduler);
			SetFilesChanges(fileSystem, lol, new[]
			{
				OnNext(0, Encoding.UTF8.GetBytes("let lol = 0;")),
				OnNext(TimeSpan.FromSeconds(3).Ticks, Encoding.UTF8.GetBytes("let lol = 10;"))
			}, scheduler);

			var assetsWatcher = new AssetsWatcher(fileSystem, Observable.Return(DirectoryPath.GetTempPath()), scheduler);

			var whatHappened = assetsWatcher.UpdateChangedBundleFiles(
				scheduler.CreateColdObservable(
					OnNext(TimeSpan.FromSeconds(0).Ticks, ImmutableHashSet.Create(foo)),
					OnNext(TimeSpan.FromSeconds(1).Ticks, ImmutableHashSet.Create(foo, bar)),
					OnNext(TimeSpan.FromSeconds(2).Ticks, ImmutableHashSet.Create(foo, bar, lol)),
					OnNext(TimeSpan.FromSeconds(2.5).Ticks, ImmutableHashSet.Create(foo, bar)),
					OnNext(TimeSpan.FromSeconds(3).Ticks, ImmutableHashSet.Create(foo))
				)).Check();

			scheduler.Start();

			Assert.AreEqual(4, whatHappened.Results.Count, "Seems like adding and removing files doesn't work, got incorrect number of file changed events.\n{0}", string.Join("\n", whatHappened.Results.Select(e => e.CoalesceKey + ": " + e.BlobData.ToString())));
			Assert.IsNull(whatHappened.Error, "Did not expect any errors in the observable stream.");
		}

		[Test]
		public void TestAddingMoreAndRemovingFiles1()
		{
			var scheduler = new TestScheduler();

			var foo = AbsoluteFilePath.Parse("/c/Foo.js");
			var bar = AbsoluteFilePath.Parse("/c/Bar.js");
			var lol = AbsoluteFilePath.Parse("/c/lol.js");

			var fileSystem = Substitute.For<IFileSystem>();
			SetFilesChanges(fileSystem, foo, new[]
			{
				OnNext(0, Encoding.UTF8.GetBytes("let foo = 0;")),
				OnNext(TimeSpan.FromSeconds(4).Ticks, Encoding.UTF8.GetBytes("let foo = 40;")),
				OnNext(TimeSpan.FromSeconds(7.1).Ticks, Encoding.UTF8.GetBytes("let foo = 700;"))
			}, scheduler);
			SetFilesChanges(fileSystem, bar, new[]
			{
				OnNext(0, Encoding.UTF8.GetBytes("let bar = 0;")),
				OnNext(TimeSpan.FromSeconds(5).Ticks, Encoding.UTF8.GetBytes("let bar = 50;"))
			}, scheduler);
			SetFilesChanges(fileSystem, lol, new[]
			{
				OnNext(0, Encoding.UTF8.GetBytes("let lol = 0;")),
				OnNext(TimeSpan.FromSeconds(6).Ticks, Encoding.UTF8.GetBytes("let lol = 10;"))
			}, scheduler);

			var assetsWatcher = new AssetsWatcher(fileSystem, Observable.Return(DirectoryPath.GetTempPath()), scheduler);

			var whatHappened = assetsWatcher.UpdateChangedBundleFiles(
				scheduler.CreateColdObservable(
					OnNext(TimeSpan.FromSeconds(0).Ticks, ImmutableHashSet.Create(foo)),
					OnNext(TimeSpan.FromSeconds(1).Ticks, ImmutableHashSet.Create(foo, bar)),
					OnNext(TimeSpan.FromSeconds(2).Ticks, ImmutableHashSet.Create(foo, bar, lol)),
					OnNext(TimeSpan.FromSeconds(6).Ticks, ImmutableHashSet.Create(foo, bar)),
					OnNext(TimeSpan.FromSeconds(7).Ticks, ImmutableHashSet.Create(foo))
				)).Check();

			scheduler.Start();

			Assert.AreEqual(6, whatHappened.Results.Count, "Seems like adding and removing files doesn't work, got incorrect number of file changed events.\n{0}", string.Join("\n", whatHappened.Results.Select(e => e.CoalesceKey + ": " + e.BlobData.ToString())));
			Assert.IsNull(whatHappened.Error, "Did not expect any errors in the observable stream.");
		}

		static void SetFilesChanges(IFileSystem fileSystem, Optional<AbsoluteFilePath> path, Recorded<Notification<byte[]>>[] fileChanges, TestScheduler scheduler)
		{
			var hotChanges = fileChanges.Skip(1).ToArray();
			fileSystem.Watch(path.Or(Arg.Any<AbsoluteFilePath>()))
				.Returns(scheduler.CreateHotObservable(hotChanges).Select(_ => Unit.Default));

			fileSystem.OpenRead(path.Or(Arg.Any<AbsoluteFilePath>())).Returns(new MemoryStream(fileChanges.First().Value.Value));
			scheduler.CreateHotObservable(hotChanges)
				.Subscribe(bytes => fileSystem.OpenRead(path.Or(Arg.Any<AbsoluteFilePath>())).Returns(new MemoryStream(bytes)));
		}
	}
}