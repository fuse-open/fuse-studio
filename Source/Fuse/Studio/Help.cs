using System.Diagnostics;
using Outracks.Fusion;

namespace Outracks.Fuse.Designer
{
	class Help
	{
		public Help()
		{
			Menu =
				  Menu.Item("Documentation", CreateUrlAction(DocsUrl))
				+ Menu.Item("Examples", CreateUrlAction(ExamplesUrl))
				+ Menu.Item("Community", CreateUrlAction(CommunityUrl));
		}

		static Command CreateUrlAction(string url)
		{
			return Command.Enabled(() =>
			{
				Process.Start(url);
			});
		}
		
		public Menu Menu { get; private set; }

		public static readonly string CommunityUrl = "https://go.fusetools.com/community";
		public static readonly string WeeklyUrl = "http://weekly.fusetools.com/";
		public static readonly string TutorialUrl = "http://go.fusetools.com/tutorials";
		public static readonly string DocsUrl = "https://go.fusetools.com/docs";
		public static readonly string ExamplesUrl = "https://go.fusetools.com/examples";

	}
}