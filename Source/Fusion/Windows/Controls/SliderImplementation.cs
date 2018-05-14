using System;
using System.Reactive.Linq;

namespace Outracks.Fusion.Windows
{
	static class SliderImplementation
	{
		public static void Initialize(Dispatcher dispatcher)
		{
			Slider.Implementation.Factory = (value, min, max) =>
			{
				value = value
					.PreventFeedback()
					.AutoInvalidate(TimeSpan.FromMilliseconds(200));

				return Control
					.Create(self =>
					{
						var control = new System.Windows.Controls.Slider
						{
							Padding = new System.Windows.Thickness(1)
						};

						self.BindNativeDefaults(control, dispatcher);

						self.BindNativeProperty(dispatcher, "Disabled", value.IsReadOnly, v => control.IsEnabled = !v);

						self.BindNativeProperty(dispatcher, "Minimum", min, v => control.Minimum = v);
						self.BindNativeProperty(dispatcher, "Maximum", max, v => control.Maximum = v);

						self.BindNativeProperty(control, d => d.Value, value);

						return control;
					})
					.WithHeight(30);
			};
		}
	}
}