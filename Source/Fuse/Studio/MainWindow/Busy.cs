using System;
using System.Reactive.Linq;
using Outracks.Fuse.Designer;
using Outracks.Fusion;

namespace Outracks.Fuse
{
	static class Busy
	{
		public static IControl Create(string message, IObservable<Color> background, IObservable<Color> foreground)
		{
			
			return Layout.Dock()
				.Left(
					Label.Create(message, font: Theme.DefaultFont, color: foreground.AsBrush())
						.CenterVertically()
						.WithPadding(new Thickness<Points>(8,0,0,0)))
				.Right( 
					BusyIndicator(foreground)
						.WithHeight(8)
						.WithWidth(52)
						.CenterVertically()
						.WithPadding(new Thickness<Points>(0,0,8,0)))
				.Fill()
				.WithBackground(Shapes.Rectangle(fill: background.AsBrush()))
				.WithHeight(24)
				.MakeCollapsable(RectangleEdge.Top, new[] { false, true }.ToObservable(), lazy: false);
		}

		static IControl BusyIndicator(IObservable<Color> overlaycolor)
		{
			return Image.Animation("FuseBusyAnim", typeof(LogoAndVersion).Assembly, TimeSpan.FromMilliseconds(1000), overlaycolor);
		}

	}
}