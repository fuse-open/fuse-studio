using System;
using System.Windows;

namespace Fuse.Installer.Gui
{
	public class App : Application
	{
		[STAThread]
		public static void Main(string[] args)
		{
			var model = new BootstrapperApplicationModel(new FuseBootstrapperApplication());
			var viewModel = new MainWindowModel(model);
			var view = new MainWindow(viewModel);
			model.SetWindowHandle(view);
			view.Show();
			new App().Run(view);
		}
	}
}