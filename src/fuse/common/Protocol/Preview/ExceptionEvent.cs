using System;

namespace Outracks.Fuse.Protocol.Messages
{
	[PayloadTypeName("Fuse.ExceptionEvent")]
	public class ExceptionEvent : IEventData
	{
		public string DeviceId;
		public string DeviceName;
		public string ProjectId;
		public string Type;
		public string Message;
		public string StackTrace;
		public DateTime Timestamp;
	}
}
