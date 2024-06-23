namespace Outracks.DomainModel.Tests
{
	/*
	[TestFixture]
	public class ProjectMoveTests : ProjectTest
	{
		[Test]
		public void MoveFile()
		{
			var folderAPath = _projectDir / new DirectoryName("A");
			var folderBPath = _projectDir / new DirectoryName("B");
			var fileName = new FileName("mystuff.uno");
			var srcFilePath = folderAPath / fileName;
			var dstFilePath = folderBPath / fileName;
			var proj = new Project(_projectPath, new[] { srcFilePath }, new[] { folderBPath });

			Assert.AreEqual(
				new Project(_projectPath, new[] { dstFilePath }, new[] { folderAPath }),
				proj.MoveFile(srcFilePath, dstFilePath));
		}

		[Test]
		public void MoveEmptyFolder()
		{
			var folderAPath = _projectDir / new DirectoryName("A");
			var folderBPath = _projectDir / new DirectoryName("B");
			var proj = new Project(_projectPath, null, new[] { folderAPath });

			Assert.AreEqual(
				new Project(_projectPath, null, new[] { folderBPath }),
				proj.MoveFolder(folderAPath, folderBPath));
		}

		[Test]
		public void MoveFolderWithContents()
		{
			var folderAPath = _projectDir / new DirectoryName("A");
			var folderBPath = _projectDir / new DirectoryName("B");
			var fileName = new FileName("bestdemoever.uno");
			var srcFilePath = folderAPath / fileName;
			var dstFilePath = folderBPath / fileName;
			var proj = new Project(_projectPath, new[] { srcFilePath });

			Assert.AreEqual(
				new Project(_projectPath, new[] { dstFilePath }),
				proj.MoveFolder(folderAPath, folderBPath));
		}

		[Test]
		public void MoveFolderWithNestedContents()
		{
			var folderAPath = _projectDir / new DirectoryName("A");
			var folderBPath = _projectDir / new DirectoryName("B");
			var folderCName = new DirectoryName("C");
			var srcFolderCPath = folderAPath / folderCName;
			var dstFolderCPath = folderBPath / folderCName;
			var fileName1 = new FileName("file1.uno");
			var fileName2 = new FileName("file2.uno");
			var srcFilePath1 = folderAPath / fileName1;
			var dstFilePath1 = folderBPath / fileName1;
			var srcFilePath2 = srcFolderCPath / fileName2;
			var dstFilePath2 = dstFolderCPath / fileName2;
			var proj = new Project(_projectPath, new[] { srcFilePath1, srcFilePath2 });

			Assert.AreEqual(
				new Project(_projectPath).With(files: ImmutableHashSet.Create(dstFilePath1, dstFilePath2)),
				proj.MoveFolder(folderAPath, folderBPath));
		}

		[Test]
		public void MoveFolderWithNestedEmptyFolders()
		{
			var folderAPath = _projectDir / new DirectoryName("A");
			var folderBPath = _projectDir / new DirectoryName("B");
			var folderCName = new DirectoryName("C");
			var folderDName = new DirectoryName("D");
			var srcFolderCPath = folderAPath / folderCName;
			var dstFolderCPath = folderBPath / folderCName;
			var srcFolderDPath = folderAPath / folderDName;
			var dstFolderDPath = folderBPath / folderDName;
			var fileName1 = new FileName("file1.uno");
			var fileName2 = new FileName("file2.uno");
			var srcFilePath1 = folderAPath / fileName1;
			var dstFilePath1 = folderBPath / fileName1;
			var srcFilePath2 = srcFolderCPath / fileName2;
			var dstFilePath2 = dstFolderCPath / fileName2;
			var proj =
				new Project(
					_projectPath,
					new[] { srcFilePath1, srcFilePath2 },
					new[] { srcFolderDPath });

			Assert.AreEqual(
				new Project(
					_projectPath,
					new[] { dstFilePath1, dstFilePath2 },
					new[] { dstFolderDPath }),
				proj.MoveFolder(folderAPath, folderBPath));
		}

	}*/
}
