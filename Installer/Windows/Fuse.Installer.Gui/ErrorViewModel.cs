using System.Windows.Input;

namespace Fuse.Installer.Gui
{
	public class ErrorViewModel
	{
		public string ErrorMessage { get; private set; }
		public ICommand OkCommand { get; private set; }

		public ErrorViewModel(ICommand okCommand, string errorMessage)
		{
			OkCommand = okCommand;
			ErrorMessage = errorMessage + "\n\nPlease run the installer again, but from the command prompt: `FuseSetup.exe -l somelogfile.txt`.";
		}
	}
}