using Outracks.Fuse.Protocol;

namespace Fuse.Preview
{
	[PayloadTypeName("Fuse.ReifyFileSuccessEvent")]
	public class ReifyFileSuccessEvent : IEventData
	{
		public string Path;
		public string Message;

		public override string ToString()
		{
			return "Successfully reified file: \"" + Path + "\"" + (string.IsNullOrEmpty(Message) ? "" : (". Additional info: " + Message));
		}
	}

	[PayloadTypeName("Fuse.ReifyFileErrorEvent")]
	public class ReifyFileErrorEvent : IEventData
	{
		public string Path;
		public string Message;

		public override string ToString()
		{
			return "Error reifying file: \"" + Path + "\": " + Message;
		}
	}
}