using System;

namespace Outracks.Fuse.Protocol.Messages
{
	// Be careful changing the names, it means the API changes!
	public enum ConsoleType
	{
		DebugLog,
		BuildLog
	}

	[PayloadTypeName("Fuse.LogEvent")]
	public class LogEvent : IEventData
	{
		public string DeviceId;
		public string DeviceName;
		public string ProjectId;
		public string Message;
		public DateTime Timestamp;

		public ConsoleType ConsoleType;
	}
}