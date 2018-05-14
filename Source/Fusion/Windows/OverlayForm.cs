using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;

namespace Outracks.Fusion.Windows
{
	public class OverlayForm : Form
	{
		public OverlayForm()
		{
			WinApi.SetWindowLong(Handle, WinApi.GWL_STYLE, WinApi.WS_CLIPCHILDREN);
			WinApi.SetWindowLong(Handle, WinApi.GWL_EXSTYLE, 0);
		}

		/// <param name="m">The Windows <see cref="T:System.Windows.Forms.Message"/> to process. </param>
		protected override void WndProc(ref Message m)
		{
			const int WM_ERASEBKGND = 0x0014;

			switch (m.Msg)
			{
				case WM_ERASEBKGND:
					m.Result = new IntPtr(1);
					break;
				default:
					m.Result = WinApi.DefWindowProcW(m.HWnd, (uint)m.Msg, m.WParam, m.LParam);
					break;
			}
		}

		// Main thread
		public IDisposable BindTo(IntPtr unoHostHwnd, IntPtr controlHwnd, IControl control, IObservable<Ratio<Pixels, Points>> density)
		{
			var element = control.NativeHandle as FrameworkElement;
			if (element == null)
				throw new InvalidOperationException("Can't bind to an empty control");

			var frames = DataBinding
				.ObservableFromNativeEvent<object>(element, "LayoutUpdated")
				.StartWith(new object())
				.Select(_ =>
				{
					var r = element.GetScreenRect();

					return new ClipResult
					{
						BoundsInIntersection = r.Item1.RelativeTo(r.Item2.Position),
						IntersectionInParent = r.Item2
					};
				});

			WinApi.SetWindowParent(unoHostHwnd, Handle);
			WinApi.SetWindowParent(Handle, controlHwnd);
			WinApi.ShowWindow(unoHostHwnd, WinApi.ShowWindowEnum.ShowNoActivate);
		
			var flags = WinApi.SWP_NOZORDER | WinApi.SWP_NOACTIVATE;

			return Disposable.Combine
				(
					Disposable.Create(() => Invoke(new Action(Hide))),

					frames.BufferPrevious().WithLatestFromBuffered(density, Tuple.Create).Subscribe(
						(f, d) => Fusion.Application.MainThread.Schedule(() => 
						{
							WinApi.SetWindowPos(
								Handle,
								f.Current.IntersectionInParent.Position * d, // Bug in Window chrome, where position must be set once more, even though it has been set.
								f.ChangesTo(fr => fr.IntersectionInParent.Size * d),
								f.Current.IntersectionInParent.Size.HasZeroArea(),
								flags);

							WinApi.SetWindowPos(
								unoHostHwnd,
								f.ChangesTo(fr => fr.BoundsInIntersection.Position * d),
								f.ChangesTo(fr => fr.BoundsInIntersection.Size * d),
								f.Current.BoundsInIntersection.Size.HasZeroArea(),
								flags | WinApi.SWP_ASYNCWINDOWPOS);
						}))
			);
		}


	}

	class ClipResult
	{
		public Rectangle<Points> IntersectionInParent { get; set; }
		public Rectangle<Points> BoundsInIntersection { get; set; }
	}

	static class ClipToExtension
	{
		public static IObservable<Tuple<Rectangle<Points>, Rectangle<Points>>> ScreenRect(this FrameworkElement self, Dispatcher dispatcher)
		{
			return DataBinding
				.ObservableFromNativeEvent<EventArgs>(self, "LayoutUpdated")
				.Select(_ => Unit.Default)
				.StartWith(Unit.Default)
				.SubscribeOn(self.Dispatcher)
				.Select(_ => dispatcher.InvokeAsync(() => self.GetScreenRect2()))
				.Switch();
				
			//.ObserveOn(self.Dispatcher);
		}
		public static Tuple<Rectangle<Points>, Rectangle<Points>> GetScreenRect(this FrameworkElement self)
		{
			// TODO: this can throw NRE
			var hwndSource = PresentationSource.FromVisual(self);
			if (hwndSource == null)
				return Tuple.Create(Rectangle.FromPositionSize<Points>(0, 0, 0, 0), Rectangle.FromPositionSize<Points>(0, 0, 0, 0));

			var rootVisual = hwndSource.RootVisual;
			var selfToRoot = self.TransformToAncestor(rootVisual);

			var unclippedRect = new Rect(
				selfToRoot.Transform(new System.Windows.Point(0, 0)),
				selfToRoot.Transform(new System.Windows.Point(self.ActualWidth, self.ActualHeight)));

			var clippedRect = unclippedRect;

			var parent = VisualTreeHelper.GetParent(self);

			while (parent != null)
			{
				var visual = parent as Visual;
				var control = parent as Canvas;
				var scrollView = parent as ScrollViewer;

				var availableSize = Optional.None<System.Windows.Point>();
				if (control != null && control.ClipToBounds)
					availableSize = new System.Windows.Point(control.ActualWidth, control.ActualHeight);
				else if (scrollView != null && scrollView.ClipToBounds)
					availableSize = new System.Windows.Point(
						scrollView.ViewportWidth 
							- (scrollView.ComputedVerticalScrollBarVisibility == Visibility.Visible ? 15 : 0), 
						scrollView.ViewportHeight
							- (scrollView.ComputedHorizontalScrollBarVisibility == Visibility.Visible ? 15 : 0));

				if (visual != null && availableSize.HasValue)
				{
					var visualToRoot = visual.TransformToAncestor(rootVisual);

					var pointAncestorTopLeft = visualToRoot.Transform(new System.Windows.Point(0, 0));
					var pointAncestorBottomRight = visualToRoot.Transform(availableSize.Value);
					var ancestorRect = new Rect(pointAncestorTopLeft, pointAncestorBottomRight);

					clippedRect.Intersect(ancestorRect);
				}

				parent = VisualTreeHelper.GetParent(parent);
			}

			return Tuple.Create(
				Rectangle.FromSides<Points>(unclippedRect.Left, unclippedRect.Top, unclippedRect.Right, unclippedRect.Bottom),
				Rectangle.FromSides<Points>(clippedRect.Left, clippedRect.Top, clippedRect.Right, clippedRect.Bottom));

		}

		public static Tuple<Rectangle<Points>, Rectangle<Points>> GetScreenRect2(this FrameworkElement self)
		{
			// TODO: this can throw NRE
			var hwndSource = PresentationSource.FromVisual(self);
			if (hwndSource == null)
				return Tuple.Create(Rectangle.FromSides<Points>(0, 0, 0, 0), Rectangle.FromSides<Points>(0, 0, 0, 0));

			var rootVisual = hwndSource.RootVisual;
			var selfToRoot = self.TransformToAncestor(rootVisual);

			var unclippedRect = new Rect(
				selfToRoot.Transform(new System.Windows.Point(0, 0)),
				selfToRoot.Transform(new System.Windows.Point(self.ActualWidth, self.ActualHeight)));

			var clippedRect = new Rect(0, 0, double.PositiveInfinity, double.PositiveInfinity);

			var parent = VisualTreeHelper.GetParent(self);

			while (parent != null)
			{
				var visual = parent as Visual;
				var control = parent as Canvas;
				var scrollView = parent as ScrollViewer;

				var availableSize = Optional.None<System.Windows.Point>();
				if (control != null && control.ClipToBounds)
					availableSize = new System.Windows.Point(control.ActualWidth, control.ActualHeight);
				else if (scrollView != null && scrollView.ClipToBounds)
					availableSize = new System.Windows.Point(scrollView.ViewportWidth, scrollView.ViewportHeight);

				if (visual != null && availableSize.HasValue)
				{
					var visualToRoot = visual.TransformToAncestor(rootVisual);

					var pointAncestorTopLeft = visualToRoot.Transform(new System.Windows.Point(0, 0));
					var pointAncestorBottomRight = visualToRoot.Transform(availableSize.Value);
					var ancestorRect = new Rect(pointAncestorTopLeft, pointAncestorBottomRight);

					clippedRect.Intersect(ancestorRect);
				}

				parent = VisualTreeHelper.GetParent(parent);
			}

			return Tuple.Create(
				Rectangle.FromSides<Points>(unclippedRect.Left, unclippedRect.Top, unclippedRect.Right, unclippedRect.Bottom),
				Rectangle.FromSides<Points>(clippedRect.Left, clippedRect.Top, clippedRect.Right, clippedRect.Bottom));

		}
	}
}