using Uno;
using System.IO;

namespace Outracks.Simulator
{
	public class SourceReference
	{
		public SourceReference(
			string file,
			Optional<TextPosition> location)
		{
			File = file;
			Location = location;
		}

		public readonly string File;
		public readonly Optional<TextPosition> Location;

		public static SourceReference Read(BinaryReader r)
		{
			var file = r.ReadString();
			var location = Optional.Read(r, (Func<BinaryReader, TextPosition>)TextPosition.Read);

			return new SourceReference(file, location);
		}

		public static void Write(BinaryWriter w, SourceReference s)
		{
			w.Write(s.File);
			Optional.Write(w, s.Location, (Action<BinaryWriter, TextPosition>)TextPosition.Write);
		}
	}

	public static class SourceReferenceCanonicalization
	{
		public static string ToCanonicalForm(this Optional<SourceReference> src)
		{
			return src.HasValue ? ToCanonicalForm(src.Value) : "?";
		}

		public static string ToCanonicalForm(this SourceReference src)
		{
			return src.File +
				(src.Location.HasValue
					? "(" + src.Location.Value.ToCanonicalForm() + ")"
					: "");
		}

		static string ToCanonicalForm(this TextPosition pos)
		{
			return pos.Line + "," + pos.Character;
		}
	}
}