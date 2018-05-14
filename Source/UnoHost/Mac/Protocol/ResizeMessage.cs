namespace Outracks.UnoHost.OSX.Protocol
{
	class SizeData
	{
		public readonly Size<Points> Size;
		public readonly Ratio<Pixels, Points> Density;

		public SizeData(Size<Points> size, Ratio<Pixels, Points> density)
		{
			Size = size;
			Density = density;
		}
	}

	static class ResizeMessage
	{
		const string Name = "OSX.Resize";

		public static IBinaryMessage Compose(SizeData sizeData)
		{
			return BinaryMessage.Compose(Name, writer =>
			{
				writer.Write((int)sizeData.Size.Width);
				writer.Write((int)sizeData.Size.Height);
				writer.Write((double)sizeData.Density);
			});
		}

		public static Optional<SizeData> TryParse(IBinaryMessage message)
		{
			return message.TryParse(Name, reader =>
				new SizeData(
					new Size<Points>(reader.ReadInt32(), reader.ReadInt32()),
					reader.ReadDouble())
			);
		}
	}
}