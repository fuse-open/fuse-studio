using System.Windows.Media;
using System.Windows.Shapes;

namespace Outracks.Fusion.Windows
{
	static class ShapeControl
	{
		public static void BindShapeProperties(this IMountLocation self, Shape control, Dispatcher dispatcher, Brush fill, Stroke stroke)
		{
			self.BindNativeProperty(dispatcher, "fill.Color", fill, f => control.Fill = new SolidColorBrush(f.ToColor()));

			self.BindNativeProperty(dispatcher, "stroke.Brush.Color", stroke.Brush, s => control.Stroke = new SolidColorBrush(s.ToColor()));
			self.BindNativeProperty(dispatcher, "stroke.Thickness", stroke.Thickness, s => control.StrokeThickness = s);
			self.BindNativeProperty(dispatcher, "stroke.DashArray", stroke.DashArray, s => control.StrokeDashArray = s.ToDashArray());
		}
	}
}