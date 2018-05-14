using System.IO;

namespace Outracks.UnoHost
{
	public class FramebufferHandle
	{
		readonly int _handle;
		public FramebufferHandle(int handle)
		{
			_handle = handle;
		}

		public static implicit operator int(FramebufferHandle handle)
		{
			return handle._handle;
		}

		public static void Write(BinaryWriter w, FramebufferHandle h)
		{
			w.Write(h._handle);
		}

		public static FramebufferHandle Read(BinaryReader r)
		{
			return new FramebufferHandle(r.ReadInt32());
		}

		public override string ToString()
		{
			return "{Framebuffer " + _handle + "}";
		}
	}
}