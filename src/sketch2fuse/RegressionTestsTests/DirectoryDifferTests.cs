using System;
using System.IO;
using NUnit.Framework;
using RegressionTests;

namespace RegressionTestsTests
{
	public class DirectoryDifferTests
	{
		private static readonly string TestDir = Path.Combine(Path.GetTempPath() + Guid.NewGuid());
		private static readonly string OldDir = Path.Combine(TestDir, "Old");
		private static readonly string NewDir = Path.Combine(TestDir, "New");

		[SetUp]
		public void SetUp()
		{
			Directory.CreateDirectory(TestDir);
			Directory.CreateDirectory(OldDir);
			Directory.CreateDirectory(NewDir);
		}

		[TearDown]
		public void TearDown()
		{
			Directory.Delete(TestDir, true);
		}

		[Test]
		public void TwoEmptyDirs()
		{
			Assert.AreEqual(0, DirectoryDiffer.Diff(OldDir, NewDir).Count);
		}

		[Test]
		public void FailsOnUnexpectedSubDirectories()
		{
			//The only subdirectories created in the ux writer is a potential folder for images. Throw if we
			//find any other subfolders since we don't support that yet in the differ
			//Can add it later if we start using it in the ux writer
			//This test is here to let us know if the ux writer started doing that, so we notice that we have to start supporting it in the differ too.
			var subDir = Path.Combine(NewDir, "foo");
			Directory.CreateDirectory(subDir);
			File.WriteAllText(Path.Combine(OldDir, "justToNotTriggerEmptyDirException"), "");
			Assert.That(() => DirectoryDiffer.Diff(OldDir, NewDir),
				Throws.TypeOf<Exception>().With.Message
					.EqualTo("A subdirectory exists in '" + NewDir + "', we don't support that."));
			Assert.That(() => DirectoryDiffer.Diff(NewDir, OldDir),
				Throws.TypeOf<Exception>().With.Message
					.EqualTo("A subdirectory exists in '" + NewDir + "', we don't support that."));
		}

		[Test]
		public void DoNotFailOnExpectedSubDirectories()
		{
			//Ignore subfolder called `images` in the differ. We probably don't want to diff these anyway
			//Can add it later if we start using it in the ux writer
			//This test is here to let us know if the ux writer started doing that, so we notice that we have to start supporting it in the differ too.
			var subDir = Path.Combine(NewDir, "images");
			Directory.CreateDirectory(subDir);
			File.WriteAllText(Path.Combine(OldDir, "justToNotTriggerEmptyDirException"), "");
			Assert.DoesNotThrow(() => DirectoryDiffer.Diff(OldDir, NewDir));
			Assert.DoesNotThrow(() => DirectoryDiffer.Diff(NewDir, OldDir));
		}

		[Test]
		public void FileRemoved()
		{
			var oldFile = Path.Combine(OldDir, "oldfile");
			File.WriteAllText(oldFile, "foo\nbar");

			var diff = DirectoryDiffer.Diff(OldDir, NewDir);

			var expected = "-foo\n-bar\n";
			Assert.That(diff.Count, Is.EqualTo(1));
			Assert.That(diff[0].Diff, Is.EqualTo(expected));
			Assert.That(diff[0].DiffType, Is.EqualTo(DiffType.Deleted));
			Assert.That(diff[0].OldFile, Is.EqualTo(oldFile));
			Assert.That(diff[0].NewFile, Is.EqualTo(Path.Combine(NewDir, "oldfile")));
		}

		[Test]
		public void FileAdded()
		{
			var newFile = Path.Combine(NewDir, "newfile");
			File.WriteAllText(newFile, "foo\nbar");

			var diff = DirectoryDiffer.Diff(OldDir, NewDir);

			var expected = "+foo\n+bar\n";
			Assert.That(diff.Count, Is.EqualTo(1));
			Assert.That(diff[0].Diff, Is.EqualTo(expected));
			Assert.That(diff[0].DiffType, Is.EqualTo(DiffType.Added));
			Assert.That(diff[0].OldFile, Is.EqualTo(Path.Combine(OldDir, "newfile")));
			Assert.That(diff[0].NewFile, Is.EqualTo(newFile));
		}

		[Test]
		public void FileModified()
		{
			var oldFile = Path.Combine(OldDir, "file");
			var newFile = Path.Combine(NewDir, "file");
			File.WriteAllText(oldFile, "1\n2\n3\n4");
			File.WriteAllText(newFile, "1\na\nb\n4");

			var diff = DirectoryDiffer.Diff(OldDir, NewDir);

			var expected = " 1\n-2\n-3\n+a\n+b\n 4\n";
			Assert.That(diff.Count, Is.EqualTo(1));
			Assert.That(diff[0].Diff, Is.EqualTo(expected));
			Assert.That(diff[0].DiffType, Is.EqualTo(DiffType.Modified));
			Assert.That(diff[0].OldFile, Is.EqualTo(oldFile));
			Assert.That(diff[0].NewFile, Is.EqualTo(newFile));
		}

		[Test]
		public void NoChanges()
		{
			File.WriteAllText(Path.Combine(OldDir, "file"), "1\n2\n3\n4");
			File.WriteAllText(Path.Combine(NewDir, "file"), "1\n2\n3\n4");

			var diff = DirectoryDiffer.Diff(OldDir, NewDir);

			Assert.That(diff.Count, Is.EqualTo(0));
		}

		[Test]
		public void MultipleChanges()
		{
			var oldFile = Path.Combine(OldDir, "oldfile");
			var newFile = Path.Combine(NewDir, "newfile");
			var oldChangedFile = Path.Combine(OldDir, "changedfile");
			var newChangedFile = Path.Combine(NewDir, "changedfile");
			File.WriteAllText(oldFile, "foo\nbar");
			File.WriteAllText(newFile, "foo\nbar");
			File.WriteAllText(oldChangedFile, "1\n2\n3\n4");
			File.WriteAllText(newChangedFile, "1\na\nb\n4");

			var diff = DirectoryDiffer.Diff(OldDir, NewDir);

			Assert.That(diff.Count, Is.EqualTo(3));

			Assert.That(diff[0].DiffType, Is.EqualTo(DiffType.Modified));
			Assert.That(diff[0].OldFile, Is.EqualTo(oldChangedFile));
			Assert.That(diff[0].NewFile, Is.EqualTo(newChangedFile));
			Assert.That(diff[0].Diff, Is.EqualTo(" 1\n-2\n-3\n+a\n+b\n 4\n"));

			Assert.That(diff[1].DiffType, Is.EqualTo(DiffType.Deleted));
			Assert.That(diff[1].OldFile, Is.EqualTo(oldFile));
			Assert.That(diff[1].NewFile, Is.EqualTo(Path.Combine(NewDir, "oldfile")));
			Assert.That(diff[1].Diff, Is.EqualTo("-foo\n-bar\n"));

			Assert.That(diff[2].DiffType, Is.EqualTo(DiffType.Added));
			Assert.That(diff[2].OldFile, Is.EqualTo(Path.Combine(OldDir, "newfile")));
			Assert.That(diff[2].NewFile, Is.EqualTo(newFile));
			Assert.That(diff[2].Diff, Is.EqualTo("+foo\n+bar\n"));
		}

		[Test]
		public void ReferenceDirDoesntExist()
		{
			Directory.Delete(OldDir);

			var newFile = Path.Combine(NewDir, "newfile");
			File.WriteAllText(newFile, "foo\nbar");

			var diff = DirectoryDiffer.Diff(OldDir, NewDir);

			var expected = "+foo\n+bar\n";
			Assert.That(diff.Count, Is.EqualTo(1));
			Assert.That(diff[0].Diff, Is.EqualTo(expected));
			Assert.That(diff[0].DiffType, Is.EqualTo(DiffType.Added));
			Assert.That(diff[0].OldFile, Is.EqualTo(Path.Combine(OldDir, "newfile")));
			Assert.That(diff[0].NewFile, Is.EqualTo(newFile));
		}

		[Test]
		public void OutputDirDoesntExist()
		{
			Directory.Delete(NewDir);

			var oldFile = Path.Combine(OldDir, "oldfile");
			File.WriteAllText(oldFile, "foo\nbar");

			var diff = DirectoryDiffer.Diff(OldDir, NewDir);

			var expected = "-foo\n-bar\n";
			Assert.That(diff.Count, Is.EqualTo(1));
			Assert.That(diff[0].Diff, Is.EqualTo(expected));
			Assert.That(diff[0].DiffType, Is.EqualTo(DiffType.Deleted));
			Assert.That(diff[0].OldFile, Is.EqualTo(oldFile));
			Assert.That(diff[0].NewFile, Is.EqualTo(Path.Combine(NewDir, "oldfile")));
		}
	}
}
