using System.Reactive.Concurrency;

namespace Outracks.Fusion.OSX
{
	static class ShapeControl
	{
		public static void BindShapeProperties(this IMountLocation self, NSShape control, IScheduler dispatcher, Brush fill, Stroke stroke)
		{
			self.BindNativeProperty(dispatcher, "fill.Color", fill, f => control.FillColor = f.ToNSColor());

			self.BindNativeProperty(dispatcher, "stroke.Brush.Color", stroke.Brush, s => control.StrokeColor = s.ToNSColor());
			self.BindNativeProperty(dispatcher, "stroke.Thickness", stroke.Thickness, s => control.LineThickness = (float)s);
			self.BindNativeProperty(dispatcher, "stroke.DashArray", stroke.DashArray, s => control.LineDash = s.ToLineDash());
		}
	}
}
