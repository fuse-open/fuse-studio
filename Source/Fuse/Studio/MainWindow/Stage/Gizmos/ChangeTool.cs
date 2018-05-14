using System;
using System.IO;

namespace Outracks.Fuse.Stage
{
	enum Tool
	{
		None,
		Select,
	}

	class ChangeTool : IBinaryMessage
	{
		public static readonly string MessageType = "ChangeTool";
		public string Type { get { return MessageType; } }

		public Tool Tool;

		public void WriteDataTo(BinaryWriter writer)
		{
			writer.Write(Tool.ToString());
		}

		public static ChangeTool ReadDataFrom(BinaryReader reader)
		{
			return new ChangeTool
			{
				Tool = (Tool)Enum.Parse(typeof(Tool), reader.ReadString())
			};
		}
	}
}