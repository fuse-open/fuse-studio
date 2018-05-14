using System;

namespace Outracks.Fuse.Protocol
{
	[PayloadTypeName("Fuse.BuildLogged")]
	public class BuildLoggedData : IEventData
	{
		[PluginComment("")]
		public Guid BuildId;

		[PluginComment("")]
		public string Message;
	}

}