namespace Fuse.Installer.Gui
{
	/// <summary>
	/// Interaction logic for UninstallView.xaml
	/// </summary>
	public partial class UninstallView
	{
		public UninstallView(UninstallViewModel viewModel)
		{
			DataContext = viewModel;
			InitializeComponent();
		}
	}
}
