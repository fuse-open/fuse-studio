namespace Outracks.IO
{
	public static class LineEndings
	{
		public static string NormalizeLineEndings(this string text)
		{
			return text.Replace("\r\n", "\n").Replace("\r", "\n");
		}

		public static string NormalizeLineEndings(this string text, string defaultLineEnding)
		{
			return NormalizeLineEndings(text).Replace("\n", defaultLineEnding);
		}
	}
}