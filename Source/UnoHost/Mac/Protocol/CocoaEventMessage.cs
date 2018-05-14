using System;
using System.Runtime.InteropServices;
using AppKit;
using Foundation;

namespace Outracks.UnoHost.OSX.Protocol
{
	static class CocoaEventMessage
	{
		const string Name = "OSX.CocoaEvent";

		public static IBinaryMessage Compose(NSEvent nsEvent)
		{
			return BinaryMessage.Compose(Name, writer =>
			{
				try
				{
					var data = NSKeyedArchiver.ArchivedDataWithRootObject(nsEvent);
					var managedData = new byte[data.Length];
					Marshal.Copy(data.Bytes, managedData, 0, (int)data.Length);

					writer.Write((int)data.Length);
					writer.Write(managedData);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			});
		}

		public static Optional<NSEvent> TryParse(IBinaryMessage message)
		{
			return message.TryParse(Name, reader =>
			{
				var length = reader.ReadInt32();
				var data = reader.ReadBytes(length);
				var nsData = NSData.FromArray(data);
				return (NSEvent)NSKeyedUnarchiver.UnarchiveObject(nsData);
			});
		}
	}
}