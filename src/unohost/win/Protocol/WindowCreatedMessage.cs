using System;

namespace Outracks.UnoHost.Windows.Protocol
{
	public static class WindowCreatedMessage
	{
		const string Type = "Windows.WindowCreated";

		public static IBinaryMessage Compose(IntPtr hwnd)
		{
			return BinaryMessage.Compose(Type, writer =>
				writer.Write(hwnd.ToInt32()));
		}

		public static Optional<IntPtr> TryParse(IBinaryMessage message)
		{
			return message.TryParse(Type, reader =>
				new IntPtr(reader.ReadInt32()));
		}
	}
}