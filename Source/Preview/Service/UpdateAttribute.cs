using System;
using System.IO;
using Uno;

namespace Outracks.Simulator.Protocol
{
	public class UpdateAttribute : IBinaryMessage
	{
		public static readonly string MessageType = "UpdateAttribute";
		public string Type { get { return MessageType; } }

		public Guid Id { get; set; }
		public ObjectIdentifier Object { get; private set; }
		public string Property { get; private set; }
		public Optional<string> Value { get; private set; }

		public SourceReference Source { get; private set; }

		public UpdateAttribute(
			ObjectIdentifier obj, 
			string property, 
			Optional<string> value,
			SourceReference source)
		{
			Object = obj;
			Property = property;
			Value = value;
			Source = source;
		}

		public override string ToString()
		{
			return "Set " + Object + "." + Property + " = " + Value;
		}

		public void WriteDataTo(BinaryWriter writer)
		{
			writer.WriteGuid(Id);
			Object.Write(writer);
			writer.Write(Property);
			Optional.Write(writer, Value, writer.Write);
			SourceReference.Write(writer, Source);
		}

		public static UpdateAttribute ReadDataFrom(BinaryReader reader)
		{
			var id = reader.ReadGuid();
			var obj = ObjectIdentifier.Read(reader);
			var property = reader.ReadString();
			var value = Optional.Read(reader, (Func<string>)reader.ReadString);
			var source = SourceReference.Read(reader);

			return new UpdateAttribute(obj, property, value, source)
			{
				Id = id,
			};
		}
	}
}