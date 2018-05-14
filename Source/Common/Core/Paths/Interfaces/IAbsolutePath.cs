namespace Outracks.IO
{
	public interface IAbsolutePath : IPath, IMatchTypes<AbsoluteFilePath, AbsoluteDirectoryPath>
	{
		string Name { get; } 

		string NativePath { get; } 

		AbsoluteDirectoryPath ContainingDirectory { get; }
	}
}