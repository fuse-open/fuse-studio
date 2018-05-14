using System;
using System.IO;

namespace Outracks.Simulator.Protocol
{
	public class GenerateBytecode : IBinaryMessage
	{
		public static readonly string MessageType = "GenerateBytecode";
		public string Type { get { return MessageType; } }

		public Guid Id { get; private set; }
		public ImmutableList<UxFileContents> UxFiles { get; private set; }

		public GenerateBytecode(Guid id, ImmutableList<UxFileContents> uxFiles)
		{
			Id = id;
			UxFiles = uxFiles;
		}
			
		public void WriteDataTo(BinaryWriter writer)
		{
			writer.WriteGuid(Id);
			List.Write(writer, UxFiles, (Action<UxFileContents, BinaryWriter>)UxFileContents.Write);
		}

		public static GenerateBytecode ReadDataFrom(BinaryReader reader)
		{
			var id = reader.ReadGuid();
			var uxFiles = List.Read(reader, (Func<BinaryReader, UxFileContents>)UxFileContents.Read);
			return new GenerateBytecode(id, uxFiles);
		}

	}
}