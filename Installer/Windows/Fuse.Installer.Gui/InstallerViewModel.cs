using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

namespace Fuse.Installer.Gui
{
	public class InstallerViewModel : NotificationObject
	{
		public ICommand GotoLicensePageCommand { get; private set; }
		public ICommand InstallCommand { get; private set; }
		public ICommand CancelCommand { get; private set; }

		bool _licenseAgreed;
		public InstallerViewModel(ICommand installCommand, ICommand cancelCommand)
		{
			GotoLicensePageCommand = new DelegateCommand(() => Process.Start("https://go.fusetools.com/license-fuse"));
			InstallCommand = 
				new DelegateCommand(() => installCommand.Execute(null),
					() => _licenseAgreed);
			CancelCommand = cancelCommand;
		}

		public void UpdateLicenseAgreedState(bool isAgreed)
		{
			_licenseAgreed = isAgreed;
			((DelegateCommand)InstallCommand).RaiseCanExecuteChanged();
		}
	}
}