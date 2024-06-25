using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Outracks.Fusion.Windows
{
	public static class OverlayImplementation
	{
		public static void Initialize(Dispatcher dispatcher)
		{
			Overlay.Initialize((background, foreground) =>
				Control.Create(self =>
				{
					var canvas = (FrameworkElement)foreground.NativeHandle;

					Fusion.Application.MainThread.Schedule(() =>
					{
						if (canvas.Parent != null)
							((Canvas) canvas.Parent).Children.Remove(canvas);
					});

					var foregroundWindow = new OverlayWindow
					{
						Content = new Canvas
						{
							Children = { canvas }
						},
					};

					var backgroundElement = background.NativeHandle as FrameworkElement;
					if (backgroundElement != null)
					{
						background.Mount(self);
					}
					else
					{
						var dummy = Shapes.Rectangle();
						dummy.Mount(self);
						backgroundElement = (FrameworkElement)dummy.NativeHandle;
					}

					Fusion.Application.MainThread.Schedule(() =>
					{
						foregroundWindow.KeyDown += (sender, args) =>
						{
							var visual = PresentationSource.FromVisual(backgroundElement);
							if (visual == null)
								return;

							if (args.Key == System.Windows.Input.Key.DeadCharProcessed)
								return;

							backgroundElement.RaiseEvent(new KeyEventArgs(System.Windows.Input.Keyboard.PrimaryDevice, visual, 0, args.Key)
							{
								RoutedEvent = System.Windows.Input.Keyboard.PreviewKeyDownEvent,
								Source = backgroundElement,
							});
						};

						foregroundWindow.KeyUp += (sender, args) =>
						{
							var visual = PresentationSource.FromVisual(backgroundElement);
							if (visual == null)
								return;

							if (args.Key == System.Windows.Input.Key.DeadCharProcessed)
								return;

							backgroundElement.RaiseEvent(new KeyEventArgs(System.Windows.Input.Keyboard.PrimaryDevice, visual, 0, args.Key)
							{
								RoutedEvent = System.Windows.Input.Keyboard.PreviewKeyUpEvent,
								Source = backgroundElement,
							});
						};
					});

					var windowFrames = backgroundElement.ScreenRect(dispatcher);
					var intersection = windowFrames.Select(f => f.Item2);
					var frame = windowFrames.Select(f => f.Item1);
					var bounds = frame.RelativeTo(intersection.Position()).Replay(1).RefCount();

					foreground.Mount(
						new MountLocation.Mutable
						{
							IsRooted = self.IsRooted,
							AvailableSize = self.AvailableSize,
							NativeFrame = bounds.Transpose(),
						});

					var parentWindow =
						self.IsRooted.Switch(isRooted =>
							isRooted == false
								? Observable.Return(Optional.None<WindowWithOverlays>())
								: backgroundElement.GetWindow<WindowWithOverlays>());

					parentWindow.SubscribeUsing(tmp =>
							tmp.MatchWith(
								none: () => Disposable.Empty,
								some: backgroundWindow =>
								{
									var windowLocation = DataBinding.ObservableFromNativeEvent<object>(backgroundWindow, "LocationChanged")
										.StartWith(new object())
										.Select(_ => dispatcher.InvokeAsync(() => new Point<Points>(backgroundWindow.Left, backgroundWindow.Top)))
										.Switch();

									dispatcher.Schedule(() =>
									{
										foregroundWindow.ShowActivated = false;
										foregroundWindow.Show();
										foregroundWindow.Owner = backgroundWindow;
									});

									return Disposable.Combine(
										backgroundWindow.AddOverlay("name", foregroundWindow),

										Disposable.Create(() => dispatcher.Schedule(foregroundWindow.Hide)),

										intersection.MoveTo(windowLocation)/*.Sample(Fusion.Application.PerFrame)*/
											.Subscribe(s => dispatcher.Schedule(() =>
											{
												if (!double.IsInfinity(s.Left()))
													foregroundWindow.Left = s.Left();
												if (!double.IsInfinity(s.Top()))
													foregroundWindow.Top = s.Top();
												if (!double.IsInfinity(s.Width))
													foregroundWindow.Width = s.Width.Max(0);
												if (!double.IsInfinity(s.Height))
													foregroundWindow.Height = s.Height.Max(0);
											})));
								}));

					return backgroundElement;
				}));
		}
	}
}