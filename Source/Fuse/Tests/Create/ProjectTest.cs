using Outracks.IO;

namespace Outracks.DomainModel.Tests
{
	public abstract class ProjectTest
	{
		protected readonly AbsoluteDirectoryPath _projectDir = AbsoluteDirectoryPath.Parse("C:\\MyProject");
		protected readonly AbsoluteFilePath _projectPath;

		protected ProjectTest()
		{
			_projectPath = _projectDir / new FileName("some.unoproj");
		}
	}
}
