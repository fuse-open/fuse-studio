using System.Windows.Shapes;

namespace Outracks.Fusion.Windows
{
	static class CircleImplementation
	{
		public static void Initialize(Dispatcher dispatcher)
		{
			Shapes.Implementation.CircleFactory = (strokeF, fillF) =>
				 Control
					.Create(self =>
					{
						var control = new Ellipse
						{
							IsHitTestVisible = false
						};

						self.BindShapeProperties(control, dispatcher, fillF, strokeF);
						self.BindNativeDefaults(control, dispatcher);

						return control;
					});

		}
	}
}