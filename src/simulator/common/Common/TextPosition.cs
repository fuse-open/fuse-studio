using Uno;
using System.IO;

namespace Outracks.Simulator
{
	public struct TextPosition //: IEquatable<TextPosition>
	{
		public readonly LineNumber Line;
		public readonly CharacterNumber Character;

		public TextPosition(LineNumber line, CharacterNumber character)
		{
			Line = line;
			Character = character;
		}

		public override string ToString()
		{
			return Line + ":" + Character;
		}

		public override int GetHashCode()
		{
			return Line.GetHashCode() ^ Character.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return obj is TextPosition && Equals((TextPosition)obj);
		}

		public bool Equals(TextPosition other)
		{
			return Line == other.Line && Character == other.Character;
		}

		public static bool operator ==(TextPosition a, TextPosition b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(TextPosition a, TextPosition b)
		{
			return !(a == b);
		}

		public static bool operator <(TextPosition a, TextPosition b)
		{
			if (a.Line < b.Line) return true;
			if (a.Line > b.Line) return false;
			return a.Character < b.Character;
		}

		public static bool operator >(TextPosition a, TextPosition b)
		{
			return a != b && !(a < b);
		}

		public static bool operator <=(TextPosition a, TextPosition b)
		{
			return  a == b || a < b;
		}

		public static bool operator >=(TextPosition a, TextPosition b)
		{
			return a == b || a > b;
		}

		public static TextPosition Read(BinaryReader reader)
		{
			var line = reader.ReadInt32();
			var character = reader.ReadInt32();
			return new TextPosition(
				new LineNumber(line),
				new CharacterNumber(character));
		}

		public static void Write(BinaryWriter writer, TextPosition value)
		{
			writer.Write(value.Line);
			writer.Write(value.Character);
		}
	}

}