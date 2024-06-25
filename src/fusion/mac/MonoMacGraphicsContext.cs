using System;
using System.Drawing;
using Foundation;
using AppKit;
using ObjCRuntime;
using OpenGL;

namespace Outracks.UnoCore.MonoMacSupport
{
    class MonoMacGraphicsContext : Uno.Runtime.Implementation.GraphicsContextHandle
    {
        UnoGLView _view;

        public MonoMacGraphicsContext(UnoGLView view)
        {
            _view = view;
        }

        public override int GetBackbufferGLHandle()
        {
            return 0;
        }

        public override Uno.Int2 GetBackbufferSize()
        {
            return _view.DrawableSize;
        }

        public override Uno.Int2 GetBackbufferOffset()
        {
            return new Uno.Int2(0, 0);
        }

        public override Uno.Recti GetBackbufferScissor()
        {
            return new Uno.Recti(new Uno.Int2(0, 0), _view.DrawableSize);
        }

        public override int GetRealBackbufferHeight()
        {
            return _view.DrawableSize.Y;
        }
    }
}

