using System.Diagnostics;
using Outracks.Fusion;

namespace Outracks.Fuse.Studio
{
	class Help
	{
		public Menu Menu { get; private set; }

		public Help()
		{
			Menu =
				  Menu.Item(Texts.SubMenu_Help_Documentation, CreateUrlAction(WebLinks.Documentation))
				+ Menu.Item(Texts.SubMenu_Help_Examples, CreateUrlAction(WebLinks.Examples))
				+ Menu.Item(Texts.SubMenu_Help_Tutorial, CreateUrlAction(WebLinks.Tutorial))
				+ Menu.Separator
				+ Menu.Item(Texts.SubMenu_Help_Community, CreateUrlAction(WebLinks.Community))
				+ Menu.Item(Texts.SubMenu_Help_Forums, CreateUrlAction(WebLinks.Forums))
				+ Menu.Separator
				+ Menu.Item(Texts.SubMenu_Help_ReportIssue, CreateUrlAction(WebLinks.Issues));
		}

		static Command CreateUrlAction(string url)
		{
			return Command.Enabled(() => Process.Start(url));
		}
	}
}
