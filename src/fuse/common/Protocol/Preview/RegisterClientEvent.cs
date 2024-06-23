namespace Outracks.Fuse.Protocol.Messages
{
	[PayloadTypeName("Fuse.RegisterClient")]
	public class RegisterClientEvent : IEventData
	{
		public string DeviceId;
		public string DeviceName;
		public string ProjectId;
	}
}
