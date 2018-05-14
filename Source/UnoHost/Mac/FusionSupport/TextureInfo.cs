namespace Outracks.UnoHost.OSX.FusionSupport
{
	class TextureInfo
	{
		public readonly TextureHandle Handle;
		public readonly Size<Pixels> Size;

		public TextureInfo(TextureHandle handle, Size<Pixels> size)
		{
			Handle = handle;
			Size = size;
		}

		public static implicit operator TextureHandle(TextureInfo info)
		{
			return info.Handle;
		}
	}
}