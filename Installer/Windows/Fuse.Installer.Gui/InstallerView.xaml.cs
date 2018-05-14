using System.Windows;
using System.Windows.Controls;

namespace Fuse.Installer.Gui
{
	/// <summary>
	/// Interaction logic for Introduction.xaml
	/// </summary>
	public partial class InstallerView
	{
		readonly InstallerViewModel _model;
		public InstallerView(InstallerViewModel model)
		{
			_model = model;
			DataContext = model;
			InitializeComponent();
		}

		void LicenseAgreedCheck(object sender, RoutedEventArgs e)
		{
			var checkBox = e.Source as CheckBox;
			if (checkBox != null && checkBox.IsChecked.HasValue)
			{
				_model.UpdateLicenseAgreedState(checkBox.IsChecked.Value);
			}
			else
			{
				e.Handled = false;
			}
		}
	}
}
