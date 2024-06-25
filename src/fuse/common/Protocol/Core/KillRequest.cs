using Outracks.Fuse.Protocol;

namespace Outracks.Fuse.Daemon
{
	[PayloadTypeName("Fuse.KillDaemon")]
	public sealed class KillRequest : IRequestData<KillResponse>
	{
		[PluginComment("Reason for why you want to kill me.")]
		public string Reason;
	}
}