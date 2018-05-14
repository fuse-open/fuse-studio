using System;
using System.ComponentModel;
using System.Reactive.Concurrency;
using AppKit;
//using Outracks.UnoHost.OSX.FusionSupport;

namespace Outracks.Fusion.OSX
{
	public interface ICursorRectControl
	{
		Action<NSView> ResetCursorRectsHandler { set; }
	}

	static class CursorsImplementation
	{
		public static void Initialize(IScheduler dispatcher)
		{
			Cursors.Implementation.Set = (self, cursor) =>
			{
				self.BindNativeProperty(dispatcher, "cursor", cursor, (NSView view, Cursor value) =>
				{
					var responder = view as IObservableResponder;

					if (responder != null)
					{	
						responder.ResetCursorRectsHandler = v => v.AddCursorRect(v.Bounds, value.ToCocoa());
					}
					else if (view is ICursorRectControl)
					{
						((ICursorRectControl)view).ResetCursorRectsHandler = v => v.AddCursorRect(v.Bounds, value.ToCocoa());
					}

					if (view.Window != null)
						view.Window.InvalidateCursorRectsForView(view);
				});

				return self;
			};
		}

		public static NSCursor ToCocoa(this Cursor self)
		{
			switch (self)
			{
				case Cursor.Normal:
					return NSCursor.ArrowCursor;
				case Cursor.ResizeVertically:
					return NSCursor.ResizeUpDownCursor;
				case Cursor.ResizeHorizontally:
					return NSCursor.ResizeLeftRightCursor;
				case Cursor.Grab:
					return NSCursor.OpenHandCursor;
				case Cursor.Grabbing:
					return NSCursor.ClosedHandCursor;
				case Cursor.Pointing:
					return NSCursor.PointingHandCursor;
				case Cursor.Text:
					return NSCursor.IBeamCursor;
				default:
					throw new InvalidEnumArgumentException("self", (int)self, typeof(Cursor));
			}
		}
	}
}
