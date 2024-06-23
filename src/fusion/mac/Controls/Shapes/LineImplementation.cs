using System.Reactive.Concurrency;
using System.Reactive.Linq;
using CoreGraphics;

namespace Outracks.Fusion.Mac
{
	static class LineImplementation
	{
		public static void Initialize(IScheduler dispatcher)
		{
			Shapes.Implementation.LineFactory = (from, to, stroke) =>
				Control.Create(self =>
				{
					var control = new NSLine();

					self.BindNativeDefaults(control, dispatcher);

					self.BindNativeProperty(dispatcher, "x1", from.X, v => control.Start = new CGPoint(v, control.Start.Y));
					self.BindNativeProperty(dispatcher, "y1", from.Y.CombineLatest(self.NativeFrame.Height, (y, height) => height - y), v => control.Start = new CGPoint(control.Start.X, v));

					self.BindNativeProperty(dispatcher, "x2", to.X, v => control.End = new CGPoint(v, control.End.Y));
					self.BindNativeProperty(dispatcher, "y2", to.Y.CombineLatest(self.NativeFrame.Height, (y, height) => height - y), v => control.End = new CGPoint(control.End.X, v));

					self.BindNativeProperty(dispatcher, "stroke.thickness", stroke.Thickness, thickness => control.LineThickness = (float)thickness);
					self.BindNativeProperty(dispatcher, "stroke.brush", stroke.Brush, color => control.StrokeColor = color.ToNSColor());
					self.BindNativeProperty(dispatcher, "stroke.dasharray", stroke.DashArray, array => control.LineDash = array.ToLineDash());

					return control;
				});
		}
	}


}
