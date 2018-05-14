using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using AppKit;
using Foundation;

namespace Outracks.Fusion.OSX
{
	static class SliderImplementation
	{		
		public static void Initialize(IScheduler dispatcher)
		{
			Slider.Implementation.Factory = (value, min, max) =>
			{
				value = value
					.PreventFeedback()
					.AutoInvalidate(TimeSpan.FromMilliseconds(200));

				return Control
					.Create(self =>
					{
						var s = new NSSlider();

						self.BindNativeDefaults(s, dispatcher);

						self.BindNativeProperty(dispatcher, "Minimum", min, (v) => s.MinValue = v);
						self.BindNativeProperty(dispatcher, "Maximum", max, (v) => s.MaxValue = v);
						self.BindNativeProperty(dispatcher, "Enabled", value.IsReadOnly, (e) => s.Enabled = !e);

						self.BindNativeProperty(s, slider => slider.DoubleValue, s.WhenChanged(), value);
						
						return s;
					});
			};
		}
	}
}
