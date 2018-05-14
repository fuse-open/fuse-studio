namespace Fuse.Installer.Gui
{
	/// <summary>
	/// Interaction logic for ErrorView.xaml
	/// </summary>
	public partial class ErrorView
	{
		public ErrorView(ErrorViewModel model)
		{
			DataContext = model;
			InitializeComponent();
		}
	}
}
