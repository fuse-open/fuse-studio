using System.Windows.Media;
using System.Windows.Shapes;

namespace Outracks.Fusion.Windows
{
	static class LineImplementation
	{
		public static void Initialize(Dispatcher dispatcher)
		{
			Shapes.Implementation.LineFactory = (from, to, stroke) =>
				Control.Create(self =>
				{
					var control = new Line()
					{
						IsHitTestVisible = false,
					};

					self.BindNativeDefaults(control, dispatcher);

					self.BindNativeProperty(dispatcher, "x1", from.X, (v) => control.X1 = v);
					self.BindNativeProperty(dispatcher, "y1", from.Y, (v) => control.Y1 = v);

					self.BindNativeProperty(dispatcher, "x2", to.X, v => control.X2 = v);
					self.BindNativeProperty(dispatcher, "y2", to.Y, v => control.Y2 = v);

					self.BindNativeProperty(
						dispatcher, "stroke.thickness", stroke.Thickness,
						thickness => control.StrokeThickness = thickness);

					self.BindNativeProperty(
						dispatcher, "stroke.brush", stroke.Brush,
						brush => control.Stroke = new SolidColorBrush(brush.ToColor()));

					self.BindNativeProperty(
						dispatcher, "stroke.dasharray", stroke.DashArray,
						dashArray => control.StrokeDashArray = dashArray.ToDashArray()); ;

					return control;
				});


		}
	}
}