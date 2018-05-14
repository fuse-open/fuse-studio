using System.IO;
using Outracks.IO;

namespace Outracks
{
	public class TextDocumentContent
	{
		public readonly AbsoluteFilePath Path;
		public readonly string Content;

		public TextDocumentContent(AbsoluteFilePath path, string content)
		{
			Path = path;
			Content = content;
		}

		public static void Write(BinaryWriter writer, TextDocumentContent c)
		{
			AbsoluteFilePath.Write(writer, c.Path);
			writer.Write(c.Content);
		}

		public static TextDocumentContent Read(BinaryReader r)
		{
			return new TextDocumentContent(
				AbsoluteFilePath.Read(r),
				r.ReadString());
		}
	}
}