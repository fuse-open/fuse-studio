using System.Reactive.Linq;
using Outracks.Fusion;

namespace Outracks.Fuse
{
	public class ThemedButton
	{
		public static IControl Create(
			Command command,
			Text label,
			IControl icon = null,
			Text tooltip = default(Text),
			Brush hoverColor = default(Brush))
		{
			icon = icon ?? Control.Empty;
			hoverColor = hoverColor | Theme.Purple;
			return Button.Create(
					command,
					bs => Layout.StackFromLeft(
							icon.WithPadding(new Thickness<Points>(7, 0)).CenterVertically(),
							Label.Create(
									label,
									Theme.DefaultFont,
									color: Observable.CombineLatest(
										bs.IsEnabled, bs.IsHovered,
										(enabled, hovering) =>
											enabled
												? (hovering ? hoverColor : Theme.DefaultText)
												: Theme.DisabledText)
										.Switch())
								.CenterVertically())
						.Center(),
					tooltip)
				.WithHeight(32);
		}
	}
}