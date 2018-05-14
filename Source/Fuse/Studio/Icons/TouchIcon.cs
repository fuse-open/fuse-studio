using System;
using System.Reactive.Linq;
using Outracks.Fuse.Theming.Themes;

namespace Outracks.Fuse.Designer.Icons
{
	using Fusion;

	class TouchIcon
	{
		public static IControl Create(IObservable<bool> isEnabled, bool isHittable = false)
		{
			var enabledIcon = Theme.CurrentTheme.Select(
					theme => Image.FromResource(
						theme == Themes.OriginalDark
							? "Outracks.Fuse.Icons.HeaderBar.touchOn_dark.png"
							: "Outracks.Fuse.Icons.HeaderBar.touchOn_light.png", typeof(MinimizeAndMaximizeIcon).Assembly))
				.Switch();
			var disabledIcon = Theme.CurrentTheme.Select(
					theme => Image.FromResource(
						theme == Themes.OriginalDark
							? "Outracks.Fuse.Icons.HeaderBar.touchOff_dark.png"
							: "Outracks.Fuse.Icons.HeaderBar.touchOff_light.png", typeof(MinimizeAndMaximizeIcon).Assembly))
				.Switch();

			return isEnabled.Select(e => e ? disabledIcon : enabledIcon)
				.Switch()
				.WithBackground(Color.Transparent)
				.WithSize(new Size<Points>(15,37));
		}
	}
}
