namespace Outracks.IO
{
	public enum FileSystemEvent
	{
		Removed,
		Renamed,
		Changed,
		Created,
	}

	public class FileSystemEventData
	{
		public readonly AbsoluteFilePath File;
		public readonly FileSystemEvent Event;
		public readonly Optional<AbsoluteFilePath> OldFile;

		public FileSystemEventData(
			AbsoluteFilePath file, 
			FileSystemEvent evt, 
			Optional<AbsoluteFilePath> oldFile = default(Optional<AbsoluteFilePath>))
		{
			File = file;
			Event = evt;
			OldFile = oldFile;
		}
	}
}