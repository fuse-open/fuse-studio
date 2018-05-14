using AppKit;

namespace Outracks.UnoHost.OSX.UnoView.RenderTargets
{
	class BackbufferRenderTarget : IRenderTarget
	{
		public int GetFramebufferHandle(Size<Pixels> size)
		{
			return 0;
		}

		public void Flush(NSOpenGLContext ctx)
		{
			ctx.FlushBuffer();
		}
	}
}