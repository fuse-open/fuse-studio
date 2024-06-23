using System;
using System.IO;

namespace Outracks.Fuse
{
	static class ExceptionWriter
	{
		public static void WriteStackTraces(this TextWriter writer, Exception e)
		{
			writer.WriteLine(e.StackTrace);
			while ((e = e.InnerException) != null)
			{
				writer.WriteLine("Inner exception: " + e.Message);
				writer.WriteLine(e.StackTrace);
			}
		}
	}
}