using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Outracks.Fusion.Windows
{
	static class PointerImplementation
	{
		static Dispatcher _dispatcher;

		public static void Initialize(Dispatcher dispatcher)
		{
			_dispatcher = dispatcher;

			Pointer.Implementation.MakeHittable = MakeHittable;
		}

		static IControl MakeHittable(IControl control, Space space, Pointer.Callbacks callbacks)
		{
			return Control.Create(location =>
			{
				var view = new Canvas()
				{
					Background = Brushes.Transparent,
				};

				view.MouseDown += (s, a) =>
				{
					view.CaptureMouse();
					callbacks.OnPressed(new Pointer.OnPressedArgs(ToPoint(a, view, space), a.ClickCount));
					a.Handled = true;
				};

				view.MouseUp += (s, a) =>
				{
					callbacks.OnReleased();
					a.Handled = true;
					view.ReleaseMouseCapture();

					if (a.ChangedButton == MouseButton.Right)
					{
						if (view.ContextMenu == null) return;
						view.ContextMenu.IsOpen = true;
					}
				};

				view.MouseMove += (s, a) =>
				{
					callbacks.OnMoved(ToPoint(a, view, space));
					a.Handled = true;

					if (view.IsMouseCaptured && a.LeftButton == MouseButtonState.Pressed)
					{
						callbacks.OnDragged().Do(data =>
							DragDrop.DoDragDrop(view, new DraggingImplementation.DragData(data), DragDropEffects.All));
					}
				};

				view.MouseEnter += (s, a) =>
				{
					callbacks.OnEntered(ToPoint(a, view, space));
					a.Handled = true;
				};

				view.MouseLeave += (s, a) =>
				{
					callbacks.OnExited(ToPoint(a, view, space));
					a.Handled = true;
				};

				view.GotMouseCapture += (s, a) =>
				{
					callbacks.OnGotFocus();
					a.Handled = true;
				};

				view.LostMouseCapture += (s, a) =>
				{
					callbacks.OnLostFocus();
					a.Handled = true;
				};

				location.BindNativeDefaults(view, _dispatcher);

				control.Mount(
					new MountLocation.Mutable
					{
						AvailableSize = location.AvailableSize,
						IsRooted = location.IsRooted,
						NativeFrame = ObservableMath.RectangleWithSize(location.NativeFrame.Size),
					});

				var contentHandle = control.NativeHandle as FrameworkElement;
				if (contentHandle != null)
					_dispatcher.Enqueue(() => view.Children.Add(contentHandle));

				return view;
			}).WithSize(control.DesiredSize);
		}

		public static Point<Points> ToPoint(MouseEventArgs args, IInputElement element, Space space)
		{
			var p = space == Space.Local ? args.GetPosition(element) : args.GetPosition(null);
			return Point.Create<Points>(p.X, p.Y);
		}
	}
}