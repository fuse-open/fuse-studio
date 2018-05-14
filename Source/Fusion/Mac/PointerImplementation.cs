using System;
using System.Reactive.Concurrency;
using AppKit;
using Foundation;
using System.Drawing;
using CoreGraphics;

namespace Outracks.Fusion.OSX
{
	static class PointerImplementation
	{
		static IScheduler _dispatcher;

		public static void Initialize(IScheduler dispatcher)
		{
			_dispatcher = dispatcher;

			Pointer.Implementation.MakeHittable = MakeHittable;
		}

		static IControl MakeHittable(IControl control, Space space, Pointer.Callbacks callbacks)
		{
			return Control.Create(location =>
				{
					control.Mount(
						new MountLocation.Mutable
						{
							AvailableSize = location.AvailableSize,
							IsRooted = location.IsRooted,
							NativeFrame = ObservableMath.RectangleWithSize(location.NativeFrame.Size),
						});

					var nativeHandle = control.NativeHandle as NSView;
					
					var view = new HittableView(space, callbacks);
					if (nativeHandle != null)
						view.AddSubview(nativeHandle);

					location.BindNativeDefaults(view, _dispatcher);

					return view;
				}).WithSize(control.DesiredSize);
		}
	}

	class HittableView : NSDefaultView
	{
		readonly Space _space;
		readonly Pointer.Callbacks _callbacks;

		public HittableView(IntPtr handle) : base(handle)
		{
		}

		public HittableView(Space space, Pointer.Callbacks callbacks)
		{
			_space = space;
			_callbacks = callbacks;
			base.AutoresizesSubviews = false;

			var trackingArea = new NSTrackingArea(
				new CGRect(),
				NSTrackingAreaOptions.ActiveInKeyWindow | NSTrackingAreaOptions.InVisibleRect |
				NSTrackingAreaOptions.MouseMoved | NSTrackingAreaOptions.MouseEnteredAndExited,
				this,
				null);

			AddTrackingArea(trackingArea);
		}

		public override bool BecomeFirstResponder()
		{
			_callbacks.OnGotFocus();
			return base.BecomeFirstResponder();
		}

		public override bool ResignFirstResponder()
		{
			_callbacks.OnLostFocus();
			return base.ResignFirstResponder();
		}

		public override void MouseDown(NSEvent ev)
		{
			_callbacks.OnPressed(new Pointer.OnPressedArgs(ToPoint(ev), (int)ev.ClickCount));
		}

		public override void MouseUp(NSEvent ev)
		{
			_callbacks.OnReleased();
		}

		public override void MouseDragged(NSEvent ev)
		{
			_callbacks.OnDragged().MatchWith(
				data =>
				{
					var pasteBoardItem = new NSPasteboardItem();
					pasteBoardItem.SetStringForType("fuse-drag-n-drop", "public.data");
					var draggingItem = new NSDraggingItem(pasteBoardItem);

					var imgCache = BitmapImageRepForCachingDisplayInRect(Bounds);
					CacheDisplay(Bounds, imgCache);
					var img = new NSImage(imgCache.Size);
					img.LockFocus();
					imgCache.DrawInRect(Bounds, Bounds, NSCompositingOperation.Copy, 0.5f, true, new NSDictionary());
					img.UnlockFocus();

					draggingItem.SetDraggingFrame(new CGRect(Bounds.Location, img.Size), img);

					BeginDraggingSession(new[] { draggingItem }, ev, new DragSource(this, _space, _callbacks, data));
				},
				() =>
				{
					BeginDraggingSession(new NSDraggingItem[0], ev, new DragSource(this, _space, _callbacks, null));
				});
		}

		public override void MouseEntered(NSEvent ev)
		{
			_callbacks.OnEntered(ToPoint(ev));
		}

		public override void MouseExited(NSEvent ev)
		{
			_callbacks.OnExited(ToPoint(ev));
		}

		public override void MouseMoved(NSEvent ev)
		{
			_callbacks.OnMoved(ToPoint(ev));
		}

		Point<Points> ToPoint(NSEvent ev)
		{
			if (_space == Space.Local)
			{
				var localPointCocoa = ConvertPointFromView(ev.LocationInWindow, null);
				return Point.Create<Points>((double)localPointCocoa.X, (double)(Frame.Height - localPointCocoa.Y));
			}
			else
			{
				return Point.Create<Points>((double)ev.LocationInWindow.X, (double)(ev.Window.Frame.Height - ev.LocationInWindow.Y));
			}
		}
	}
}
