using System;
using System.Drawing;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media.Effects;

namespace Outracks.Fusion.Windows
{
	static class EffectsImplementation
	{
		public static void Initialize(Dispatcher dispatcher)
		{
			Effects.Implementation.DropShadow = (content, color, radius, angle, distance) =>
			{
				var offset = angle
					.Select(d => Math.PI * d / 180.0)
					.CombineLatest(distance, (r, d) =>
						new SizeF((float)(Math.Cos(r) * d), (float)(Math.Sin(r) * d)));

				var extrudeSize = offset
					.CombineLatest(radius, (off, rad) =>
						new Size<Points>(Math.Abs(off.Width) + rad, Math.Abs(off.Height) + rad))
					.Select(s => new Thickness<Points>(s.Width, s.Height));

				content = content.WithPadding(extrudeSize);
				var self = Layout.Layer(content).WithSize(content.DesiredSize);

				dispatcher.Enqueue(() =>
				{
					var effect = new DropShadowEffect();
					var control = self.NativeHandle as FrameworkElement;
					if (control == null)
						return;

					self.BindNativeProperty(dispatcher, "color", color, c => effect.Color = c.ToColor());
					self.BindNativeProperty(dispatcher, "color.a", color, c => effect.Opacity = c.A);
					self.BindNativeProperty(dispatcher, "radius", radius, c => effect.BlurRadius = c);
					self.BindNativeProperty(dispatcher, "angle", angle, c => effect.Direction = c);
					self.BindNativeProperty(dispatcher, "distance", distance, c => effect.ShadowDepth = c);

					control.Effect = effect;
				});

				return self;
			};
		}
	}
}