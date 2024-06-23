namespace Outracks.UnoHost
{
	public class OpenGlVersionMessage
	{
		public const string Type = "OpenGLVersionMessage";
		public static IBinaryMessage Compose(OpenGlVersion openGlVersion)
		{
			return BinaryMessage.Compose(Type, writer =>
			{
				writer.Write(openGlVersion.GlVersion);
				writer.Write(openGlVersion.GlVendor);
				writer.Write(openGlVersion.GlRenderer);
			});
		}

		public static Optional<OpenGlVersion> TryParse(IBinaryMessage message)
		{
			return message.TryParse(Type, reader =>
				new OpenGlVersion(reader.ReadString(), reader.ReadString(), reader.ReadString()));
		}
	}
}
