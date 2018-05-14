using System;
using AppKit;

namespace Outracks.UnoHost.OSX.UnoView.RenderTargets
{
	interface IRenderTarget
	{
		/// <exception cref="ArgumentOutOfRangeException">Dimensions of size is less than 1</exception>
		int GetFramebufferHandle(Size<Pixels> size);

		void Flush(NSOpenGLContext ctx);
	}
}