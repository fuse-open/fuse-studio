namespace Outracks.IO
{
	public interface IDirectoryPath : IPath, IMatchTypes<AbsoluteDirectoryPath, RelativeDirectoryPath>
	{
		/*
		IDirectoryPath Combine(RelativeDirectoryPath path);
		IFilePath Combine(RelativeFilePath path);

		IDirectoryPath Combine(string pathName);
		IFilePath Combine(FileName name);
		*/
	}
}