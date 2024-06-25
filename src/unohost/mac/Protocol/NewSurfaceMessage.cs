using Outracks.UnoHost.Mac.UnoView.RenderTargets;

namespace Outracks.UnoHost.Mac.Protocol
{
	static class NewSurfaceMessage
	{
		const string Name = "OSX.NewSurface";

		public static IBinaryMessage Compose(IOSurfaceObject surface)
		{
			return Compose(surface.GetSurfaceId());
		}

		public static IBinaryMessage Compose(int surfaceId)
		{
			return BinaryMessage.Compose(Name, writer =>
				writer.Write(surfaceId));
		}

		public static Optional<int> TryParse(IBinaryMessage message)
		{
			return message.TryParse(Name, reader =>
				reader.ReadInt32());
		}
	}
}