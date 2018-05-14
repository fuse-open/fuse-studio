using System.IO;

namespace Outracks.IO
{
	public partial class AbsoluteFilePath
	{
		public static AbsoluteFilePath Read(BinaryReader reader)
		{
			return Parse(reader.ReadString());
		}

		public static void Write(BinaryWriter writer, AbsoluteFilePath path)
		{
			writer.Write(path.NativePath);
		}
	}

	public partial class AbsoluteDirectoryPath
	{
		public static AbsoluteDirectoryPath Read(BinaryReader reader)
		{
			return Parse(reader.ReadString());
		}

		public static void Write(BinaryWriter writer, AbsoluteDirectoryPath path)
		{
			writer.Write(path.NativePath);
		}
	}
}
