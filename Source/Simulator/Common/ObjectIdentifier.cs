using Uno;
using System.IO;

namespace Outracks.Simulator
{
	public sealed class ObjectIdentifier : IEquatable<ObjectIdentifier>
	{
		public static readonly ObjectIdentifier None = new ObjectIdentifier("N/A", 0);

		public static void Write(ObjectIdentifier id, BinaryWriter writer)
		{
			id.Write(writer);
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(_string);
			writer.Write(Document);
		}

		public static ObjectIdentifier Read(BinaryReader reader)
		{
			var str = reader.ReadString();
			var document = reader.ReadString();

			return new ObjectIdentifier(
				str: str,
				document: document);
		}

		public readonly string Document;

		readonly string _string;

		public ObjectIdentifier(string document, int tagIndex)
		{
			_string = document + "#" + tagIndex;
			Document = document;
		}

		public ObjectIdentifier(string str, string document = "N/A")
		{
			_string = str;
			Document = document;
		}

		public bool Equals(ObjectIdentifier other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return _string.Equals(other._string);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is ObjectIdentifier && Equals((ObjectIdentifier)obj);
		}

		public override int GetHashCode()
		{
			return _string.GetHashCode();
		}

		public static bool operator ==(ObjectIdentifier left, ObjectIdentifier right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(ObjectIdentifier left, ObjectIdentifier right)
		{
			return !Equals(left, right);
		}

		public override string ToString()
		{
			return _string;
		}
	}
}