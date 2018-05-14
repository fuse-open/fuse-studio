using System.Reactive.Linq;
using System.Windows.Controls;

namespace Outracks.Fusion.Windows
{
	static class DropDownImplementation
	{
		public static void Initialize(Dispatcher dispatcher)
		{
			DropDown.Implementation.Factory = (value, items, nativeLook) =>
				Control
					.Create(self =>
						{
							value = value
								.ConnectWhile(self.IsRooted)
								.PreventFeedback()
								.AutoInvalidate();

							var control = new ComboBox
							{
								Padding = new System.Windows.Thickness(1),
							};
									
							if (!nativeLook)
							{
								control.BorderBrush = null;
								control.Background = null;
							}

							// Disable control if list is empty
							var isDisabled = value.IsReadOnly.Or(items.Select(x => x.IsEmpty()));

							self.BindNativeProperty(dispatcher, "Disabled", isDisabled, v =>
							{
								control.IsReadOnly = v;
								control.IsEnabled = !v;
							});

						
							self.BindNativeDefaults(control, dispatcher);

							self.BindNativeProperty(dispatcher, "items", items, i => control.ItemsSource = i);
							self.BindNativeProperty(control, c => c.SelectedItem, value, eventName: "SelectionChanged");
							return control;
						})
					.WithHeight(30);


		}
	}
}