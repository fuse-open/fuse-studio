using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using AppKit;
using CoreGraphics;

namespace Outracks.Fusion.Mac
{
	static class EffectsImplementation
	{
		public static void Initialize(IScheduler dispatcher)
		{
			Effects.Implementation.DropShadow = (content, color, radius, angle, distance) =>
			{
				radius = radius.Select(r => new Points(r * 1.0/3.0));
				var offset = angle
					.Select(d => Math.PI * d / 180.0)
					.CombineLatest(distance, (r, d) =>
						new CGSize(Math.Cos(r) * d, Math.Sin(r) * d));

				var extrudeSize = offset
					.CombineLatest(radius, (off, rad) =>
						new Size<Points>(Math.Abs(off.Width) + rad*2.0, Math.Abs(off.Height) + rad*2.0))
					.Select(s => new Thickness<Points>(s.Width.Round(), s.Height.Round()));

				content = content.WithPadding(extrudeSize);
				var self = Layout.Layer(content).WithSize(content.DesiredSize);

				Fusion.Application.MainThread.Schedule(() =>
				{
					var contentView = self.NativeHandle as NSView;
					if (contentView == null)
						return;

					contentView.WantsLayer = true;

					var effect = new NSShadow();

					self.BindNativeProperty(dispatcher, "color", color, c =>
					{
						effect.ShadowColor = c.ToNSColor().ShadowWithLevel(0.5f);
						contentView.Shadow = effect;
					});

					self.BindNativeProperty(dispatcher, "radius", radius, c =>
					{
						effect.ShadowBlurRadius = (float)c;
						contentView.Shadow = effect;
					});

					self.BindNativeProperty(dispatcher, "offset", offset, c =>
					{
						effect.ShadowOffset = c;
						contentView.Shadow = effect;
					});
				});

				return self;
			};
		}
	}
}