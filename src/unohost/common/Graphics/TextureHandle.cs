using System.IO;

namespace Outracks.UnoHost
{
	public class TextureHandle
	{
		readonly int _handle;
		public TextureHandle(int handle)
		{
			_handle = handle;
		}

		public static implicit operator int(TextureHandle handle)
		{
			return handle._handle;
		}

		public static void Write(BinaryWriter w, TextureHandle h)
		{
			w.Write(h._handle);
		}

		public static TextureHandle Read(BinaryReader r)
		{
			return new TextureHandle(r.ReadInt32());
		}

		public override string ToString()
		{
			return "{Texture " + _handle + "}";
		}
	}
}