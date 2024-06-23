using System.IO;
using Outracks.IO;

namespace Outracks.Simulator.Protocol
{
	public class Ended : IBinaryMessage
	{
		public static string MessageType = "Ended";
		public string Type { get { return MessageType; } }

		public IBinaryMessage Command { get; set; }
		public bool Success { get; set; }
		public AbsoluteDirectoryPath BuildDirectory { get; set; }

		public void WriteDataTo(BinaryWriter writer)
		{
			Command.WriteTo(writer);
			writer.Write(Success);
			AbsoluteDirectoryPath.Write(writer, BuildDirectory);
		}

		public static Ended ReadDataFrom(BinaryReader reader)
		{
			return new Ended
			{
				Command = BinaryMessage.ReadFrom(reader),
				Success = reader.ReadBoolean(),
				BuildDirectory = AbsoluteDirectoryPath.Read(reader)
			};
		}
	}
}