using System;
using OpenTK.Graphics.OpenGL;

namespace Outracks.UnoHost.Mac.UnoView.RenderTargets
{
	static class IOSurfaceFramebuffer
	{
		public static FramebufferInfo CreateFramebuffer(this IOSurfaceObject surface)
		{
			var size = surface.Size.Max(1, 1);

			var handles = new int[1];
			GL.GenFramebuffers(1, handles);
			var handle = new FramebufferHandle(handles[0]);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);

			var depthbuffer = CreateDepthBuffer(size);
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthbuffer);

			var colorbuffer = IOSurfaceTexture.Create(size, surface);
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.TextureRectangle, colorbuffer, 0);

			CheckStatus();

			return new FramebufferInfo(handle, depthbuffer, Optional.None(), colorbuffer);
		}

		static RenderbufferHandle CreateDepthBuffer(Size<Pixels> size)
		{
			var handles = new int[1];
			GL.GenRenderbuffers(1, handles);
			var depth = new RenderbufferHandle(handles[0]);
			ResizeDepthBuffer(depth, size);
			return depth;
		}

		static void ResizeDepthBuffer(RenderbufferHandle renderbuffer, Size<Pixels> size)
		{
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbuffer);
			GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent16, (int)size.Width, (int)size.Height);
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
		}

		static void CheckStatus()
		{
			var result = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
			if (result != FramebufferErrorCode.FramebufferComplete)
				throw new Exception("Failed to create framebuffer (" + result + "). Try updating to your graphics card drivers.");
		}


		public static TextureHandle CreateTexture(this IOSurfaceObject surface)
		{
			return IOSurfaceTexture.Create(surface.Size, surface);
		}
	}
}
