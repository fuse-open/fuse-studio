using System.IO;

namespace Outracks.Fuse
{
	public static class VersionWriter
	{
		public static void Write(TextWriter textWriter, string version)
		{
			textWriter.WriteLine("fuse X version {0}", version);
		}
	}
}
