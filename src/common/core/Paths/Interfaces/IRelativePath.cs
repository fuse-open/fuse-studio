namespace Outracks.IO
{
	public interface IRelativePath : IPath, IMatchTypes<RelativeFilePath, RelativeDirectoryPath>
	{
		RelativeDirectoryPath BasePath { get; }
	}
}