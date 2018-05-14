using System.Reactive;

namespace Outracks.UnoHost.Windows.Protocol
{
	public static class WindowContextMenuMessage
	{
		const string Type = "Windows.WindowContextMenuMessage";

		public static IBinaryMessage Compose(bool show)
		{
			return BinaryMessage.Compose(Type, writer => writer.Write(show));
		}

		public static Optional<bool> TryParse(IBinaryMessage message)
		{
			return message.TryParse(Type, reader => reader.ReadBoolean());
		}
	}
}