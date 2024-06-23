using System.Collections.Generic;

namespace Outracks.UnoHost.Windows.Protocol
{
	public enum MouseEventType
	{
		MouseDown,
		MouseUp,
		MouseMove
	}

	public class MouseEventArgs
	{
		public readonly MouseEventType EventType;
		public readonly Point<Pixels> Position;

		public MouseEventArgs(MouseEventType eventType, Point<Pixels> position)
		{
			EventType = eventType;
			Position = position;
		}
	}

	public static class MouseEventMessage
	{
		const string Type = "Windows.MouseEvent";

		public static IBinaryMessage Compose(MouseEventArgs eventArgs)
		{
			return BinaryMessage.Compose(Type, writer =>
			{
				writer.Write((int)eventArgs.EventType);
				writer.Write((double)eventArgs.Position.X);
				writer.Write((double)eventArgs.Position.Y);
			});
		}

		public static Optional<MouseEventArgs> TryParse(IBinaryMessage message)
		{
			return message.TryParse(Type,
				reader =>
				{
					var type = (MouseEventType)reader.ReadInt32();
					return new MouseEventArgs(type, new Point<Pixels>(reader.ReadDouble(), reader.ReadDouble()));
				});
		}
	}
}