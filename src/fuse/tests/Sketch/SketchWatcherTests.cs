using System;
using System.Collections.Generic;
using System.IO;
using NSubstitute;
using NUnit.Framework;
using Outracks.Diagnostics;
using Outracks.Fuse.Studio;
using Outracks.IO;

namespace Outracks.Fuse.Protocol.Tests.Sketch
{
	class SketchWatcherTests
	{
		readonly AbsoluteFilePath _sketchListFile = DirectoryPath.GetTempPath() / new FileName(new Guid() + ".sketchFiles");
		readonly AbsoluteDirectoryPath _projectRoot = AbsoluteDirectoryPath.Parse(Platform.IsWindows ? @"c:\Project" : "/Project");

		[TearDown]
		public void TearDown()
		{
			try
			{
				File.Delete(_sketchListFile.NativePath);
			} catch (Exception) { }
		}

		[Test]
		public void SketchListFilePathGetsCorrectPath()
		{
			var projectFilePath = _projectRoot / new FileName("Project.unoproj");
			var sketchPath = SketchImportUtils.SketchListFilePath(projectFilePath);
			Assert.That(sketchPath, Is.EqualTo(_projectRoot / new FileName("Project.sketchFiles")));
		}

		[Test]
		public void SketchListFilePath_OnlyReplacesAtEnd()
		{
			var stupidProjectRoot = AbsoluteDirectoryPath.Parse(Platform.IsWindows ? @"c:\Project.unoproj" : "/Project.unoproj");
			var projectFilePath = stupidProjectRoot / new FileName("Project.unoproj");
			var sketchPath = SketchImportUtils.SketchListFilePath(projectFilePath);
			Assert.That(sketchPath, Is.EqualTo(stupidProjectRoot / new FileName("Project.sketchFiles")));
		}

		[Test]
		public void SketchListFilePath_FailsIfDoesntEndWithUnoProj()
		{
			var projectFilePath = _projectRoot / new FileName("Project.unicorn");
			Assert.That(() => SketchImportUtils.SketchListFilePath(projectFilePath), Throws.ArgumentException.With.Message.Contains("does not seem to be a .unoproj"));
		}

		[Test]
		public void SketchFilePaths_ParsesJson()
		{
			var absPathFile = Platform.IsWindows ? @"C:\\File2.sketch" : @"/File2.sketch";
			var relPathFile = Platform.IsWindows ? @"SketchFiles\\File1.sketch" : "SketchFiles/File1.sketch";
			var content = "[\"" + relPathFile + "\",\"" + absPathFile + "\"]";
			File.WriteAllText(_sketchListFile.NativePath, content);
			var report = Substitute.For<IReport>();
			var fs = Substitute.For<IFileSystem>();
			var sketchFiles = new SketchWatcher(report, fs).SketchFilePaths(_sketchListFile);

			var expected = new List<IFilePath>
			{
				RelativeFilePath.Parse(relPathFile),
				AbsoluteFilePath.Parse(absPathFile)
			};
			Assert.That(sketchFiles, Is.EqualTo(expected));
		}

		[Test]
		public void SketchFilePaths_MakePathsAbsolute()
		{
			var relPathFile = Platform.IsWindows ? @"SketchFiles\\File1.sketch" : "SketchFiles/File1.sketch";
			var absPathFile = Platform.IsWindows ? @"C:\\File2.sketch" : @"/File2.sketch";
			var content = "[\"" + relPathFile + "\",\"" + absPathFile + "\"]";
			File.WriteAllText(_sketchListFile.NativePath, content);


			var report = Substitute.For<IReport>();
			var fs = Substitute.For<IFileSystem>();
			var sketchFiles = SketchImportUtils.MakeAbsolute(
				new SketchWatcher(report, fs).SketchFilePaths(_sketchListFile),
				_sketchListFile.ContainingDirectory);

			var expected = new List<AbsoluteFilePath>
			{
				_sketchListFile.ContainingDirectory / "SketchFiles" / new FileName("File1.sketch"),
				AbsoluteFilePath.Parse(absPathFile)
			};
			Assert.That(sketchFiles, Is.EqualTo(expected));
		}

		[Test]
		public void SketchFilePaths_LogsAndReturnsEmptyOnError()
		{
			var report = Substitute.For<IReport>();
			var fs = Substitute.For<IFileSystem>();
			var paths = new SketchWatcher(report, fs).SketchFilePaths(FilePath.ParseAndMakeAbsolute("doesntExist.sketchFiles"));
			Assert.That(paths, Is.Empty);
			report.Received(1).Error(Arg.Any<string>());
		}
	}
}
