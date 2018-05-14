using System.Windows.Input;

namespace Fuse.Installer.Gui
{
	public class UninstallViewModel
	{
		public ICommand UninstallCommand { get; private set; }

		public UninstallViewModel(ICommand uninstallCommand)
		{
			UninstallCommand = uninstallCommand;
		}
	}
}