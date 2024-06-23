using System;
using System.IO;

namespace Outracks
{
	public struct TextRegion : IEquatable<TextRegion>
	{
		public readonly TextPosition From;
		public readonly TextPosition To; // TODO: figure out if this should be inclusive or not

		public TextRegion(TextPosition from, TextPosition to)
		{
			if (from > to) throw new ArgumentException("from-position can not be after to-position"); // TODO: potentially update this when figured out
			From = from;
			To = to;
		}

		public override string ToString()
		{
			return "[" + From + ", " + To + "] or >"; // TODO: update this when figured out
		}

		public override int GetHashCode()
		{
			return From.GetHashCode() ^ To.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return obj is TextRegion && Equals((TextRegion)obj);
		}

		public bool Equals(TextRegion other)
		{
			return From == other.From && To == other.To;
		}

		public static bool operator ==(TextRegion a, TextRegion b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(TextRegion a, TextRegion b)
		{
			return !a.Equals(b);
		}

		public static TextRegion Read(BinaryReader reader)
		{
			return new TextRegion(TextPosition.Read(reader), TextPosition.Read(reader));
		}

		public static void Write(BinaryWriter writer, TextRegion value)
		{
			TextPosition.Write(writer, value.From);
			TextPosition.Write(writer, value.To);
		}
	}
}