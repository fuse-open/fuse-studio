using System;
using System.IO;
using System.Reactive;

namespace Outracks.UnoHost.Windows.Protocol
{
	public class TextureWithSize
	{
		public readonly IntPtr D3DTextureHandle;
		public readonly Size<Pixels> Size;
		public readonly Ratio<Pixels, Points> Dpi;

		public TextureWithSize(IntPtr d3DTextureHandle, Size<Pixels> size, Ratio<Pixels,Points> dpi)
		{
			D3DTextureHandle = d3DTextureHandle;
			Size = size;
			Dpi = dpi;
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(D3DTextureHandle.ToInt32());
			writer.Write((double)Size.Width);
			writer.Write((double)Size.Height);
			writer.Write(Dpi);
		}

		public static TextureWithSize Read(BinaryReader reader)
		{
			return new TextureWithSize(
				new IntPtr(reader.ReadInt32()), 
				new Size<Pixels>(reader.ReadDouble(), reader.ReadDouble()),
				reader.ReadDouble());
		}
	}

	public static class SetSurfaceMessage
	{
		const string Type = "Windows.SetSurface";

		public static IBinaryMessage Compose(TextureWithSize textureWithSize)
		{			
			return BinaryMessage.Compose(Type, textureWithSize.Write);
		}

		public static Optional<TextureWithSize> TryParse(IBinaryMessage message)
		{
			return message.TryParse(Type, TextureWithSize.Read);
		}
	}
}