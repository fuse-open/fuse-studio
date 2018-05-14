namespace Outracks.Fuse.Protocol
{
	[PayloadTypeName("PublishService")]
	public sealed class PublishServiceRequest : IRequestData<PublishServiceResponse>
	{
		[PluginComment("A list of requests that you respond to.")]
		public readonly string[] RequestNames;

		public PublishServiceRequest(string []requestNames)
		{
			RequestNames = requestNames;
		}
	}

	[PayloadTypeName("PublishService")]
	public sealed class PublishServiceResponse : IResponseData
	{
	}

}