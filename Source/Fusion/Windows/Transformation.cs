using System;
using System.Reactive.Concurrency;
using System.Windows;
using System.Windows.Controls;

namespace Outracks.Fusion.Windows
{
	class Transformation
	{
		static Dispatcher _dispatcher;
		public static void Initialize(Dispatcher dispatcher)
		{
			_dispatcher = dispatcher;
			Fusion.Transformation.Initialize(Transform);
		}

		static IControl Transform(IControl ctrl, IObservable<Matrix> matrix)
		{
			return Control.Create(
				self =>
				{
					ctrl.Mount(
						new MountLocation.Mutable
						{
							AvailableSize = self.AvailableSize,
							IsRooted = self.IsRooted,
							NativeFrame = ObservableMath.RectangleWithSize(ctrl.DesiredSize),
						});

					var nativeThing = ctrl.NativeHandle as FrameworkElement;
					if (nativeThing == null)
						return null;

					var wrapper = new Canvas
					{
						Children =
						{
							nativeThing
						}
					};

					self.BindNativeDefaults(wrapper, _dispatcher);
					self.BindNativeProperty(
						Fusion.Application.MainThread,
						"transform",
						matrix,
						mat =>
						{
							//element.RenderTransformOrigin = new System.Windows.Point(element.Width / 2, element.Height / 2);
							nativeThing.LayoutTransform = mat.ToWpfScaleRotation();
						});



					return wrapper;
				}).WithSize(
						Rectangle
							.FromPositionSize(
								Point.Create(ctrl.DesiredSize.Width.Div(-2), ctrl.DesiredSize.Height.Div(-2)),
								ctrl.DesiredSize)
							.Transpose()
							.Transform(matrix)
							.Transpose()
							.Size);
		}
	}
}