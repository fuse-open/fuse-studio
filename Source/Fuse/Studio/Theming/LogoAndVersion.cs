using System;
using System.Reactive.Linq;
using Outracks.Fusion;

namespace Outracks.Fuse.Designer
{
	public static class LogoAndVersion
	{
		public static IControl Create(Version fuseVersion)
		{
			var versionStr = "Version " + fuseVersion.ToString(3) + " (build " + fuseVersion.Revision + ")";
			return Layout.StackFromTop(
				Image.FromResource("Outracks.Fuse.Icons.FuseProLogo.png", typeof(LogoAndVersion).Assembly, Theme.DefaultText)
					.WithSize(new Size<Points>(336 * 0.65, 158 * 0.65))
					.WithPadding(top: new Points(20), bottom: new Points(5)),
				Label.Create(
						versionStr,
						font: Theme.DescriptorFont,
						color: Theme.DefaultText,
						textAlignment: TextAlignment.Center)
					.WithPadding(new Thickness<Points>(0, 0, 0, 10)));
		}
	}
}