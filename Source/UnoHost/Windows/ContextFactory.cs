using System;
using OpenTK.Graphics;
using OpenTK.Platform;

namespace Outracks.UnoHost.Windows
{
	static class ContextFactory
	{

		public static GraphicsContext CreateContext(IWindowInfo window)
		{
			var color = new ColorFormat(8, 8, 8, 8);
			var accum = ColorFormat.Empty;
			var depth = 24;
			var stencil = 8;
			var samples = 1;

			Exception innerException = null;
			try
			{
				var mode = new GraphicsMode(color, depth, stencil, samples, accum);
				var result = new GraphicsContext(mode, window, 2, 0, GraphicsContextFlags.Default);

				result.MakeCurrent(window);
				result.LoadAll();

				return result;
			}
			catch (Exception e)
			{
				innerException = e;				
			}

			throw new ContextCreationFailed(innerException);
		}
	}
}
