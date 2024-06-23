using System;
using System.Linq;
using System.Reactive.Linq;
using Outracks.Fuse.Studio;
using Outracks.Fusion;

namespace Outracks.Fuse
{
	static class Notification
	{
		public static IControl Create(string message, params Tuple<string, Command>[] commands)
		{
			return Layout.Dock()
				.Right(Spacer.Small)
				.Right(Observable.Return(
					commands.Select(cmd =>
						Buttons.NotificationButton(cmd.Item1, cmd.Item2, Theme.ErrorColor)
							.WithWidth(80)
							.WithHeight(18)
							.Center()))
					.StackFromLeft(separator: () => Spacer.Small))
				.Right(Spacer.Small)
				.Right(Label.Create(message, Theme.DescriptorFont, color: Color.White)
					.CenterVertically())
				.Fill()
				.WithBackground(Shapes.Rectangle(fill: Theme.ErrorColor))
				.WithHeight(24)
				.MakeCollapsable(RectangleEdge.Top, new [] { false, true }.ToObservable(), lazy: false);
		}
	}
}