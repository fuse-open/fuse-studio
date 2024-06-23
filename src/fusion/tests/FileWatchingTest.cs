using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Outracks.Diagnostics;

namespace Outracks.IO.Tests
{
	// This is an exception to the no IO rule when doing unit tests.
	public class FileWatchingTest
	{
		readonly IFileSystem _fs = new Shell();
		AbsoluteDirectoryPath _dir;
		AbsoluteFilePath _file;

		[SetUp]
		public void SetUp()
		{
			_dir = DirectoryPath.GetCurrentDirectory() / new DirectoryName(Guid.NewGuid().ToString());
			_file = _dir / new FileName("foobar.txt");
		}

		[TearDown]
		public void TearDown()
		{
			for (int i = 0; i < 5; i++)
			{
				try
				{
					if (Directory.Exists(_dir.NativePath))
						Directory.Delete(_dir.NativePath, true);
					return;
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception);
					Thread.Sleep(100);
				}
			}
		}

		[Test]
		public async Task FileCreated()
		{
			_fs.Create(_dir);
			var unit = _fs.Watch(_file).FirstAsync().ToTask();

			_fs.Create(_file).Dispose();

			Assert.AreEqual(Unit.Default, await unit.TimeoutAfter(TimeSpan.FromSeconds(2)), "Did not get file created event!");
		}

		[Test]
		public async Task FileChanged()
		{
			_fs.Create(_dir);
			using (var stream = _fs.Create(_file))
			{
				await stream.FlushAsync();

				var unit = _fs.Watch(_file).FirstAsync().ToTask();

				using (var streamWriter = new StreamWriter(stream))
				{
					streamWriter.Write("Foo");
				}

				Assert.AreEqual(Unit.Default, await unit.TimeoutAfter(TimeSpan.FromSeconds(0.5)), "Did not get file changed event!");
			}
		}

		[Test]
		public async Task FileDeleted()
		{
			_fs.Create(_dir);
			_fs.Create(_file).Dispose();

			var unit = _fs.Watch(_file).FirstAsync().ToTask();

			_fs.Delete(_file);

			Assert.AreEqual(Unit.Default, await unit.TimeoutAfter(TimeSpan.FromSeconds(0.5)), "Did not get file deleted event!");
		}

		[Test]
		public async Task DirDeleted()
		{
			_fs.Create(_dir);
			_fs.Create(_file).Dispose();

			var watcher = _fs.Watch(_file);

			var deletedEvent = watcher.FirstAsync().ToTask();
			_fs.Delete(_dir);
			Assert.AreEqual(Unit.Default, await deletedEvent.TimeoutAfter(TimeSpan.FromSeconds(2)), "Did not get file deleted event!");
		}

		[Test]
		public async Task FileRenamed()
		{
			_fs.Create(_dir);
			_fs.Create(_file).Dispose();
			var tmpFile = _file.Rename(Guid.NewGuid() + ".txt");

			var renamed = _fs.Watch(_file).Take(1).ToTask();
			_fs.Move(_file, tmpFile);
			Assert.AreEqual(Unit.Default, await renamed.TimeoutAfter(TimeSpan.FromSeconds(2)), "Did not get file moved event!");

			var renamedBack = _fs.Watch(_file).Take(1).ToTask();
			_fs.Move(tmpFile, _file);
			Assert.AreEqual(Unit.Default, await renamedBack.TimeoutAfter(TimeSpan.FromSeconds(2)), "Did not get file moved event!");
		}

		[Test]
		[Ignore("Under investigation, seems to have passed by mistake randomly")]
		public async Task WithCatchAndRetry()
		{
			_fs.Create(_dir);
			var file = _fs.Watch(_file).StartWith(Unit.Default).CatchAndRetry(TimeSpan.Zero).Publish();

			file.Connect();

			var created = file.FirstAsync().ToTask();
			_fs.Create(_file);
			Assert.AreEqual(Unit.Default, await created.TimeoutAfter(TimeSpan.FromSeconds(0.5)), "Did not get file created event!");

			try
			{
				_fs.Delete(_dir); // this causes errors in the watcher, but we're using CatchAndRetry so we should be good

				var createdAgain = file.FirstAsync().ToTask();
				_fs.Create(_dir);
				Assert.AreEqual(Unit.Default, await createdAgain.TimeoutAfter(TimeSpan.FromSeconds(0.5)), "Did not get file created event!");
			}
			catch (Exception)
			{
				Assert.AreEqual(OS.Windows, Platform.OperatingSystem, "Deleting watched folder is assumed only to throw on Windows");
			}
		}

	}
}