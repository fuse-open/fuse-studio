using System.IO;

namespace Outracks.Simulator.Protocol
{
	public class Started : IBinaryMessage
	{
		public static string MessageType = "Started";
		public string Type { get { return MessageType; } }

		public IBinaryMessage Command { get; set; }

		public void WriteDataTo(BinaryWriter writer)
		{
			Command.WriteTo(writer);
		}

		public static Started ReadDataFrom(BinaryReader reader)
		{
			return new Started
			{
				Command = BinaryMessage.ReadFrom(reader)
			};
		}
	}
}