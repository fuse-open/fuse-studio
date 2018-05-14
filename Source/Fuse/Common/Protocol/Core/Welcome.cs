using Outracks.Fuse.Protocol;

namespace Outracks.Fuse.Daemon
{
	[PayloadTypeName("Welcome")]
	public sealed class Welcome : IEventData
	{
		[PluginComment("A friendly message.")]
		public string Message;
	}
}