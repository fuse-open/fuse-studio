using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using CoreVideo;
using Foundation;
using OpenGL;
using OpenTK.Graphics.OpenGL;
using Outracks.Fusion.Mac;
using GL = OpenTK.Graphics.OpenGL.GL;

namespace Outracks.UnoHost.Mac.FusionSupport
{
	class UnoHostLayer : CAOpenGLLayer
	{
		readonly Action _draw;
		int _dirtyVersion;
		int _version;

		public UnoHostLayer(Action draw)
		{
			_draw = draw;
			base.Asynchronous = true;
			base.NeedsDisplayOnBoundsChange = true;
		}

		public void MakeDirty()
		{
			Interlocked.Increment(ref _dirtyVersion);
		}

		public override bool CanDrawInCGLContext(
			CGLContext glContext,
			CGLPixelFormat pixelFormat,
			double timeInterval,
			ref CVTimeStamp timeStamp)
		{
			return _version != _dirtyVersion;
		}

		public override void DrawInCGLContext(CGLContext glContext, CGLPixelFormat pixelFormat, double timeInterval, ref CVTimeStamp timeStamp)
		{
			_version = _dirtyVersion;
			_draw();
			base.DrawInCGLContext(glContext, pixelFormat, timeInterval, ref timeStamp);
		}
	}

	public class ServerView : NSView, ICursorRectControl
	{
		public readonly ISubject<NSEvent> Events = new Subject<NSEvent>();

		readonly NSTrackingArea _trackingArea;
		readonly SurfaceCache _surfaceCache;
		readonly Quad _quad = new Quad();
		readonly UnoHostLayer _backedLayer;

		readonly ISubject<Size<Points>> _size = new ReplaySubject<Size<Points>>(1);
		public IObservable<Size<Points>> Size { get { return _size.DistinctUntilChanged(); } }

		readonly BehaviorSubject<FocusState> _focus = new BehaviorSubject<FocusState>(FocusState.Blurred);
		public IObservable<FocusState> Focus
		{
			get { return _focus; }
		}

		readonly BehaviorSubject<Ratio<Pixels, Points>> _density;
		public IObservable<Ratio<Pixels, Points>> Density { get { return _density.DistinctUntilChanged(); } }

		public override bool IsOpaque
		{
			get { return true; }
		}

		internal ServerView(SurfaceCache surfaceCache, Optional<CGRect> frame = default(Optional<CGRect>))
			: base(frame.Or(new CGRect()))
		{
			_surfaceCache = surfaceCache;
			_backedLayer = new UnoHostLayer(OnDraw);
			surfaceCache.SurfaceSwapped.Subscribe(_ => _backedLayer.MakeDirty());
			WantsLayer = true;

			_trackingArea = new NSTrackingArea(
				new CGRect(),
				NSTrackingAreaOptions.ActiveInKeyWindow | NSTrackingAreaOptions.InVisibleRect | NSTrackingAreaOptions.MouseMoved | NSTrackingAreaOptions.MouseEnteredAndExited,
				this,
				null);
			AddTrackingArea(_trackingArea);

			_density = new BehaviorSubject<Ratio<Pixels, Points>>(GetDensity());
		}

		public Action<NSView> ResetCursorRectsHandler { get; set; }
		public override void ResetCursorRects()
		{
			if (ResetCursorRectsHandler != null)
				ResetCursorRectsHandler(this);
		}

		[Export("viewDidChangeBackingProperties")]
		public void ViewDidChangeBackingProperties()
		{
			var density = GetDensity();
			_density.OnNext(density);
			_backedLayer.ContentsScale = (float) density;

			// Force update size of backed layer when density changes
			// Issue: https://github.com/fusetools/Fuse/issues/2738
			SetFrameSize(Frame.Size + new CGSize(1, 1));
			SetFrameSize(Frame.Size - new CGSize(1, 1));
		}

		Ratio<Pixels, Points> GetDensity()
		{
			return new Ratio<Pixels, Points>(ConvertSizeToBacking(new CGSize(1, 1)).Width);
		}

		public override CALayer MakeBackingLayer()
		{
			return _backedLayer;
		}

		#region EventOverriding
		public override bool AcceptsFirstResponder()
		{
			return true;
		}

		public override bool BecomeFirstResponder()
		{
			_focus.OnNext(FocusState.Focused);
			return base.BecomeFirstResponder();
		}

		public override bool ResignFirstResponder()
		{
			_focus.OnNext(FocusState.Blurred);
			return base.ResignFirstResponder();
		}

		public override bool AcceptsFirstMouse(NSEvent theEvent)
		{
			return true;
		}

		public override void MouseDown(NSEvent theEvent)
		{
			ForwardEvent(theEvent);
			if(theEvent.Window != null)
				theEvent.Window.MakeFirstResponder(this);

			while (true)
			{
				// Custom event loop to handle mouse dragging outside of window
				theEvent = Window.NextEventMatchingMask(NSEventMask.LeftMouseDragged | NSEventMask.LeftMouseUp);

				if(theEvent == null)
					break;

				ForwardEvent(theEvent);
				if (theEvent.Type == NSEventType.LeftMouseUp)
					break;
			}
		}

		public override void MouseUp(NSEvent theEvent)
		{
			ForwardEvent(theEvent);
		}

		public override void MouseDragged(NSEvent theEvent)
		{
			ForwardEvent(theEvent);
		}

		public override void RightMouseDown(NSEvent theEvent)
		{
			_focus.OnNext(FocusState.Focused); // Give focus on right click too
			base.RightMouseDown(theEvent); // Handle context menu
		}

		public override void RightMouseUp(NSEvent theEvent)
		{
			base.RightMouseUp(theEvent); // Handle context menu
			ForwardEvent(theEvent);
		}

		public override void RightMouseDragged(NSEvent theEvent)
		{
			ForwardEvent(theEvent);
		}

		public override void OtherMouseDown(NSEvent theEvent)
		{
			ForwardEvent(theEvent);
		}

		public override void OtherMouseUp(NSEvent theEvent)
		{
			ForwardEvent(theEvent);
		}

		public override void OtherMouseDragged(NSEvent theEvent)
		{
			ForwardEvent(theEvent);
		}

		public override void MouseEntered(NSEvent theEvent)
		{
			ForwardEvent(theEvent);
		}

		public override void MouseMoved(NSEvent theEvent)
		{
			ForwardEvent(theEvent);
		}

		public override void MouseExited(NSEvent theEvent)
		{
			ForwardEvent(theEvent);
		}

		public override void ScrollWheel(NSEvent theEvent)
		{
			base.ScrollWheel(theEvent);
			ForwardEvent(theEvent);
		}

		public override void KeyDown(NSEvent theEvent)
		{
			// Clone key events to avoid crashing the application when reading
			// Characters or CharactersIgnoringModifiers on a different thread
			ForwardEvent(NSEvent.KeyEvent(
				theEvent.Type,
				theEvent.LocationInWindow,
				theEvent.ModifierFlags,
				theEvent.Timestamp,
				theEvent.WindowNumber,
				theEvent.Context,
				theEvent.Characters,
				theEvent.CharactersIgnoringModifiers,
				theEvent.IsARepeat,
				theEvent.KeyCode
			));
		}

		public override void KeyUp(NSEvent theEvent)
		{
			// Clone key events to avoid crashing the application when reading
			// Characters or CharactersIgnoringModifiers on a different thread
			ForwardEvent(NSEvent.KeyEvent(
				theEvent.Type,
				theEvent.LocationInWindow,
				theEvent.ModifierFlags,
				theEvent.Timestamp,
				theEvent.WindowNumber,
				theEvent.Context,
				theEvent.Characters,
				theEvent.CharactersIgnoringModifiers,
				theEvent.IsARepeat,
				theEvent.KeyCode
			));
		}
		#endregion

		void ForwardEvent(NSEvent theEvent)
		{
			Events.OnNext(EventIntercepter.TransformLocationOfEvent(this, theEvent));
		}

		public override void SetFrameSize(CGSize newSize)
		{
			base.SetFrameSize(newSize);
			_size.OnNext(new Size<Points>((double)newSize.Width, (double)newSize.Height));
		}

		public override void ViewWillDraw()
		{
			_backedLayer.MakeDirty();
		}

		void OnDraw()
		{
			GL.ClearColor(0.0f, 0.0f, 0.0f, .0f);
			GL.Clear(ClearBufferMask.ColorBufferBit);

			_surfaceCache.GetCurrentFrontTexture().Do(
				texture =>
				{
					try
					{
						var clipRect = Rectangle.FromSides<ClipSpaceUnits>(0, 0, 1, 1);

						_quad.Render(TextureTarget.TextureRectangle, texture, texture.Size,
							Rectangle.FromSides<ClipSpaceUnits>(-1, -1, 1, 1), clipRect);
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
					}
				});

			_surfaceCache.WipeUnusedSurfaces();
		}
	}
}
