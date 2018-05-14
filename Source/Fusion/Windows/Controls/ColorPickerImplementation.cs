using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ColorPickerControls.Pickers;

namespace Outracks.Fusion.Windows
{
	static class ColorPickerImplementation
	{
		public static void Initialize(Dispatcher dispatcher)
		{
			var editingColorSubject = new BehaviorSubject<Optional<IProperty<Color>>>(Optional.None<IProperty<Color>>());

			var editingColor = editingColorSubject
				.ReplaceNone(() => Property.AsProperty(Observable.Never<Color>()))
				.Switch()
				.PreventFeedback()
				.AutoInvalidate(TimeSpan.FromMilliseconds(200));

			Fusion.Application.Desktop.CreateSingletonWindow(
				isVisible: editingColorSubject.Select(e => e.HasValue),
				window: window => new Window
				{
					Title = Observable.Return("Color"),
					Closed = editingColorSubject.Update(Optional.None<IProperty<Color>>()),
					Size = Optional.Some(Property.Constant(Optional.Some(new Size<Points>(560, 300)))),
					Background = Color.FromRgb(0xFCFCFC),
					Foreground =  Color.FromRgb(0x676767),
					Border = Stroke.Create(1, Color.FromRgb(0xe0e0e0)),
					Content = Control
						.Create(self => 
						{
							var dialog = new ColorPickerFullWithAlpha();

							dialog.UserChangedColor
								.Select(_ => dialog.SelectedColor.ToFusion())
								.Subscribe(editingColor);

							self.BindNativeProperty(dispatcher, "color", editingColor, c => dialog.SelectedColor = c.ToColor());
							self.BindNativeDefaults(dialog, dispatcher);

							return dialog;
						})
						.WithPadding(new Thickness<Points>(10))
						.WithBackground(Color.FromRgb(0xFCFCFC)),
				});

			ColorPicker.Implementation.OpenCommand = (color) =>
				editingColorSubject.Update(
					old =>
						old == Optional.Some(color)
							? Optional.None<IProperty<Color>>()
							: Optional.Some(color));

		}
	}
}