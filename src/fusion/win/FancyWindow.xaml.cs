using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Outracks.Fusion.Windows
{
	public partial class FancyWindow
	{
		public System.Windows.Controls.Menu Menu { get; } = new System.Windows.Controls.Menu();

		public FancyWindow(bool hasTitleBar)
		{
			CustomTitlebar.ApplyTo(this);

			InitializeComponent();
			if (hasTitleBar)
				Style = (Style) Resources["FancyWindowStyle"];
			else
			{
				WindowStyle = System.Windows.WindowStyle.None;
				Style = (Style) Resources["NoTitlebar"];
			}

			MouseDown += OnMouseDown;
			SizeChanged += OnSizeChanged;
		}

		void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
				DragMove();
		}

		void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (Template.FindName("WindowTitleTextBlock", this) is TextBlock title)
				title.Opacity = Math.Max(0, Math.Min(1, (ActualWidth * .5 - Menu.ActualWidth - title.ActualWidth * .5 - 28) / (title.ActualWidth * .5)));
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			var container = Template.FindName("MenuContainer", this) as StackPanel;
			container?.Children.Add(Menu);
		}
	}
}
