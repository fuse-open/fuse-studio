using System;

namespace Outracks.Fuse.Protocol
{
	[PayloadTypeName("Unsubscribe")]
	public sealed class UnsubscribeRequest : IRequestData<SubscribeResponse>
	{
		[PluginComment("Id of subscription")]
		public Guid SubscriptionId;
	}

	[PayloadTypeName("Unsubscribe")]
	public sealed class UnsubscribeResponse : IResponseData
	{
	}
}