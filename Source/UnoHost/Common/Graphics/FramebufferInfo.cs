using System.IO;

namespace Outracks.UnoHost
{
	public class FramebufferInfo
	{
		public FramebufferHandle Handle { get; private set; }

		public Optional<RenderbufferHandle> Depthbuffer { get; private set; }
		public Optional<RenderbufferHandle> Stencilbuffer { get; private set; }
		public Optional<TextureHandle> Colorbuffer { get; private set; }

		public bool IsRealBackbuffer
		{
			get { return Handle == new FramebufferHandle(0); }
		}

		public bool HasCombinedDepthStencil
		{
			get { return Depthbuffer == Stencilbuffer; }
		}

		public FramebufferInfo(
			FramebufferHandle handle, 
			Optional<RenderbufferHandle> depthbuffer, 
			Optional<RenderbufferHandle> stencilbuffer, 
			Optional<TextureHandle> colorbuffer)
		{
			Colorbuffer = colorbuffer;
			Depthbuffer = depthbuffer;
			Stencilbuffer = stencilbuffer;
			Handle = handle;
		}

		public override string ToString()
		{
			return "{Handle = " + Handle + ", Colorbuffer = " + Colorbuffer + ", Depthbuffer = " + Depthbuffer + "}";
		}

		public static void Write(BinaryWriter writer, FramebufferInfo info)
		{
			FramebufferHandle.Write(writer, info.Handle);
			Optional.Write(writer, info.Depthbuffer, buf => RenderbufferHandle.Write(writer, buf));
			Optional.Write(writer, info.Stencilbuffer, buf => RenderbufferHandle.Write(writer, buf));
			Optional.Write(writer, info.Colorbuffer, buf => TextureHandle.Write(writer, buf));
		}

		public static FramebufferInfo Read(BinaryReader reader)
		{
			return new FramebufferInfo(
				FramebufferHandle.Read(reader),
				Optional.Read(reader, () => RenderbufferHandle.Read(reader)),
				Optional.Read(reader, () => RenderbufferHandle.Read(reader)),
				Optional.Read(reader, () => TextureHandle.Read(reader)));
		}
	}
}