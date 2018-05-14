using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Outracks.Fusion.Windows
{
	static class ScrollingImplementation
	{
		public static void Initialize(Dispatcher dispatcher)
		{
			Scrolling.Implementation.Factory = (content, darkTheme, supportsOpenGL, zooomAttribs, onBoundsChanged, scrollToRectangle, verticalScrollBarVisible, horizontalScrollBarVisible) =>
				Control.Create(self =>
				{
					var contentNativeFrame = ObservableMath.RectangleWithSize(content.DesiredSize.Max(self.NativeFrame.Size));
							
					content.Mount(
						new MountLocation.Mutable
						{
							IsRooted = self.IsRooted,
							NativeFrame = contentNativeFrame,
							AvailableSize = self.NativeFrame.Size,
						});

					var dummyControl = new Canvas();
					var view = new ScrollViewer()
					{
						ClipToBounds = true,
						Content = new ContentControl()
						{
							VerticalAlignment = VerticalAlignment.Top,
							HorizontalAlignment = HorizontalAlignment.Left,
							Content = dummyControl,
						},

						HorizontalScrollBarVisibility = horizontalScrollBarVisible ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden,
						VerticalScrollBarVisibility = verticalScrollBarVisible ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden,
					};
	
					self.BindNativeProperty(dispatcher, "containerWidth", contentNativeFrame.Size.Width, width => dummyControl.Width = Math.Max(0.0, width));
					self.BindNativeProperty(dispatcher, "containerHeight", contentNativeFrame.Size.Height, height => dummyControl.Height = Math.Max(0.0, height));

					self.BindNativeProperty(dispatcher, "scrollTarget", scrollToRectangle,
						r =>
						{
							dummyControl.BringIntoView(new Rect(r.Left(), r.Top(), r.Width, r.Height));
						});

					onBoundsChanged.Do(handler =>
					{
						DataBinding.ObservableFromNativeEvent<EventArgs>(view, "ScrollChanged")
							.CombineLatest(
								contentNativeFrame.Transpose().DistinctUntilChanged(),
								(_, contentFrame) => new ScrollBounds(
									Rectangle.FromPositionSize<Points>(
										view.HorizontalOffset,
										view.VerticalOffset,
										view.ViewportWidth,
										view.ViewportHeight),
									contentFrame))
							.DistinctUntilChanged()
							.Subscribe(handler);
					});

					self.BindNativeDefaults(view, dispatcher);

					var child = (FrameworkElement) content.NativeHandle;
					
					dummyControl.Children.Add(child);

					return view;
				});
		}
	}
}