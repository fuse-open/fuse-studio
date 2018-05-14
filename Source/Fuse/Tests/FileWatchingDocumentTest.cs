using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Outracks.Diagnostics;
using Outracks.Fuse.Live;
using Outracks.IO;

namespace Outracks.Fuse.Tests
{
	[TestFixture]
	public class FileWatchingDocumentTest
	{
		[Test]
		public async Task Foo()
		{
			var fs = new Shell();
			var path = DirectoryPath.GetCurrentDirectory() / new FileName(Guid.NewGuid().ToString());
			var doc = new FileWatchingDocument(fs, path);

			var externalChanges = doc.ExternalChanges.FirstAsync().ToTask();

			await doc.Save(new byte[] { 13, 37 });
		
			await doc.Contents.FirstAsync();
			Assert.ThrowsAsync<TimeoutException>(async () =>
				await externalChanges.ToObservable().Timeout(TimeSpan.FromSeconds(1)).FirstAsync());

			fs.ForceWriteText(path, "trettentretisju");

			await externalChanges;
		}

		[Test]
		[SuppressMessage("ReSharper", "AccessToModifiedClosure", Justification = "Using closures for capturing observable changes")]
		public async Task Retries_reading_of_file_when_getting_exception_until_successful()
		{
			var testScheduler = new HistoricalScheduler(DateTimeOffset.Now);

			var path = AbsoluteFilePath.Parse(Platform.OperatingSystem == OS.Windows ? @"c:\trololol\foobar.ux" : @"/trololol/foobar.ux");
			var fileNotifications = new Subject<Unit>();
			var fs = Substitute.For<IFileSystem>();
			bool fileLocked = false;
			string fileContent = "abcd";
			string readContent = null;
			int readErrorCounter = 0;
			int errorsDuringLoadingCounter = 0;
			var errorDuringLoading = Optional.None<Exception>();

			fs.Watch(path).Returns(fileNotifications);
			fs.OpenRead(path).Returns(_ =>
			{
				if (fileLocked)
				{
					readErrorCounter++;
					throw new IOException(string.Format("The process cannot access the file '{0}' because it is being used by another process.", path.NativePath));
				}
				return new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
			});

			// Start by opening FileWatchingDocument, reading unlocked file.
			var doc = new FileWatchingDocument(fs, path, testScheduler);

			doc.Contents.Subscribe(bytes => readContent = Encoding.UTF8.GetString(bytes));
			doc.ErrorsDuringLoading.Subscribe(err =>
			{
				if (err.HasValue)
					errorsDuringLoadingCounter++;
				errorDuringLoading = err;
			});

			fileNotifications.OnNext(Unit.Default);
			testScheduler.AdvanceBy(FileWatchingDocument.PreLogRetryInterval);

			Assert.That(readContent, Is.EqualTo(fileContent));

			// Now change and lock the file
			fileContent = "1337";
			fileLocked = true;
			fileNotifications.OnNext(Unit.Default);

			var slightlyBeforeErrorMessage = FileWatchingDocument.RetryErrorMessageDelay - FileWatchingDocument.PreLogRetryInterval;
			testScheduler.AdvanceBy(slightlyBeforeErrorMessage);
			Assert.That(readContent, Is.EqualTo("abcd"));
			Assert.That(readErrorCounter, Is.EqualTo(slightlyBeforeErrorMessage.TotalSeconds / FileWatchingDocument.PreLogRetryInterval.TotalSeconds).Within(1.0));

			// Wait another long interval
			readErrorCounter = 0;
			testScheduler.AdvanceBy(FileWatchingDocument.PostLogRetryInterval);

			// Check that we have the exception message ready now
			Assert.That(errorDuringLoading.HasValue, Is.True);
			Assert.That(errorDuringLoading.Value.Message, Is.EqualTo(string.Format(@"The process cannot access the file '{0}' because it is being used by another process. Retrying in background until problem is resolved.", path.NativePath)));
			Assert.That(errorsDuringLoadingCounter, Is.EqualTo(1));
			Assert.That(readErrorCounter, Is.GreaterThanOrEqualTo(1));

			// Check that error message is only output once
			readErrorCounter = 0;
			testScheduler.AdvanceBy(FileWatchingDocument.PostLogRetryInterval);
			Assert.That(readErrorCounter, Is.GreaterThanOrEqualTo(1));
			Assert.That(errorsDuringLoadingCounter, Is.EqualTo(1));

			fileLocked = false;
			testScheduler.AdvanceBy(TimeSpan.FromSeconds(FileWatchingDocument.PostLogRetryInterval.TotalSeconds * 1.1));
			Assert.That(errorDuringLoading.HasValue, Is.False);

			// HACK: I'm not completely sure why this is necesarry, I suspect it could be due to the use of Observable.DeferAsync?
			// Fix to work around HistoricalScheduler not playing nice with DeferAsync(?) 
			await Task.Delay(10);

			Assert.That(readContent, Is.EqualTo(fileContent));

		}
	}
}