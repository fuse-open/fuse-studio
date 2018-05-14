namespace Outracks.Fuse
{
	using Fusion;

	public static class AutoReloadContent
	{
		public static IControl Create()
		{
			return Layout.StackFromTop(
				Label.Create(
					text: "Open AutoReloadContent.cs, and put your content there",
					font: Font.SystemDefault(40),
					color: Theme.DefaultText).Center(),
				Label.Create(
					text: "Enjoy autoreload! :)",
					font: Font.SystemDefault(30),
					color: Theme.DefaultText).Center())
				.Center()
				.WithBackground(Theme.PanelBackground);
		}
	}
}