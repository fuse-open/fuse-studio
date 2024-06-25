
namespace Outracks.Simulator
{
	public partial class DeveloperMenu
	{
		void GoBack(object sender, Uno.EventArgs args)
		{
			Fuse.Input.Keyboard.EmulateBackButtonTap();
		}

		void Close(object sender, Uno.EventArgs args)
		{
			Parent.BeginRemoveVisual(this);
		}
	}
}