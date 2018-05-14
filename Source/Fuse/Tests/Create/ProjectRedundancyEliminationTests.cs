namespace Outracks.DomainModel.Tests
{
	/*
	[TestFixture]
	public class ProjectRedundancyEliminationTests
	{
		readonly AbsoluteDirectoryPath _projectDir = AbsoluteDirectoryPath.Parse("C:\\MyProject");
		readonly AbsoluteFilePath _projectPath;

		public ProjectRedundancyEliminationTests()
		{
			_projectPath = _projectDir / new FileName("some.unoproj");
		}

		[Test]
		public void AddExistingFile()
		{
			var filePath = _projectDir / new FileName("source.uno");
			var proj = new Project(
				_projectPath,
				new[] { filePath });

			Assert.AreEqual(proj.AddFile(filePath), proj);
		}

		[Test]
		public void AddExistingFolder()
		{
			var folderPath = _projectDir / new DirectoryName("ImEmpty");
			var proj = new Project(
				_projectPath,
				null,
				new[] { folderPath });

			Assert.AreEqual(proj.AddFolder(folderPath), proj);
		}

		[Test]
		public void AddRedundantFolder()
		{
			var folderPath = _projectDir / new DirectoryName("Stuff");
			var proj = new Project(
				_projectPath,
				new[] { folderPath / new FileName("awesomebindings.uxl") });

			Assert.AreEqual(proj.AddFolder(folderPath), proj);
		}

		[Test]
		public void AddAFileRemoveAFolder()
		{
			var folderPath = _projectDir / new DirectoryName("Stuff");
			var filePath = folderPath / new FileName("awesomebindings.uxl");

			Assert.AreEqual(
				new Project(
					_projectPath,
					new[] { filePath }),
				new Project(
					_projectPath,
					null,
					new[] { folderPath })
				.AddFile(filePath));
		}
	}*/
}
