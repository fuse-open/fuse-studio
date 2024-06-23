using Outracks.Fuse.Theming.Themes;
using Outracks.Fusion;
using System.Reactive.Linq;

namespace Outracks.Fuse.Studio
{
	public static class LogoAndVersion
	{
		public static IControl Create(string fuseVersion)
		{
			var versionStr = "Version " + fuseVersion;
			return Layout.StackFromTop(
				Logo()
					.WithSize(new Size<Points>(336 * 0.75, 173 * 0.75))
					.WithPadding(top: new Points(20), bottom: new Points(5)),
				Label.Create(
						versionStr,
						font: Theme.DescriptorFont,
						color: Theme.DescriptorText,
						textAlignment: TextAlignment.Center)
					.WithPadding(new Thickness<Points>(0, 0, 0, 10)));
		}

		public static IControl Logo()
		{
			return Theme.CurrentTheme.Select(
					theme => Image.FromResource(
						theme == Themes.OriginalDark
							? "Outracks.Fuse.Icons.FuseProLogo_dark.png"
							: "Outracks.Fuse.Icons.FuseProLogo_light.png", typeof(LogoAndVersion).Assembly))
				.Switch();
		}
	}
}
