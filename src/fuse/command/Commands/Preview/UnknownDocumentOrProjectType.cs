using System;
using Outracks.IO;

namespace Outracks.Fuse
{
	public sealed class UnknownDocumentOrProjectType : Exception
	{
		public readonly AbsoluteFilePath Path;

		public UnknownDocumentOrProjectType(AbsoluteFilePath path)
		{
			Path = path;
		}
	}
}