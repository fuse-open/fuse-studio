using System;
using System.IO;
using NUnit.Framework;
using Outracks.IO;

namespace Outracks.Tests.FileOperations
{
	public class CopyTests
	{
		AbsoluteDirectoryPath _dir;
		Shell _shell = new Shell();


		[SetUp]
		public void SetUp()
		{
			_dir = DirectoryPath.GetCurrentDirectory() / new DirectoryName(Guid.NewGuid().ToString());
			Directory.CreateDirectory(_dir.NativePath);
		}

		[TearDown]
		public void TearDown()
		{
			if (Directory.Exists(_dir.NativePath))
			{
				//Commented out due to #4225 (It makes FileWatchingTest.DirDeleted fail for some weird reason)
				//Directory.Delete(_dir.NativePath, true);
			}
		}

		[Test]
		public void CopyDirectory()
		{
			Directory.CreateDirectory((_dir / "source" / "subdir").NativePath);
			File.WriteAllText((_dir / "source" / "subdir" / "file.txt").NativePath, "Hack the planet!");
			_shell.Copy(_dir / "source", _dir / "target");
			Assert.AreEqual("Hack the planet!", File.ReadAllText((_dir / "target" / "subdir" / "file.txt").NativePath));
		}

	}
}
