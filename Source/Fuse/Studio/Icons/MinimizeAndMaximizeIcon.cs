using System;
using System.Reactive.Linq;
using Outracks.Fusion;
using Outracks.Fuse.Theming.Themes;


namespace Outracks.Fuse.Designer.Icons
{
	static class MinimizeAndMaximizeIcon
	{
		public static IControl Create(IObservable<Mode> mode)
		{
			var minimize = Theme.CurrentTheme.Select(
					theme => Image.FromResource(
						theme == Themes.OriginalDark
							? "Outracks.Fuse.Icons.HeaderBar.Minimize_dark.png"
							: "Outracks.Fuse.Icons.HeaderBar.Minimize_light.png", typeof(MinimizeAndMaximizeIcon).Assembly))
				.Switch();
			var maximize = Theme.CurrentTheme.Select(
					theme => Image.FromResource(
						theme == Themes.OriginalDark
							? "Outracks.Fuse.Icons.HeaderBar.Maximize_dark.png"
							: "Outracks.Fuse.Icons.HeaderBar.Maximize_light.png", typeof(MinimizeAndMaximizeIcon).Assembly))
				.Switch();

			return mode.Select(m => m == Mode.Compact ?  maximize.WithSize(new Size<Points>(16,16)) : minimize.WithSize(new Size<Points>(15,15)))
				.Switch();
		}
	}
}
               