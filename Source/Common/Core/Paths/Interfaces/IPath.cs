namespace Outracks.IO
{
	public interface IPath : IMatchTypes<IFilePath, IDirectoryPath>, IMatchTypes<IAbsolutePath, IRelativePath>
	{
	}

}