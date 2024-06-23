using System;

namespace Outracks.Fuse.Protocol
{
	// Be careful changing the names, it means the API changes!
	public enum BuildIssueTypeData
	{
		Unknown,
		FatalError,
		Error,
		Warning,
		Message,
	}


	[PayloadTypeName("Fuse.BuildIssueDetected")]
	public class BuildIssueDetectedData : IEventData
	{
		[PluginComment("")]
		public Guid BuildId;

		[PluginComment("")]
		public BuildIssueTypeData IssueType;

		[PluginComment("Optional")]
		public string Path;

		[PluginComment("Optional")]
		public Messages.TextPosition StartPosition;

		[PluginComment("Optional")]
		public Messages.TextPosition EndPosition;

		[PluginComment("")]
		public string ErrorCode;

		[PluginComment("")]
		public string Message;
	}

}