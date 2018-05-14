using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Outracks.Fusion.OSX
{
	class CircleImplementation
	{
		public static void Initialize(IScheduler dispatcher)
		{
			Shapes.Implementation.CircleFactory = (strokeF, fillF) =>
				Control.Create(mountLocation =>
				{
					var view = new NSCircle();

					mountLocation.BindShapeProperties(view, dispatcher, fillF, strokeF);
					mountLocation.BindNativeDefaults(view, dispatcher);

					return view;
				});
		}
	}
}
