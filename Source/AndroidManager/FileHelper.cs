using Outracks.IO;

namespace Outracks.AndroidManager
{
	public static class FileHelper
	{
		public static void CreateEmpty(this IFileSystem fileSystem, AbsoluteFilePath filePath)
		{
			fileSystem.Create(filePath).Dispose();
		}
	}
}