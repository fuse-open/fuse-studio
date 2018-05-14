using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Outracks.Fusion.OSX
{
	class RectangleImplementation
	{
		public static void Initialize(IScheduler dispatcher)
		{
			Shapes.Implementation.RectangleFactory = (strokeF, fillF, cornerRadius) =>
				Control.Create(location =>
				{
					var control = new NSRectangle();

					if (cornerRadius.HasValue)
					{
						location.BindNativeProperty(
							dispatcher, "radius",
							cornerRadius.Value,
							v => control.CornerRadius = v);
					}

					location.BindShapeProperties(control, dispatcher, fillF, strokeF);
					location.BindNativeDefaults(control, dispatcher);

					return control;
				});
		}
	}
}