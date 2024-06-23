using System;

namespace Outracks.Fuse.Protocol
{
	[PayloadTypeName("Fuse.BuildEnded")]
	public class BuildEndedData : IEventData
	{
		[PluginComment("")]
		public Guid BuildId;

		public BuildStatus Status;

		// output and stuff?
	}

	public enum BuildStatus
	{
		Success,
		Error,
		InternalError,
	}
}