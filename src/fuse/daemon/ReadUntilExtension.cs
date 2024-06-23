using System;
using System.IO;

namespace Outracks.Fuse
{
	public static class ReadUntilExtension
	{
		public static bool ReadLinesUntil(this StreamReader reader, Func<string, bool> predicate)
		{
			while (true)
			{
				var line = reader.ReadLine();
				if (line == null)
					return false;

				if (predicate(line))
					return true;
			}
		}
	}
}