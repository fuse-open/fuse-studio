using System;

namespace Outracks.Fuse.Protocol.Messages
{
	[PayloadTypeName("Fuse.ProjectClosed")]
	public class ProjectClosed : IEventData
	{
		public Guid ProjectId;
	}
}
