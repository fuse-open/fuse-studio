using System;

namespace Outracks.Fuse.Stage
{
	public class MalformedDeviceInfo : Exception
	{
		public MalformedDeviceInfo(Exception innerException)
			: base("MalformedDeviceInfo", innerException)
		{ }
	}
}