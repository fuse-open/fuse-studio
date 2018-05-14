using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Outracks.Fusion.Windows
{
	static class ClippingImplementation
	{
		

		public static void Initialize(Dispatcher dispatcher)
		{
			Layout.Implementation.LayerControls = (childFactory) =>
				Control.Create(
					ctrl =>
					{
						var element = new Canvas();

						ctrl.BindNativeDefaults(element, dispatcher);

						childFactory(ctrl)
							// IMPORTANT: ConnectWhile has to be done first since else we'll lose all changes done to the children list while the ctrl is unrooted.
							// which breaks diffing, and we get dangling views (views that aren't removed).
							.ConnectWhile(ctrl.IsRooted)							
							.DiffSequence()
							.ObserveOn(Fusion.Application.MainThread)
							.Subscribe(children =>
							{
								foreach (var child in children.Removed)
								{
									var nativeChild = child.NativeHandle as FrameworkElement;
									if (nativeChild != null) 
											element.Children.Remove(nativeChild);

									child.Mount(MountLocation.Unmounted);
								}

								foreach (var child in children.Added)
								{	
									child.Mount(new MountLocation.Mutable
									{
										IsRooted = ctrl.IsRooted,
										AvailableSize = ctrl.AvailableSize,
										NativeFrame = ObservableMath.RectangleWithSize(ctrl.NativeFrame.Size),
									});

									var nativeChild = child.NativeHandle as FrameworkElement;
									if (nativeChild == null)
										continue;

									var parent = nativeChild.Parent as Canvas;
									if (parent != null)
										parent.Children.Remove(nativeChild);

									element.Children.Add(nativeChild);
								}
							});

						return element;
					});
			
			Clipping.Initialize(
				(content, clipToBounds) =>
				{
					var container = Layout.Layer(content);

					Fusion.Application.MainThread.Schedule(() =>
					{
						var elm = (FrameworkElement)container.NativeHandle;
						elm.ClipToBounds = clipToBounds;
					});

					return container.WithSize(content.DesiredSize);
				});
		}
	}
}