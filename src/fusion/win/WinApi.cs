using System;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace Outracks.Fusion.Windows
{
	public static class WinApi
	{
		public static IntPtr HWND_TOP = new IntPtr(0);
		public static IntPtr HWND_BOTTOM = new IntPtr(1);
		public static IntPtr HWND_TOPMOST = new IntPtr(-1);
		public static IntPtr HWND_NOTOPMOST = new IntPtr(-2);

		/*
		public static IObservable<Rectangle<Points>> NativeScreenFrame(this FrameworkElement element, System.Windows.Window window)
		{
			return Observable.Defer(
				() =>
				{
					var windowMoved = Observable.FromEventPattern(window, "LocationChanged").Select(_ => Unit.Default);
					var layoutUpdated = Observable.FromEventPattern(element, "LayoutUpdated").Select(_ => Unit.Default);
					var couldHaveMovedOnscreen = windowMoved.Merge(layoutUpdated).StartWith(Unit.Default);

					return couldHaveMovedOnscreen
						.Where(_ => element.Dispatcher.Invoke(() => PresentationSource.FromVisual(element) != null))
						.Select(_ => element.Dispatcher.Invoke(() =>
						{
							var dpi = element.GetDpi();
							var topLeft = element.PointToScreen(new System.Windows.Point(0, 0));
							var bottomRight = element.PointToScreen(new System.Windows.Point(element.ActualWidth, element.ActualHeight));

							return Rectangle.FromSides<Pixels>(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y)
								.Mul(dpi.Reciprocal());
						}))
						.DistinctUntilChanged()
						.Replay(1)
						.RefCount();
				});
		}

		public static IObservable<Rectangle<Points>> NativeWindowFrame(this FrameworkElement element)
		{
			return Observable
				.Defer(() => element.Dispatcher.Invoke(() =>
					Observable
						.FromEventPattern(element, "LayoutUpdated")
						.Select(_ => Unit.Default)
						.StartWith(Unit.Default)
						.Select(_ =>
							element.Dispatcher.Invoke(() =>
							{
								var window = System.Windows.Window.GetWindow(element);
								var topLeft = element.TranslatePoint(new System.Windows.Point(0, 0), window);
								var bottomRight = element.TranslatePoint(
									new System.Windows.Point(element.ActualWidth, element.ActualHeight),
									window);
								return Rectangle.FromSides<Points>(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
							}))))
				.DistinctUntilChanged()
				.Replay(1)
				.RefCount();
		}
		*/

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		public static void BringInFrontOf(IntPtr hwnd, IntPtr parent)
		{
			SetWindowPos(hwnd, parent, 0, 0, 0, 0, /*SWP_SHOWWINDOW |*/ SWP_NOACTIVATE | SWP_NOSIZE | SWP_NOMOVE /*| SWP_ASYNCWINDOWPOS*/);
		}

		public static void ActivateWindow(IntPtr hwnd)
		{
			SetWindowPos(hwnd, HWND_TOP, 0, 0, 0, 0, SWP_SHOWWINDOW | SWP_NOSIZE | SWP_NOMOVE);
		}
		public static IDisposable SubscribeWindowFrame(IntPtr hwnd, IObservable<Rectangle<Points>> frame, Optional<IntPtr> parent = default(Optional<IntPtr>))
		{
			return frame
				.Select(r => Rectangle.FromPositionSize(r.Position.Round(), r.Size.Round()))
				.DistinctUntilChanged()
				.Subscribe(s =>
				{
					var flags = SWP_ASYNCWINDOWPOS | SWP_NOACTIVATE;
					if (!parent.HasValue)
						flags |= SWP_NOZORDER;

					SetWindowPos(
						hwnd,
						parent.Or(IntPtr.Zero),
						(int)s.Left(), (int)s.Top(),
						(int) s.Width, (int) s.Height,
						flags);
				});
		}

		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern int GetWindowLong(IntPtr hWnd, int nIndex);


		public static IDisposable SetWindowParent(IntPtr child, IntPtr parent)
		{
			SetParent(child, parent);
			return Disposable.Create(() => SetParent(child, IntPtr.Zero));
		}

		[DllImport("user32")]
		public static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);

		[DllImport("user32")]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);


		public static bool SetWindowPos(IntPtr hWnd, Optional<Point<Pixels>> position, Optional<Size<Pixels>> size, Optional<bool> hasZeroArea, int uFlags)
		{
			var p = position.Or(new Point<Pixels>(0,0));
			var s = size.Or(new Size<Pixels>(0,0));

			if (hasZeroArea == true)
				uFlags |= SWP_HIDEWINDOW;

			if (hasZeroArea == false)
				uFlags |= SWP_SHOWWINDOW;

			if (position.HasValue || size.HasValue || hasZeroArea.HasValue)
				return SetWindowPos(hWnd, HWND_TOP, (int)p.X, (int)p.Y, (int)s.Width, (int)s.Height, uFlags
					| (position.HasValue ? 0 : SWP_NOMOVE)
					| (size.HasValue ? 0 : SWP_NOSIZE));

			return false;
		}


		public static bool SetWindowPos(IntPtr hWnd, Rectangle<Points> frame, int uFlags)
		{
			return SetWindowPos(hWnd, HWND_TOP, (int) frame.Left(), (int) frame.Top(), (int) frame.Width, (int) frame.Height, uFlags);

		}

		[DllImport("user32.dll")]
		public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

		public const int SWP_ASYNCWINDOWPOS = 0x4000;
		public const int SWP_NOZORDER = 0x0004;
		public const int SWP_NOACTIVATE = 0x0010;
		const int SWP_HIDEWINDOW = 0x0080;
		public const int SWP_NOMOVE = 0x0002;
		public const int SWP_NOSIZE = 0x0001;
		public const int SWP_NOCOPYBITS = 0x0100;
		const int SWP_SHOWWINDOW = 0x0040;
		public const int GWL_STYLE = -16;
		public const int GWL_EXSTYLE = -20;
		public const int WS_CLIPCHILDREN = 0x02000000;
		public const int WS_EX_TRANSPARENT = 0x00000020;

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

		public enum ShowWindowEnum
		{
			Hide = 0,
			ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
			Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
			Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
			Restore = 9, ShowDefault = 10, ForceMinimized = 11
		};
	}

	static class VisualExtensions
	{
		public static Ratio<Pixels, Points> GetDpi(this Visual visual)
		{
			var source = PresentationSource.FromVisual(visual);
			var dpi = new Ratio<Pixels, Points>(1.0);
			if (source != null && source.CompositionTarget != null)
				dpi = new Ratio<Pixels, Points>(source.CompositionTarget.TransformToDevice.M11);

			return dpi;
		}
	}
}