using System;
using OpenTK.Graphics;
using OpenTK.Platform;

namespace Outracks.UnoHost.Windows
{
	static class ContextFactory
	{

		public static GraphicsContext CreateContext(IWindowInfo window)
		{
			Exception innerException = null;
			try
			{
				var result = new GraphicsContext(new GraphicsMode(32, 24), window, 2, 0, GraphicsContextFlags.Angle);

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
