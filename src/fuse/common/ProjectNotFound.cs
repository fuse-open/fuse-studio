using System;

namespace Outracks.Fuse
{
	public class ProjectNotFound : Exception
	{
		public ProjectNotFound(Exception innerException = null)
			: base("Project not found", innerException)
		{
		}
	}
}