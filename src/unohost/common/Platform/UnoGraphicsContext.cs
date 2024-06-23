using OpenGL;
using Uno;
using Uno.Platform;

namespace Outracks.UnoHost
{
	public class UnoGraphicsContext : GraphicsContextBackend
	{
		readonly UnoWindow _unoWindow;
		GLFramebufferHandle _handle;

		public UnoGraphicsContext(UnoWindow window)
		{
			_unoWindow = window;
		}

        public void ChangeBackbuffer(GLFramebufferHandle handle)
        {
            _handle = handle;
	        var app = Application.Current;
			if (app != null && app.GraphicsController != null)
				app.GraphicsController.UpdateBackbuffer();
        }

		public override GLFramebufferHandle GetBackbufferGLHandle()
		{
			return _handle;
		}

		public override Int2 GetBackbufferSize()
		{
			return _unoWindow.GetClientSize();
		}

		public override Int2 GetBackbufferOffset()
		{
			return new Int2(0,0);
		}

		public override Recti GetBackbufferScissor()
		{
			return new Recti(new Int2(0,0), _unoWindow.GetClientSize());
		}

		public override int GetRealBackbufferHeight()
		{
			return _unoWindow.GetClientSize().Y;
		}
	}
}
