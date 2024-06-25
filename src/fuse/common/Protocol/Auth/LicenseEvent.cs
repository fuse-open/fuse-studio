namespace Outracks.Fuse.Protocol.Auth
{
	[PayloadTypeName("License")]
	public class LicenseEvent: IEventData
	{
		public bool Error = false;
	}
}
