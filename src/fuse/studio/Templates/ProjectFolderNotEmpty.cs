using System;

namespace Outracks.Fuse.Templates
{
	public class ProjectFolderNotEmpty : Exception
	{
		public ProjectFolderNotEmpty(string path)
			: base("Destination folder is not empty: " + path)
		{
		}
	}
}
