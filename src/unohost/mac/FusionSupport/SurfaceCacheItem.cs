using System;
using OpenTK.Graphics.OpenGL;
using Outracks.UnoHost.Mac.UnoView.RenderTargets;

namespace Outracks.UnoHost.Mac.FusionSupport
{
	class SurfaceCacheItem : IDisposable
	{
		public readonly IOSurfaceObject Surface;
		Optional<TextureHandle> _texture;

		public SurfaceCacheItem(IOSurfaceObject surface)
		{
			Surface = surface;
		}

		public TextureHandle GetOrCreateTexture()
		{
			if (!_texture.HasValue)
				_texture = Surface.CreateTexture();

			return _texture.Value;
		}

		public void Dispose()
		{
			_texture.Do(texture => GL.DeleteTexture(texture));
			Surface.Dispose();
		}
	}
}