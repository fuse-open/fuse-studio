namespace Outracks.DomainModel.Tests
{
	/*
	[TestFixture]
	public class ProjectAddRemoveCornerCaseTests
	{
		readonly AbsoluteDirectoryPath _projectDir = AbsoluteDirectoryPath.Parse("C:\\MyProject");
		readonly AbsoluteFilePath _projectPath;

		public ProjectAddRemoveCornerCaseTests()
		{
			_projectPath = _projectDir / new FileName("some.unoproj");
		}

		[Test]
		public void RemoveFolderContainingFiles()
		{
			var folderPath = _projectDir / new DirectoryName("ThingsILike");
			var filePath = folderPath / new FileName("mystuff.uno");
			var proj = new Project(_projectPath, new[] { filePath });

			Assert.AreEqual(new Project(_projectPath), proj.RemoveFolder(folderPath));
		}

		[Test]
		public void RemoveLastFileInFolder()
		{
			var folderPath = _projectDir / new DirectoryName("ThingsILike");
			var filePath = folderPath / new FileName("mystuff.uno");
			var proj = new Project(_projectPath, new[] { filePath });

			Assert.AreEqual(
				new Project(
					_projectPath,
					null,
					new[] { folderPath }),
				proj.RemoveFile(filePath));
		}

		[Test]
		public void RemoveLastFileInProjectAndDontCreateFolder()
		{
			var filePath = _projectDir / new FileName("I'm gonna win the gathering with this demo unless jake makes a snes demo. plus he's handsome.uno");
			var proj = new Project(_projectPath, new[] { filePath });

			Assert.AreEqual(
				new Project(_projectPath),
				proj.RemoveFile(filePath));
		}

		[Test]
		public void RemoveFolderContainingOnlySingleEmptyFolder()
		{
			var folderPath = _projectDir / new DirectoryName("ThingsILike");
			var nestedFolderPath = folderPath / new DirectoryName("NestedThingsILike");
			var proj = new Project(_projectPath, new AbsoluteFilePath[0], new[] { nestedFolderPath });

			Assert.AreEqual(new Project(_projectPath), proj.RemoveFolder(folderPath));
		}
	}
	 * */
}
