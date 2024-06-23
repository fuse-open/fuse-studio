using Outracks.Fusion;

namespace Outracks.Mac.MenuBarApp
{
	static class MainClass
	{
		static void Main()
		{
			Fuse.Tray.Program.Start(Icon.FromResource("Outracks.Mac.MenuBarApp.Resources.Fuse.icns"));
		}
	}
}
