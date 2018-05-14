namespace Outracks.Fuse.Protocol
{
	[PayloadTypeName("Subscribe")]
	public sealed class SubscribeRequest : IRequestData<SubscribeResponse>
	{
		[PluginComment("A string consisting of a RegEx specifiying a filter for broadcasted events.")]
		public readonly string Filter;

		[PluginComment("Determins if the sender wants to get a replay of the stored buffer of events (before receieving currently broadcasted events)")]
		public readonly bool Replay;

		[PluginComment("Id of subscription")]
		public readonly int SubscriptionId;

		public SubscribeRequest(string filter, bool replay, int subscriptionId)
		{
			Filter = filter;
			Replay = replay;
			SubscriptionId = subscriptionId;
		}
	}

	[PayloadTypeName("Subscribe")]
	public sealed class SubscribeResponse : IResponseData
	{
	}

}