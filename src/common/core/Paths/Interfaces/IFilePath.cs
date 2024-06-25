namespace Outracks.IO
{
	public interface IFilePath : IPath, IMatchTypes<AbsoluteFilePath, RelativeFilePath>
	{
		FileName Name { get; }
	}
}