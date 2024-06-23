using System;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;

namespace Outracks.Fusion.Windows
{
	class LogViewImplementation
	{
		public static void Initialize(Dispatcher dispatcher)
		{
			LogView.Implementation.Factory = (stream, color, linkColor, clear, dark) =>
			{
				TextEditor textBox = null;

				return Control.Create(self =>
				{
					textBox = new TextEditor()
					{
						IsReadOnly = true,
						Background = Brushes.Transparent,
						VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
						HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
					};

					textBox.SizeChanged += (sender, args) =>
					{
						var scrollToEnd = args.PreviousSize.Height + textBox.VerticalOffset + 20 >= textBox.ExtentHeight;
						if (scrollToEnd)
							textBox.ScrollToEnd();
					};

					stream.Buffer(TimeSpan.FromSeconds(1.0 / 30.0))
						.Where(c => c.Count > 0)
						.ObserveOn(Fusion.Application.MainThread)
						.Subscribe(msgsToAdd =>
						{
							var shouldScrollToEnd = textBox.ViewportHeight + textBox.VerticalOffset + 20 >= textBox.ExtentHeight;

							textBox.BeginChange();
							foreach(var msg in msgsToAdd)
								textBox.AppendText(msg);
							textBox.EndChange();

							if (shouldScrollToEnd)
								textBox.ScrollToVerticalOffset(double.MaxValue);
						});

					clear.ObserveOn(Fusion.Application.MainThread)
						.Subscribe(_ => textBox.Clear());

					self.BindNativeDefaults(textBox, dispatcher);

					self.BindNativeProperty(dispatcher, "color", color,
						value =>
						{
							textBox.Foreground = new SolidColorBrush(value.ToColor());
						});

					self.BindNativeProperty(dispatcher, "linkColor", linkColor,
						value => {
							textBox.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(value.ToColor());
						});

					return textBox;
				}).SetContextMenu(
					Menu.Item(name: "Copy", command: Command.Enabled(() => textBox.Copy()))
					+ Menu.Item(name: "Select All", command: Command.Enabled(() => textBox.SelectAll()))
				);
			};
		}
	}
}
