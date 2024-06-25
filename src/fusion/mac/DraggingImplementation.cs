using System;
using System.Reactive.Concurrency;
using AppKit;
using CoreGraphics;
using Foundation;

namespace Outracks.Fusion.Mac
{
	class DragSource : NSDraggingSource
	{
		readonly NSView _originView;
		readonly Space _space;
		readonly Pointer.Callbacks _callbacks;
		public readonly object Data;

		public DragSource(IntPtr handle) : base(handle)
		{
		}

		public DragSource(NSView originView, Space space, Pointer.Callbacks callbacks, object data)
		{
			_originView = originView;
			_space = space;
			_callbacks = callbacks;
			Data = data;
		}

		[Action("draggingSession:sourceOperationMaskForDraggingContext:")]
		public NSDragOperation DraggingSessionSourceOperationMask(NSDraggingSession session, NSDraggingContext context)
		{
			if (context == NSDraggingContext.WithinApplication)
			{
				return NSDragOperation.Private;
			}

			return NSDragOperation.None;
		}

		[Action("draggingSession:endedAtPoint:operation:")]
		public void DraggingSessionEnded(NSDraggingSession session, CGPoint screenPoint, NSDragOperation operation)
		{
			_callbacks.OnReleased();
		}

		[Action("draggingSession:movedToPoint:")]
		public void DraggingSessionMoved(NSDraggingSession session, CGPoint screenPoint)
		{
			ToPoint(screenPoint).Do(_callbacks.OnMoved);
		}

		[Action("draggingSession:willBeginAtPoint:")]
		public void DraggingSessionWillBegin(NSDraggingSession session, CGPoint screenPoint)
		{
		}

		Optional<Point<Points>> ToPoint(CGPoint screenPoint)
		{
			var window = _originView.Window;
			if (window == null)
			{
				// The view isn't in any window, so there's nothing sensible to return.
				return Optional.None();
			}
			var windowPoint = window.ConvertRectFromScreen(new CGRect(screenPoint, new CGSize(0, 0))).Location;

			if (_space == Space.Local)
			{
				var localPointCocoa = _originView.ConvertPointFromView(windowPoint, null);
				return Point.Create<Points>((double)localPointCocoa.X, (double)(_originView.Frame.Height - localPointCocoa.Y));
			}
			else
			{
				return Point.Create<Points>((double)windowPoint.X, (double)(window.Frame.Height - windowPoint.Y));
			}
		}
	}

	class DragDestinationView : NSView
	{
		readonly Func<object, bool> _canDrop;
		public Action<object> OnDrop { get; set; }
		public Action<object> OnEnter { get; set; }
		public Action<object> OnExit { get; set; }

		public DragDestinationView(IntPtr handle) : base(handle)
		{
		}

		public DragDestinationView(Func<object, bool> canDrop)
		{
			_canDrop = canDrop;
			base.RegisterForDraggedTypes(new []
			{
				"public.data"
			});
		}

		public override NSDragOperation DraggingEntered(NSDraggingInfo sender)
		{
			var data = sender.DraggingPasteboard.GetStringForType("public.data");
			if (data != "fuse-drag-n-drop")
				return NSDragOperation.None;

			var source = (DragSource) sender.DraggingSource;
			if (_canDrop(source.Data) == false)
				return NSDragOperation.None;

			OnEnter(source.Data);

			return NSDragOperation.Private;
		}

		public override void DraggingExited(NSDraggingInfo sender)
		{
			var data = sender.DraggingPasteboard.GetStringForType("public.data");
			if (data != "fuse-drag-n-drop")
				return;

			var source = (DragSource)sender.DraggingSource;
			if (_canDrop(source.Data) == false)
				return;

			OnExit(source.Data);

			base.DraggingExited(sender);
		}

		public override bool PerformDragOperation(NSDraggingInfo sender)
		{
			try
			{
				var source = (DragSource)sender.DraggingSource;
				OnDrop(source.Data);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}

	static class DraggingImplementation
	{
		public static void Initialize(IScheduler dispatcher)
		{
			Dragging.Implementation.OnDragOver = (control, canDrop, drop, enter, leave) =>
			{
				control = Layout.Layer(control).WithSize(control.DesiredSize);
				return Control.Create(location =>
				{
					control.Mount(
						new MountLocation.Mutable
						{
							AvailableSize = location.AvailableSize,
							IsRooted = location.IsRooted,
							NativeFrame = ObservableMath.RectangleWithSize(location.NativeFrame.Size),
						});

					Func<object, bool> canDropNow = o => false;
					control.BindNativeProperty(Fusion.Application.MainThread, "canDrop", canDrop, c => canDropNow = c);

					var nativeHandle = (NSView)control.NativeHandle;

					var view = new DragDestinationView(o => canDropNow(o))
					{
						OnDrop = drop,
						OnEnter = enter,
						OnExit = leave
					};

					view.AddSubview(nativeHandle);

					location.BindNativeDefaults(view, dispatcher);

					return view;
				}).WithSize(control.DesiredSize);
			};
		}

	}
}
