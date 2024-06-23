namespace Outracks.UnoHost
{
	public enum FocusState
	{
		Focused,
		Blurred
	}

	public class WindowFocusMessage
	{
		const string Type = "Windows.WindowFocus";
		public static IBinaryMessage Compose(FocusState focusState)
		{
			return BinaryMessage.Compose(Type, writer =>
				writer.Write((int)focusState));
		}

		public static Optional<FocusState> TryParse(IBinaryMessage message)
		{
			return message.TryParse(Type, reader =>
				(FocusState)reader.ReadInt32());
		}
	}
}