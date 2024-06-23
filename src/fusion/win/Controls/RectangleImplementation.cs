namespace Outracks.Fusion.Windows
{
	static class RectangleImplementation
	{
		public static void Initialize(Dispatcher dispatcher)
		{
			Shapes.Implementation.RectangleFactory = (strokeF, fillF, cornerRadius) =>
				Control.Create(self =>
				{
					var control = new System.Windows.Shapes.Rectangle
					{
						IsHitTestVisible = false,
					};

					if (cornerRadius.HasValue)
						self.BindNativeProperty(dispatcher, "radius", cornerRadius.Value, v =>
						{
							control.RadiusX = v.RadiusX;
							control.RadiusY = v.RadiusY;
						});

					self.BindShapeProperties(control, dispatcher, fillF, strokeF);
					self.BindNativeDefaults(control, dispatcher);

					return control;
				});
		}
	}
}