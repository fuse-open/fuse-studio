using System;
using System.IO;
using Outracks;
using Outracks.Simulator;
using GuidSerializer = Outracks.Simulator.GuidSerializer;

namespace Fuse.Preview
{
	public sealed class BuildLogged : IBinaryMessage
	{
		public static readonly string MessageType = "BuildLogged";

		public string Type
		{
			get { return MessageType; }
		}

		public Guid BuildId { get; private set; }
		public string Text { get; private set; }
		public Optional<ConsoleColor> Color { get; private set; }

		public BuildLogged(string text, Optional<ConsoleColor> color, Guid buildId)
		{
			BuildId = buildId;
			Text = text;
			Color = color;
		}
		BuildLogged() { }

		public override string ToString()
		{
			return Text;
		}

		public void WriteDataTo(BinaryWriter writer)
		{
			writer.WriteGuid(BuildId);
			writer.Write(Text);
		}

		public static BuildLogged ReadDataFrom(BinaryReader reader)
		{
			return new BuildLogged
			{
				BuildId = GuidSerializer.ReadGuid(reader),
				Text = reader.ReadString()
			};
		}
	}
}