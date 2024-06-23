using System;
using System.IO;
using Outracks;
using Outracks.Simulator;
using GuidSerializer = Outracks.Simulator.GuidSerializer;

namespace Fuse.Preview
{
	public enum BuildIssueType
	{
		FatalError,
		Error,
		Warning,
		Message,
	}

	public sealed class BuildIssueDetected : IBinaryMessage
	{
		public static readonly string MessageType = "BuildIssueDetected";

		public string Type
		{
			get { return MessageType; }
		}

		public Guid BuildId { get; private set; }
		public BuildIssueType Severity { get; private set; }
		public string Code { get; private set; }
		public string Message { get; private set; }
		public Optional<SourceReference> Source { get; private set; }

		public BuildIssueDetected(
			BuildIssueType type,
			string code,
			string message,
			Optional<SourceReference> source,
			Guid buildId)
		{
			BuildId = buildId;
			Severity = type;
			Code = code;
			Message = message;
			Source = source;
		}
		BuildIssueDetected() { }

		public void WriteDataTo(BinaryWriter writer)
		{
			writer.WriteGuid(BuildId);
			writer.Write(Severity.ToString());
			writer.Write(Code);
			writer.Write(Message);
			Optional.Write(writer, Source, (Action<BinaryWriter, SourceReference>) SourceReference.Write);
		}

		public static BuildIssueDetected ReadDataFrom(BinaryReader reader)
		{
			return new BuildIssueDetected
			{
				BuildId = GuidSerializer.ReadGuid(reader),
				Severity = (BuildIssueType)Enum.Parse(typeof(BuildIssueType), reader.ReadString()),
				Code = reader.ReadString(),
				Message = reader.ReadString(),
				Source = Optional.Read(reader, (Func<BinaryReader, SourceReference>)SourceReference.Read),
			};
		}

		public override string ToString()
		{
			return string.Format(
				"{0}: {1} {2}: {3}",
				Source.ToCanonicalForm(),
				Severity, Code,
				Message);
		}
	}

	// Be careful changing the names, it means the API changes!
	public enum BuildIssueTypeData
	{
		Unknown,
		FatalError,
		Error,
		Warning,
		Message,
	}

}