using System.Windows.Controls;

namespace Fuse.Installer.Gui
{
	/// <summary>
	/// Interaction logic for ProgressView.xaml
	/// </summary>
	public partial class ProgressView : Grid
	{
		public ProgressView(ProgressViewModel model)
		{
			DataContext = model;
			InitializeComponent();
		}
	}
}
