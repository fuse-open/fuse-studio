using System;
using System.Runtime.InteropServices;
using OpenGL;
using OpenTK.Graphics.OpenGL;
using GL = OpenTK.Graphics.OpenGL.GL;

namespace Outracks.UnoHost.OSX.UnoView.RenderTargets
{
	static class IOSurfaceTexture
	{
		public static TextureHandle Create(Size<Pixels> size, IOSurface surface)
		{
			GL.ActiveTexture(TextureUnit.Texture0);
			var texture = new TextureHandle(GL.GenTexture());
			GL.BindTexture(TextureTarget.TextureRectangle, texture);
			GL.TexParameter(TextureTarget.TextureRectangle, TextureParameterName.TextureMagFilter, (int)(TextureMagFilter.Nearest));
			GL.TexParameter(TextureTarget.TextureRectangle, TextureParameterName.TextureMinFilter, (int)(TextureMinFilter.Linear));
			GL.TexParameter(TextureTarget.TextureRectangle, TextureParameterName.TextureWrapS, (int)(TextureWrapMode.ClampToEdge));
			GL.TexParameter(TextureTarget.TextureRectangle, TextureParameterName.TextureWrapT, (int)(TextureWrapMode.ClampToEdge));
			Resize(texture, size, surface);
			return texture;
		}

		public static void Resize(TextureHandle texture, Size<Pixels> size, IOSurface surface)
		{
			GL.BindTexture(TextureTarget.TextureRectangle, texture);
			CGLTexImageIOSurface2D(
				CGLContext.CurrentContext.Handle,
				(int)TextureTarget.TextureRectangle,
				(int)PixelInternalFormat.Rgba,
				(int)size.Width,
				(int)size.Height,
				(int)PixelFormat.Bgra,
				(int)PixelType.UnsignedInt8888Reversed,
				surface.Handle,
				0);
			GL.BindTexture(TextureTarget.TextureRectangle, 0);
		}

		const string GraphicsLibrary = "/System/Library/Frameworks/OpenGL.framework/Versions/A/OpenGL";
		[DllImport(GraphicsLibrary)]
		static extern void CGLTexImageIOSurface2D(IntPtr ctx, int target, int internalFormat, int width, int height, int format, int type, IntPtr ioSurface, int plane);
	}
}