using System.IO;
using Uno.UX.Markup.Reflection;

namespace Outracks.Simulator.Parser
{
	using IO;

	public class ProjectBuild : IBinaryMessage
	{
		public static readonly string MessageType = "BuiltAssembly";

		public readonly string Project;
		public readonly string Assembly;
		public readonly IDataTypeProvider TypeInfo;

		public ProjectBuild(
			string project,
			string assembly,
			IDataTypeProvider typeInfo)
		{
			Project = project;
			Assembly = assembly;
			TypeInfo = typeInfo;
		}

		public string Type { get { return MessageType; } }

		public void WriteDataTo(BinaryWriter writer)
		{
			writer.Write(Assembly);
		}

		public static SerializedData ReadDataFrom(BinaryReader reader)
		{
			return new SerializedData
			{
				Assembly = reader.ReadString(),
			};
		}

		public class SerializedData
		{
			public string Assembly { get; set; }
		}
	}
}