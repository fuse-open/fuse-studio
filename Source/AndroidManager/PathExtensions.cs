using System;
using Outracks.IO;

namespace Outracks.AndroidManager
{
	static class PathExtensions
	{
		public static AbsoluteDirectoryPath GetEnvironmentPath(Environment.SpecialFolder specialFolder)
		{
			return AbsoluteDirectoryPath.Parse(Environment.GetFolderPath(specialFolder));
		}
	}
}