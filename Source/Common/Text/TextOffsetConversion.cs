using System;
using System.Diagnostics.Contracts;

namespace Outracks
{
	public static class TextOffsetConversion
	{
		/// <summary>
		/// Returns the offset in the data string based on this text position.
		/// A multi-byte character advances the offset with one.
		/// A newline advances the offset with one, and offset is not affected by CR.
		/// </summary>
		/// <returns></returns>
		[Pure]
		public static TextOffset ToOffset(this TextPosition pos, string data)
		{
			if (string.IsNullOrEmpty(data))
				throw new ArgumentNullException(data);

			var lines = data.Split("\n");
			if (pos.Line < 1 || pos.Line > lines.Length)
				throw new IndexOutOfRangeException("Text offset is out of range");

			var sizeOfDataBefore = 0;
			for (var i = 0; i < pos.Line - 1; ++i)
			{
				sizeOfDataBefore += lines[i].Replace("\r", "").Length + 1;
			}

			var line = lines[pos.Line - 1].Replace("\r", "");
			if (pos.Character < 1 || pos.Character - 1 > line.Length)
				throw new IndexOutOfRangeException("Text offset is out of range");

			return new TextOffset(sizeOfDataBefore + pos.Character - 1);
		}

		[Pure]
		public static TextPosition ToPosition(this TextOffset offset, string data)
		{
			if (string.IsNullOrEmpty(data))
				throw new ArgumentNullException(data);

			var lines = data.Replace("\r", "").Split("\n");
			var numLinesBeforeOffset = 0;
			var pos = 0;
			for (var i = 0; i < lines.Length; ++i)
			{
				if (offset < pos + lines[i].Length + 1)
				{
					break;
				}
				++numLinesBeforeOffset;
				pos += lines[i].Length + 1;
			}

			var character = offset - pos;

			return new TextPosition(
				new LineNumber(numLinesBeforeOffset + 1),
				new CharacterNumber(character + 1));
		}

		[Pure]
		public static TextPosition OffsetBy(this TextPosition pos, int characters, string code)
		{
			return new TextOffset(pos.ToOffset(code) + characters).ToPosition(code);
		}

		[Pure]
		public static TextRegion OffsetBy(this TextRegion region, int characters, string code)
		{
			return new TextRegion(
				region.From.OffsetBy(characters, code),
				region.To.OffsetBy(characters, code));
		}
	}
}