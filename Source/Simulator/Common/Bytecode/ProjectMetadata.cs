using Uno;
using Uno.Collections;
using System.IO;

namespace Outracks.Simulator.Bytecode
{
	public class ProjectMetadata
	{
		public readonly ImmutableList<KeyValuePair<ObjectIdentifier, IEnumerable<ObjectIdentifier>>> ElementTypeHierarchy;
		public readonly ImmutableList<PrecompiledElement> PrecompiledElements;

		public ProjectMetadata(
			ImmutableList<KeyValuePair<ObjectIdentifier, IEnumerable<ObjectIdentifier>>> hierarchy,
			ImmutableList<PrecompiledElement> elements)
		{
			ElementTypeHierarchy = hierarchy;
			PrecompiledElements = elements;
		}

		public void WriteDataTo(BinaryWriter writer)
		{
			List.Write(writer, ElementTypeHierarchy, (Action<KeyValuePair<ObjectIdentifier, IEnumerable<ObjectIdentifier>>, BinaryWriter>)WriteType);
			List.Write(writer, PrecompiledElements, (Action<PrecompiledElement, BinaryWriter>)PrecompiledElement.Write);
		}

		public static ProjectMetadata ReadDataFrom(BinaryReader reader)
		{
			var hierarchy = List.Read(reader, (Func<BinaryReader, KeyValuePair<ObjectIdentifier, IEnumerable<ObjectIdentifier>>>)ReadType);
			var elements = List.Read(reader, (Func<BinaryReader, PrecompiledElement>)PrecompiledElement.Read);
			return new ProjectMetadata(hierarchy, elements);
		}

		static void WriteType(KeyValuePair<ObjectIdentifier, IEnumerable<ObjectIdentifier>> type, BinaryWriter writer)
		{
			ObjectIdentifier.Write(type.Key, writer);
			List.Write(writer, type.Value, (Action<ObjectIdentifier, BinaryWriter>)ObjectIdentifier.Write);
		}

		static KeyValuePair<ObjectIdentifier, IEnumerable<ObjectIdentifier>> ReadType(BinaryReader reader)
		{
			var key = ObjectIdentifier.Read(reader);
			var value = List.Read(reader, (Func<BinaryReader, ObjectIdentifier>)ObjectIdentifier.Read);
			return new KeyValuePair<ObjectIdentifier, IEnumerable<ObjectIdentifier>>(key, value);
		}
	}

	public class PrecompiledElement
	{
		public readonly ObjectIdentifier Id;
		public readonly string Source;

		public PrecompiledElement(ObjectIdentifier id, string source)
		{
			Id = id;
			Source = source;
		}

		public static void Write(PrecompiledElement dependency, BinaryWriter writer)
		{
			ObjectIdentifier.Write(dependency.Id, writer);
			writer.Write(dependency.Source);
		}

		public static PrecompiledElement Read(BinaryReader reader)
		{
			var id = ObjectIdentifier.Read(reader);
			var source = reader.ReadString();
			return new PrecompiledElement(id, source);
		}

		public override string ToString()
		{
			return Source;
		}
	}
}