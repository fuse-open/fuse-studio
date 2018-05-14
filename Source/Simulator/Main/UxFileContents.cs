using System.IO;

namespace Outracks.Simulator.Protocol
{
	public class UxFileContents
	{
		public string Path { get; set; }
		public string Contents { get; set; }

		public static void Write(UxFileContents str, BinaryWriter writer)
		{
			writer.Write(str.Path);
			writer.Write(str.Contents);
		}

		public static UxFileContents Read(BinaryReader reader)
		{
			return new UxFileContents
			{
				Path = reader.ReadString(),
				Contents = reader.ReadString(),
			};
		}

	}
}