using System;
using System.Reactive.Linq;
using Outracks.Fuse.Theming.Themes;

namespace Outracks.Fuse.Designer.Icons
{
	using Fusion;

	class SelectionIcon
	{
		public static IControl Create(IObservable<bool> isEnabled, bool isHittable = false)
		{
			var enabledIcon = Theme.CurrentTheme.Select(
					theme => Image.FromResource(
						theme == Themes.OriginalDark
							? "Outracks.Fuse.Icons.HeaderBar.SelectionOn_dark.png"
							: "Outracks.Fuse.Icons.HeaderBar.SelectionOn_light.png", typeof(MinimizeAndMaximizeIcon).Assembly))
				.Switch();
			var disabledIcon = Theme.CurrentTheme.Select(
					theme => Image.FromResource(
						theme == Themes.OriginalDark
							? "Outracks.Fuse.Icons.HeaderBar.SelectionOff_dark.png"
							: "Outracks.Fuse.Icons.HeaderBar.SelectionOff_light.png", typeof(MinimizeAndMaximizeIcon).Assembly))
				.Switch();

			return isEnabled.Select(e => e ? enabledIcon : disabledIcon)
				.Switch()
				.WithBackground(Color.Transparent)
				.WithSize(new Size<Points>(11,37));
		}
	}
}
