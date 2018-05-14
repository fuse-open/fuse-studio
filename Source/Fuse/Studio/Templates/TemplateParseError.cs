using System;

namespace Outracks.Fuse.Templates
{
	public class TemplateParseError : Exception
	{
		public readonly TextPosition Position;

		public TemplateParseError(string message, TextPosition position)
			: base("Failed to parse template: " + message)
		{
			Position = position;
		}
	}
}