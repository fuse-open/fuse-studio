namespace Outracks.IO
{
	public interface IShell : IFileSystem
	{
        string Name { get; }

		void OpenWithDefaultApplication(AbsoluteFilePath path);
		void OpenFolder(AbsoluteDirectoryPath path);
		void ShowInFolder(AbsoluteFilePath path);
		void OpenTerminal(AbsoluteDirectoryPath containingDirectory);

		void SetPermission(AbsoluteFilePath file, FileSystemPermission permission, FileSystemGroup group);
		void SetPermission(AbsoluteDirectoryPath dir, FileSystemPermission permission, FileSystemGroup group, bool recursive = false);
	}
}
