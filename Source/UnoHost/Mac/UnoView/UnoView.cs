using System;
using System.Diagnostics;
using System.Drawing;
using System.Reactive.Subjects;
using AppKit;
using CoreGraphics;
using OpenGL;
using Outracks.Fusion;
using Outracks.UnoHost.OSX.Protocol;
using Uno;
using Uno.Diagnostics;
using Uno.Platform;
using Uno.Runtime.Implementation;
using Uno.Runtime.Implementation.Internal;
using Key = Uno.Platform.Key;

using System.Reactive.Concurrency;

namespace Outracks.UnoHost.OSX.UnoView
{
	using RenderTargets;

	sealed class UnoView : DisplayLinkView, IAppHost, IUnoCallbacks
	{
		int _currentFrame = 0;
		readonly IRenderTarget _renderTarget;
		readonly IReport _log;

		NSEventModifierMask _modifierFlags;
		
		readonly PlatformWindowHandleImpl _unoWindow;
		readonly GraphicsContextHandleImpl _unoGraphics;

		readonly Subject<double> _perFrame = new Subject<double>();
		readonly Stopwatch _stopwatch = Stopwatch.StartNew();

		public IObservable<double> PerFrame
		{
			get { return _perFrame; }
		} 
		
		public PlatformWindowHandle GetPlatformWindow()
		{
			return _unoWindow;
		}

		public GraphicsContextHandle GetGraphicsContext()
		{
			return _unoGraphics;
		}

		public UnoView(IScheduler dispatcher, IReport log, IRenderTarget renderTarget)
			: base(v => dispatcher.Schedule(v), new CGRect(0, 0, 0, 0))
		{
			_renderTarget = renderTarget;
			_log = log;
			
			_unoWindow = new PlatformWindowHandleImpl(this);
			_unoGraphics = new GraphicsContextHandleImpl(_unoWindow);

			Reshape();
		}

		public void Initialize(UnoHostProject project, IObserver<OpenGlVersion> glObserver)
		{
			IGL gl = new MonoMacGL();
#if DEBUG
			GL.Initialize(gl, true);
#else
			GL.Initialize(gl, false);
#endif
			ApplicationContext.Initialize(this);
			glObserver.OnNext(
				new OpenGlVersion(
					gl.GetString(GLStringName.Version),
					gl.GetString(GLStringName.Vendor),
					gl.GetString(GLStringName.Renderer)));
			
		}

		public override bool AcceptsFirstResponder()
		{
			return true;
		}

		#region Cursor
		NSCursor _cursor;
		public override void ResetCursorRects()
		{
			if (_cursor != null)
			{
				AddCursorRect(Bounds, _cursor);
			}
			else
			{
				base.ResetCursorRects();
			}
		}

		public void SetCursor(Cursor cursor)
		{
			_cursor = cursor.ToCursor();
			Window.InvalidateCursorRectsForView(this);
		}
		#endregion

		#region Resize
		Size<Pixels> _unoSize = new Size<Pixels>(1,1);
		Ratio<Pixels, Points> _unoDensity = new Ratio<Pixels, Points>(1);

		readonly BehaviorSubject<Size<Points>> _size = new BehaviorSubject<Size<Points>>(new Size<Points>(0, 0));

		public IObservable<Size<Points>> Size
		{
			get { return _size; }
		}

		public void Resize(SizeData sizeData)
		{
			_unoSize = sizeData.Size * sizeData.Density;
			_unoDensity = sizeData.Density;

			SetFrameSize(sizeData.Size.ToSize());

			_unoWindow.ChangeSize(_unoSize, _unoDensity);
			_size.OnNext(sizeData.Size);

			Reshape();
		}
		#endregion

		#region Update and Draw
		protected override void OnDraw()
		{
			OpenGLContext.CGLContext.Lock().ThrowIfFailed("CGLContext.Lock()");
			try
			{
				OpenGLContext.MakeCurrentContext();

				_perFrame.OnNext(_stopwatch.Elapsed.TotalSeconds);
				
				_log.TrySomethingBlocking(Bootstrapper.OnUpdate);

				if (_unoWindow.Size.HasZeroArea())
					return;

				if(Uno.Application.Current != null && Bootstrapper.DrawNextFrame)
				{
					_unoGraphics.ChangeBackbuffer((GLFramebufferHandle) _renderTarget.GetFramebufferHandle(_unoWindow.Size));
					_log.TrySomethingBlocking(Bootstrapper.OnDraw);
					_renderTarget.Flush(OpenGLContext);
				}
			}
			finally
			{
				++_currentFrame;
				OpenGLContext.CGLContext.Unlock().ThrowIfFailed("CGLContext.Unlock()");
			}
		}

		#endregion

		#region Keyboard and Text Input
		public override void KeyDown(NSEvent theEvent)
		{
			_modifierFlags = theEvent.ModifierFlags;
			if (_unoWindow.IsTextInputActive())
			{
				theEvent.InterpretAsTextEvent().Do(
					text => _log.Try(() => Bootstrapper.OnTextInput(_unoWindow, text), false),
					() => { });
			}

			theEvent.InterpretAsKeyEvent().Do(
				key => _log.Try(() => Bootstrapper.OnKeyDown(_unoWindow, (Uno.Platform.Key)key), false),
				() => { });
		}

		public override void KeyUp(NSEvent theEvent)
		{
			_modifierFlags = theEvent.ModifierFlags;

			theEvent.InterpretAsKeyEvent().Do(
				key => _log.Try(() => Bootstrapper.OnKeyUp(_unoWindow, (Uno.Platform.Key)key), false),
				() => { });
		}
		#endregion

		#region Mouse Down
		public override void MouseDown(NSEvent a) { MouseDownInternal(a, Uno.Platform.MouseButton.Left); }
		public override void RightMouseDown(NSEvent a) { MouseDownInternal(a, Uno.Platform.MouseButton.Right); }
		public override void OtherMouseDown(NSEvent a) { MouseDownInternal(a, Uno.Platform.MouseButton.Middle); }

		void MouseDownInternal(NSEvent a, Uno.Platform.MouseButton button)
		{
			_modifierFlags = a.ModifierFlags;

			var pos = GetPosition(a);
			_log.Try(() => Bootstrapper.OnMouseDown(_unoWindow, (int)pos.X, (int)pos.Y, button), false);
		}
		#endregion

		#region Mouse Up
		public override void MouseUp(NSEvent a) { MouseUpInternal(a, Uno.Platform.MouseButton.Left); }
		public override void RightMouseUp(NSEvent a) { MouseUpInternal(a, Uno.Platform.MouseButton.Right); }
		public override void OtherMouseUp(NSEvent a) { MouseUpInternal(a, Uno.Platform.MouseButton.Middle); }

		void MouseUpInternal(NSEvent a, Uno.Platform.MouseButton button)
		{
			_modifierFlags = a.ModifierFlags;

			var pos = GetPosition(a);
			_log.Try(() => Bootstrapper.OnMouseUp(_unoWindow, (int)pos.X, (int)pos.Y, button), false);
		}
		#endregion

		#region Mouse Moved
		public override void MouseDragged(NSEvent a) { MouseMovedInternal(a); }
		public override void RightMouseDragged(NSEvent a) { MouseMovedInternal(a); }
		public override void OtherMouseDragged(NSEvent a) { MouseMovedInternal(a); }
		public override void MouseMoved(NSEvent a) { MouseMovedInternal(a); }

		int _lastMouseMovedFrame = -1;
		void MouseMovedInternal(NSEvent a)
		{
			_modifierFlags = a.ModifierFlags;

			// Only send one mouse moved event per frame (ignore the rest)
			// Issue: https://github.com/fusetools/Fuse/issues/2479 
			if (_currentFrame != _lastMouseMovedFrame)
			{
				var pos = GetPosition(a);
				_log.Try(() => Bootstrapper.OnMouseMove(_unoWindow, (int)pos.X, (int)pos.Y), false);
				_lastMouseMovedFrame = _currentFrame;
			}
		}
		#endregion

		#region Mouse Enter/Leave
		public override void MouseEntered(NSEvent a)
		{
			_modifierFlags = a.ModifierFlags;

			//var pos = GetPosition(a);
			//Log.Try(() => Bootstrapper.OnMouseEnter(_unoWindow, (int)pos.X, (int)pos.Y), false);
		}

		public override void MouseExited(NSEvent a)
		{
			_modifierFlags = a.ModifierFlags;
			_log.Try(() => Bootstrapper.OnMouseOut(_unoWindow), false);
		}
		#endregion

		#region Mouse Scroll
		public override void ScrollWheel(NSEvent a)
		{
			_modifierFlags = a.ModifierFlags;
			var deltaMode = a.HasPreciseScrollingDeltas ? WheelDeltaMode.DeltaPixel : WheelDeltaMode.DeltaLine;

			_modifierFlags = a.ModifierFlags;
			_log.Try(() => Bootstrapper.OnMouseWheel(_unoWindow, (float)a.ScrollingDeltaX, (float)a.ScrollingDeltaY, (int)deltaMode), false);
		}
		#endregion

		public bool GetKeyStateOnlyWorksForModifiers(Key key)
		{
			switch (key)
			{
				case Key.ControlKey:
					return (_modifierFlags & NSEventModifierMask.ControlKeyMask) != 0;
				case Key.ShiftKey:
					return (_modifierFlags & NSEventModifierMask.ShiftKeyMask) != 0;
				case Key.AltKey:
					return (_modifierFlags & NSEventModifierMask.AlternateKeyMask) != 0;
				case Key.OSKey:
				case Key.MetaKey:
					return (_modifierFlags & NSEventModifierMask.CommandKeyMask) != 0;
				default:
					return false;
			}
		}

		Point<Pixels> GetPosition(NSEvent a)
		{
			var p = a.LocationInWindow;
			var hostPoint = new Point<Points>((double)p.X, (double)p.Y) * _unoDensity;
			return new Point<Pixels>(hostPoint.X, (double)_unoSize.Height - (double)hostPoint.Y);
		}
	}

	public static class CursorExtensionMethods
	{
		public static NSCursor ToCursor(this Cursor cursor)
		{
			switch (cursor) 
			{
			case Cursor.Default:
				return NSCursor.ArrowCursor;
			case Cursor.Grab:
				return NSCursor.OpenHandCursor;
			case Cursor.Grabbing:
				return NSCursor.ClosedHandCursor;
			case Cursor.Pointer:
				return NSCursor.PointingHandCursor;
			case Cursor.SizeH:
				return NSCursor.ResizeLeftRightCursor;
			case Cursor.SizeV:
				return NSCursor.ResizeUpDownCursor;
			case Cursor.Text:
				return NSCursor.IBeamCursor;
			default:
				throw new NotSupportedException ();
			}
		}
	}
}
