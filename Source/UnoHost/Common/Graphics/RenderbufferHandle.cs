using System.IO;

namespace Outracks.UnoHost
{
	public class RenderbufferHandle
	{
		readonly int _handle;
		public RenderbufferHandle(int handle)
		{
			_handle = handle;
		}

		public static implicit operator int(RenderbufferHandle handle)
		{
			return handle._handle;
		}

		public bool IsValid()
		{
			return _handle > 0;
		}

		public static void Write(BinaryWriter w, RenderbufferHandle h)
		{
			w.Write(h._handle);
		}

		public static RenderbufferHandle Read(BinaryReader r)
		{
			return new RenderbufferHandle(r.ReadInt32());
		}

		public override string ToString()
		{
			return "{Renderbuffer " + _handle + "}";
		}
	}
}