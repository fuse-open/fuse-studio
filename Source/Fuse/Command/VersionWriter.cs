using System;
using System.IO;

namespace Outracks.Fuse
{
	public static class VersionWriter
	{
		public static void Write(TextWriter textWriter, Version version)
		{
			textWriter.WriteLine(string.Format("Fuse {0}.{1}.{2} (build {3})", version.Major, version.Minor, version.Build, version.Revision));
		}
	}
}
