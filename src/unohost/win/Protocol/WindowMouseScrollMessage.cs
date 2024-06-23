namespace Outracks.UnoHost.Windows.Protocol
{
	public static class WindowMouseScrollMessage
	{
		const string Type = "Windows.WindowMouseScrollMessage";

		public static IBinaryMessage Compose(int delta)
		{
			return BinaryMessage.Compose(Type, writer => writer.Write(delta));
		}

		public static Optional<int> TryParse(IBinaryMessage message)
		{
			return message.TryParse(Type, reader => reader.ReadInt32());
		}
	}
}