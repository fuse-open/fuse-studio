using System.Windows.Forms;

namespace Outracks.UnoHost.Windows.Protocol
{
	public static class WindowKeyDown
	{		
		const string Type = "Windows.WindowKeyDown";

		public static IBinaryMessage Compose(Keys keys)
		{
			return BinaryMessage.Compose(Type, writer => writer.Write((int)keys));
		}

		public static Optional<Keys> TryParse(IBinaryMessage message)
		{
			return message.TryParse(Type, reader => (Keys)reader.ReadInt32());
		}
	}

	public static class WindowKeyUp
	{
		const string Type = "Windows.WindowKeyUp";

		public static IBinaryMessage Compose(Keys keys)
		{
			return BinaryMessage.Compose(Type, writer => writer.Write((int)keys));
		}

		public static Optional<Keys> TryParse(IBinaryMessage message)
		{
			return message.TryParse(Type, reader => (Keys)reader.ReadInt32());
		}
	}
}