using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace Outracks.Fusion.Windows
{
	public static class WindowImplementation
	{
		const int TitleBarHeight = 28;

		public static System.Windows.Window Initialize(Window model)
		{
			var dispatcher = Fusion.Application.MainThread;

			var windowSubject = new BehaviorSubject<Optional<WindowWithOverlays>>(Optional.None());

			var contentContainer = new Canvas()
			{
				// This is needed for native overlays to work
				ClipToBounds = true,
			};

			var ret = new FancyWindow(model.Style == WindowStyle.Regular || model.Style == WindowStyle.Fat)
			{
				Content = contentContainer
			};

			var menu = ret.Menu;

			var focused = new Subject<Unit>();
			model.Focused.Execute.Sample(focused).Subscribe(c => c());
			ret.Activated += (sender, args) => focused.OnNext(Unit.Default);

			var availableSize = DataBinding
				.ObservableFromNativeEvent<SizeChangedEventArgs>(contentContainer, "SizeChanged")
				.Select(e => e.NewSize.ToFusion())
				.Replay(1);

			availableSize.Connect();

			var content = model.Content;
				//.SetNativeWindow(windowSubject)
				//.SetDensity(ret.DensityFactor);
			var transize = availableSize.Transpose();
			content.Mount(
				new MountLocation.Mutable
				{
					AvailableSize = transize,
					NativeFrame = ObservableMath.RectangleWithSize(transize),
					IsRooted = Observable
						.FromEventPattern<DependencyPropertyChangedEventArgs>(ret, "IsVisibleChanged")
						.Select(arg => ret.IsVisible)
						.Replay(1).RefCount(),
				});

			content.BindNativeProperty(dispatcher, "Background", model.Background, color =>
				menu.Background = ret.Background = new SolidColorBrush(color.ToColor()));

			content.BindNativeProperty(dispatcher, "Foreground", model.Foreground, color =>
				menu.Foreground = ret.Foreground = new SolidColorBrush(color.ToColor()));

			content.BindNativeProperty(dispatcher, "BorderBrush", model.Border.Brush, color =>
				ret.BorderBrush = new SolidColorBrush(color.ToColor()));

			content.BindNativeProperty(dispatcher, "BorderThickness", model.Border.Thickness, thickness =>
				ret.BorderThickness = new System.Windows.Thickness(thickness));


			Fusion.Application.MainThread
				.InvokeAsync(() => content.NativeHandle)
				.ToObservable()				.OfType<UIElement>()
				.Subscribe(nativeContent =>
					contentContainer.Children.Add(nativeContent));

			var windowState =
				model.State
					.Or(Property.Default<Optional<WindowState>>())
					.Or(WindowState.Normal)
					.AutoInvalidate()
					.PreventFeedback();

			windowState.Subscribe(state => dispatcher.InvokeAsync(() =>
				ret.WindowState = state == WindowState.Normal ? System.Windows.WindowState.Normal : System.Windows.WindowState.Maximized));

			ret.StateChanged += (sender, args) =>
				windowState.Write(
					ret.WindowState == System.Windows.WindowState.Maximized
						? WindowState.Maximized
						: WindowState.Normal);

			model.Size.Do(
				maybeSize =>
				{
					var size = maybeSize
						.Or(Size.Create<Points>(Double.NaN, Double.NaN))
						.AutoInvalidate(TimeSpan.FromSeconds(2))
						.PreventFeedback();

					bool setByUs = false;

					size.Subscribe(s => dispatcher.Schedule(() =>
					{
						setByUs = true;
						ret.Width = (float)s.Width;
						var newHeight = s.Height + (TitleBarHeight);
						ret.Height = (float) newHeight;
						setByUs = false;
					}));

					ret.SizeChanged += (s, a) =>
					{
						if (setByUs) return;

						var contentSize = new Size<Points>(a.NewSize.Width, a.NewSize.Height - (TitleBarHeight));
						size.Write(contentSize);
					};
				});

			model.Position.Do(
				maybePosition =>
				{
					var pos = maybePosition
						.Or(new Point<Points>(double.NaN, double.NaN))
						.AutoInvalidate(TimeSpan.FromSeconds(2))
						.PreventFeedback();
						// TODO: ConnectWhile() ?

					bool setByUs = false;

					pos.Subscribe(p => dispatcher.Schedule(() =>
					{
						setByUs = true;
						ret.Left = p.X;
						ret.Top = p.Y;
						setByUs = false;
					}));

					Fusion.Application.MainThread.Schedule(() =>
					{
						ret.LocationChanged += (s, args) =>
						{
							if (setByUs) return;

							var sender = s as FancyWindow;
							if (sender == null) return;

							pos.Write(new Point<Points>(sender.Left, sender.Top));
						};
					});
				});

			model.TopMost.Do(topMost =>
				topMost.Subscribe(t => dispatcher.Schedule(() =>
				{
					ret.Topmost = t;
					ret.Show();
				})));

			model.Size.Do(s =>
				s.IsReadOnly.Subscribe(isReadOnly => dispatcher.Schedule(() =>
				{
					if (isReadOnly)
					{
						ret.ResizeMode = ResizeMode.NoResize;
						ret.WindowStartupLocation = WindowStartupLocation.CenterScreen;
					}
				})));


			var closed = new Subject<Unit>();
			ret.Closed += (s, a) => closed.OnNext(Unit.Default);
			model.Closed.Execute.Sample(closed).Subscribe(e => e());

			model.Title.Subscribe(title => dispatcher.Schedule(() =>
			{
				ret.Title = model.HideTitle ? "" : title;
			}));

			var hidden = false;
			model.Menu.Do(m => WindowsMenuBuilder.Populate(m, menu, ret, dispatcher));

			ret.KeyUp += (s, e) => {
				if (hidden && e.Key == System.Windows.Input.Key.System)
					menu.Visibility = menu.Visibility == Visibility.Collapsed
						? Visibility.Visible
						: Visibility.Collapsed;
			};

			model.HideMenu.Do(hideMenu =>
				hideMenu.Subscribe(value => dispatcher.Schedule(() => {
					hidden = value;
					menu.Visibility = value
						? Visibility.Collapsed
						: Visibility.Visible;
				})));

			DataBinding
				.ObservableFromNativeEvent<EventArgs>(ret, "Loaded")
				.Subscribe(_ => windowSubject.OnNext(ret));

			DataBinding
				.ObservableFromNativeEvent<EventArgs>(ret, "LayoutUpdated")
				.Select(a => PresentationSource.FromVisual(ret))
				.DistinctUntilChanged()
				.Subscribe(
					presentation =>
					{
						if (presentation == null)
							return;

						var winHandle = new WindowInteropHelper(ret).Handle;
						WindowPlacement.SetPlacement(winHandle, WindowPlacement.GetPlacement(winHandle));
					});

			ret.Closing += (s, a) =>
			{
				model.Closed.Execute.Take(1).Subscribe(e => e());
				ExitIfLastVisibleWindow(ret);
			};

			ret.IsVisibleChanged += (s, a) =>
			{
				if (ret.IsVisible == false)
					ExitIfLastVisibleWindow(ret);
			};

			return ret;
		}

		static void ExitIfLastVisibleWindow(System.Windows.Window ret)
		{
			foreach (var windowObject in System.Windows.Application.Current.Windows)
			{
				var window = windowObject as System.Windows.Window;
				if (window == null ||
					    window.GetType().Namespace.StartsWith("Microsoft.") ||
					    window.Title == "Console output")
					continue;

				if (window != ret && window is OverlayWindow == false && window.IsVisible)
					return;
			}

			Fusion.Application.Exit(0);
		}

		static void Print(object content, int indentation)
		{
			Console.WriteLine(new string(' ', indentation) + content);

			var fe = content as Panel;
			if (fe == null)
				return;

			foreach (var child in fe.Children)
				Print(child, indentation + 1);
		}
	}
}
