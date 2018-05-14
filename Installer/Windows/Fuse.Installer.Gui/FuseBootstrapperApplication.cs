using System.Windows.Threading;
using Fuse.Installer.Gui.Domain;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace Fuse.Installer.Gui
{
	public class FuseBootstrapperApplication : BootstrapperApplication
	{
		public FuseBootstrapperApplication()
		{
			var myViewModel = new InstallerService(this);
			Dispatcher = Dispatcher.CurrentDispatcher;
		}

		public static Dispatcher Dispatcher { get; set; }

		protected override void Run()
		{
			Dispatcher = Dispatcher.CurrentDispatcher;

			var model = new BootstrapperApplicationModel(this);
			var viewModel = new MainWindowModel(model);
			var view = new MainWindow(viewModel);

			model.SetWindowHandle(view);
			Engine.Detect();
			if (NotUninstallingDuringUpgrade(model))
			{
				view.Show();
			}
			Dispatcher.Run();
			Engine.Quit(model.FinalResult);
		}

		private static bool NotUninstallingDuringUpgrade(BootstrapperApplicationModel model)
		{
			// LaunchAction.Uninstall && Display.Embedded means uninstall is called from 
			// another bootstrapper during an upgrade.
			return !(model.BootstrapperApplication.Command.Action == LaunchAction.Uninstall && 
					 model.BootstrapperApplication.Command.Display == Display.Embedded);
		}
	}
}