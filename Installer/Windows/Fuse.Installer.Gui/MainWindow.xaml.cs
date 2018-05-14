namespace Fuse.Installer.Gui
{
	public partial class MainWindow : CustomTitlebarWindow
	{
		public MainWindow(MainWindowModel viewModel)
		{
			InitializeComponent();
			DataContext = viewModel;
			Closed += (sender, e) => viewModel.CancelCommand.Execute(this);
		}
	}
}