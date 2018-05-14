using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using AppKit;
using Foundation;
using System.Reactive.Subjects;

namespace Outracks.Fusion.OSX
{
	static class ColorPickerImplementation
	{
		public static void Initialize(IScheduler dispatcher)
		{
			var defaultColor = Optional.None<IProperty<Color>>();

			var panelVisible = Property.Create(false);

			var currentColor = new BehaviorSubject<Optional<IProperty<Color>>>(defaultColor);

			var editingColor =
				currentColor
					.ReplaceNone(() => Property.AsProperty(Observable.Never<Color>()))
					.Switch()
					.PreventFeedback()
					.AutoInvalidate(TimeSpan.FromMilliseconds(200));

			Fusion.Application.MainThread.Schedule(() =>
			{
				var colorPanel = NSColorPanel.SharedColorPanel;
				colorPanel.ShowsAlpha = true;
				colorPanel.WillClose += (s, a) =>
				{
					currentColor.OnNext(defaultColor);
					panelVisible.Write(false);
				};

				var colorUpdated = new Subject<Color>();
				NSNotificationCenter.DefaultCenter.AddObserver(
					new NSString("NSColorPanelColorDidChangeNotification"),
					notification => { colorUpdated.OnNext(colorPanel.Color.ToColor()); },
					colorPanel);

				var setByUs = false;
				editingColor.ObserveOn(dispatcher).Subscribe(c =>
				{
					setByUs = true;
					try
					{
						colorPanel.Color = c.ToNSColor();
					}
					finally
					{
						setByUs = false;
					}
				});

				colorUpdated.Subscribe(color =>
				{
					if (!setByUs)
						editingColor.Write(color);
				});
			
				panelVisible.ObserveOn(dispatcher).Subscribe(visible => colorPanel.IsVisible = visible);
			});

			ColorPicker.Implementation.OpenCommand = (color) => 
				Command.Enabled(() =>
				{
					if (currentColor.Value == Optional.Some(color))
					{
						currentColor.OnNext(defaultColor);
						panelVisible.Write(false);
					}
					else
					{
						currentColor.OnNext(Optional.Some(color));
						panelVisible.Write(true);
					}
				});
		}
	}
}
