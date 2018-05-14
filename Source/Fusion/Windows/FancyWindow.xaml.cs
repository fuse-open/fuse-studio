using System.Windows;
using System.Windows.Input;

namespace Outracks.Fusion.Windows
{
	public partial class FancyWindow
	{
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
		}

		void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
				DragMove();
		}
	}
}
