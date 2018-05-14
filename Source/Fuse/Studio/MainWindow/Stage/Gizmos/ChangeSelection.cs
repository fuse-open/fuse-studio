using System.IO;

namespace Outracks.Fuse.Stage
{
	using Simulator;

	class ChangeSelection : IBinaryMessage
	{
		public static readonly string MessageType = "Select";
		public string Type { get { return MessageType; } }

		public ObjectIdentifier Id;
		public bool IsPreview;

		public void WriteDataTo(BinaryWriter writer)
		{
			writer.Write(IsPreview);
			ObjectIdentifier.Write(Id, writer);
		}
		public static ChangeSelection ReadDataFrom(BinaryReader reader)
		{
			return new ChangeSelection
			{
				IsPreview = reader.ReadBoolean(),
				Id = ObjectIdentifier.Read(reader)
			};
		}
	}
}