using System;
using OpenTK.Graphics.OpenGL;

namespace Outracks.UnoHost.Mac.UnoView.RenderTargets
{
	class IOSurfaceRenderTargetBuffer : IDisposable
	{
		public readonly IOSurfaceObject Surface;
		public readonly Size<Pixels> Size;
		Optional<FramebufferInfo> _framebufferInfo;

		public IOSurfaceRenderTargetBuffer(Size<Pixels> size)
		{
			Size = size;
			Surface = IOSurfaceObject.Create(new Size<Pixels>(size.Width, size.Height));
		}

		public int Handle
		{
			get
			{
				if (!_framebufferInfo.HasValue)
					_framebufferInfo = Surface.CreateFramebuffer();

				return _framebufferInfo.Value.Handle;
			}
		}

		public void Dispose()
		{
			_framebufferInfo.Do(Delete);
			Surface.Dispose();
		}

		static void Delete(FramebufferInfo framebufferInfo)
		{
			framebufferInfo.Colorbuffer.Do(tex => GL.DeleteTexture(tex));
			framebufferInfo.Depthbuffer.Do(renderBuffer => GL.DeleteRenderbuffers(1, new int[] { renderBuffer }));
			framebufferInfo.Stencilbuffer.Do(renderBuffer => GL.DeleteRenderbuffers(1, new int[] { renderBuffer }));
			GL.DeleteFramebuffers(1, new int[] { framebufferInfo.Handle });
		}

		public override string ToString()
		{
			return Surface.Handle + " (" + Size + ")";
		}
	}
}