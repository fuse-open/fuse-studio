using System.IO;

namespace Outracks.Simulator.Protocol
{
	public class RebuildRequired : IBinaryMessage
	{
		public static string MessageType = "RebuildRequired";
		public string Type { get { return MessageType; } }
		public void WriteDataTo(BinaryWriter writer) { }

		public static RebuildRequired ReadDataFrom(BinaryReader arg)
		{
			return new RebuildRequired();
		}
	}
}