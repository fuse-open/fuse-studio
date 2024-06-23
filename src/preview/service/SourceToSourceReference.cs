using System;
using Outracks;
using Outracks.IO;
using Outracks.Simulator;
using Uno;

namespace Fuse.Preview
{
	static class SourceToSourceReference
	{
		public static Optional<SourceReference> ToSourceReference(this Source src)
		{
			if (src.IsUnknown)
				return Optional.None();

			var path = AbsoluteFilePath.TryParse(src.FullPath);

			if (path.HasValue)
				return new SourceReference(path.Value.NativePath, src.ToLocation());

			return Optional.None();
		}

		public static TextPosition ToLocation(this Source src)
		{
			var from = new TextPosition(new LineNumber(Math.Max(src.Line, 1)), new CharacterNumber(Math.Max(src.Column + 1, 1)));

			// TODO: (needs TextRegion in uno, or maybe i can just remove this? maybe change to optional end in SourceReference
			//var to = new TextPosition(new LineNumber(Math.Max(src.EndLine, 1)), new CharacterNumber(Math.Max(src.EndColumn + 1, 1)));
			//if (to > from)
			//	return new TextRegion(from, to);

			return from;
		}

	}
}