using System.IO;

namespace Outracks.Simulator.Protocol
{
	using IO;

	public class AssemblyBuilt : IBinaryMessage
	{
		public static string MessageType = "AssemblyBuilt";
		public string Type { get { return MessageType; } }
		public AbsoluteFilePath Assembly { get; set; }

		public AbsoluteDirectoryPath BuildDirectory { get { return Assembly.ContainingDirectory; } }

		public void WriteDataTo(BinaryWriter writer)
		{
			writer.Write(Assembly.NativePath);
		}

		public static AssemblyBuilt ReadDataFrom(BinaryReader reader)
		{
			return new AssemblyBuilt
			{
				Assembly = AbsoluteFilePath.Parse(reader.ReadString()),
			};
		}
	}
}