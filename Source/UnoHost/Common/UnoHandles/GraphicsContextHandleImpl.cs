using OpenGL;
using Uno.Runtime.Implementation;

namespace Outracks.UnoHost
{
	public class GraphicsContextHandleImpl : GraphicsContextHandle
	{
		readonly PlatformWindowHandleImpl _unoWindow;
		GLFramebufferHandle _handle;

		public GraphicsContextHandleImpl(PlatformWindowHandleImpl window)
		{
			_unoWindow = window;
		}

        public void ChangeBackbuffer(GLFramebufferHandle handle)
        {
            _handle = handle;
	        var app = Uno.Application.Current;
			if (app != null && app.GraphicsController != null)
				app.GraphicsController.UpdateBackbuffer();
        }

		public override GLFramebufferHandle GetBackbufferGLHandle()
		{
			return _handle;
		}

		public override Uno.Int2 GetBackbufferSize()
		{
			return _unoWindow.GetClientSize();
		}

		public override Uno.Int2 GetBackbufferOffset()
		{
			return new Uno.Int2(0,0);
		}

		public override Uno.Recti GetBackbufferScissor()
		{
			return new Uno.Recti(new Uno.Int2(0,0), _unoWindow.GetClientSize());
		}

		public override int GetRealBackbufferHeight()
		{
			return _unoWindow.GetClientSize().Y;
		}
	}
}
