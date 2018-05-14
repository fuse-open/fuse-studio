using System;
using OpenTK.Graphics.OpenGL;

namespace Outracks.UnoHost.OSX.FusionSupport
{
	using UnoView.RenderTargets;

	class SurfaceCacheItem : IDisposable
	{
		public readonly IOSurface Surface;
		Optional<TextureHandle> _texture;

		public SurfaceCacheItem(IOSurface surface)
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