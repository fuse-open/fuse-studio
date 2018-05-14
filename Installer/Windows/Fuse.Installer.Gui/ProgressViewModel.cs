using System.Windows.Input;
using Microsoft.Practices.Prism.ViewModel;

namespace Fuse.Installer.Gui
{
	public class ProgressViewModel : NotificationObject
	{
		public ICommand CancelCommand { get; private set; }
		public string Title { get; set; }

		public ProgressViewModel(ICommand cancelCommand)
		{
			CancelCommand = cancelCommand;
		}

		double _progress;
		public double Progress
		{
			get { return _progress; }
			set
			{
				if (_progress == value)
					return;

				_progress = value;
				RaisePropertyChanged(() => Progress);
			}
		}
	}
}