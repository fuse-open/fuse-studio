using System;

namespace Outracks.Fuse.Protocol
{
	[PayloadTypeName("Hello")]
	public sealed class HelloRequest : IRequestData<HelloResponse>
	{
		[PluginComment("A list of requests that you respond to."),
		 Obsolete("This will be removed and replaced by PublishServiceRequest")]
		public string []Implements;

		[PluginComment("A string consisting of a RegEx specifiying a filter for broadcasted events."), 
		 Obsolete("This will be removed and replaced by SubscribeRequest")]
		public string EventFilter; 
		
		[PluginComment("What you want to identify yourself as for the daemon.")]
		public string Identifier;

		[PluginComment("Daemon key used to differentiate daemons between different local users. " +
			"Run `fuse daemon --get-key` to accuire this key")] 
		public string DaemonKey;
	}

	[PayloadTypeName("Hello")]
	public sealed class HelloResponse : IResponseData
	{
	}
}