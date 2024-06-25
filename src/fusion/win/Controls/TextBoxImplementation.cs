using System;
using System.Reactive.Concurrency;
using System.Windows;
using System.Windows.Media;

namespace Outracks.Fusion.Windows
{
	static class TextBoxImplementation
	{
		public static void Initialize(Dispatcher dispatcher)
		{
			TextBox.Implementation.Factory = (value, isMultiline, doWrap, maybeOnFocused, foregroundColor) =>
			{
				return Control.Create(self =>
					{
						// In the simple test this didn't do anything, possibly because we're not subscribing until we try to set something.. should try it in bindnativeproperty afterwards, first i'm going to see how everything runs atm
						value = value.PreventFeedback();

						var control = new System.Windows.Controls.TextBox
						{
							Background = Brushes.Transparent,
							Padding = new System.Windows.Thickness(3),
							TextWrapping = doWrap ? TextWrapping.Wrap : TextWrapping.NoWrap,
							VerticalContentAlignment = doWrap ? VerticalAlignment.Top : VerticalAlignment.Center,
						};

						if (isMultiline == false)
							control.MaxLines = 1;

						control.BindText(value, self.IsRooted, isMultiline);

						maybeOnFocused.Do(onFocused =>
						{
							Action action = () => { };

							onFocused.Execute
								.ConnectWhile(self.IsRooted)
								.Subscribe(e => action = e);

							Fusion.Application.MainThread.Schedule(() =>
								control.GotKeyboardFocus += (s, a) => action());
						});

						self.BindNativeProperty(dispatcher, "foreground", foregroundColor,
							foreground =>
							{
								control.Foreground = control.CaretBrush = new SolidColorBrush(foreground.ToColor());
							});

						self.BindNativeDefaults(control, dispatcher);

						self.BindNativeProperty(dispatcher, "Disabled", value.IsReadOnly, v =>
						{
							control.IsReadOnly = v;
							control.IsEnabled = !v;
						});

						return control;
					})
					.WithHeight(isMultiline ? 130 : 20).CenterVertically();
			};
		}

		static void BindText(
			this System.Windows.Controls.TextBox control,
			IProperty<string> value,
			IObservable<bool> isRooted,
			bool isMultiline)
		{
			bool valueSetByUser = true;
			bool hasUnsavedChanges = false;

			value = value
				.ConnectWhile(isRooted)
				.DistinctUntilChangedOrSet();

			DataBinding
				.ObservableFromNativeEvent<EventArgs>(control, "TextChanged")
				.Subscribe(_ =>
				{
					if (!valueSetByUser)
						return;

					hasUnsavedChanges = true;
					value.Write(control.Text, save: false);
				});

			value.Subscribe(v =>
				Fusion.Application.MainThread.Schedule(() =>
				{
					valueSetByUser = false;
					try
					{
						control.Text = v;
					}
					finally
					{
						valueSetByUser = true;
					}
				}));

			control.LostKeyboardFocus += (s, a) =>
			{
				if (!hasUnsavedChanges)
					return;

				value.Write(control.Text, save: true);
				hasUnsavedChanges = false;
			};

			if (isMultiline == false)
			{
				control.KeyDown += (s, a) =>
				{
					if (a.Key != System.Windows.Input.Key.Return)
						return;

					if (!hasUnsavedChanges)
						return;

					value.Write(control.Text, save: true);
					hasUnsavedChanges = false;
				};
			}
		}
	}
}
