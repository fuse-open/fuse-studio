using System;
using System.Reactive.Linq;

namespace Outracks.Fuse.Inspector.Editors
{
	using Fusion;

	static class ListButtons
	{
		public static IControl RemoveButton(Action clicked, IObservable<bool> isEnabled = null)
		{
			isEnabled = isEnabled ?? Observable.Return(true);

			var command = Command.Create(isEnabled, clicked);
			var stroke = IconStroke(isEnabled);
			
			return Button.Create(command, state => 
				Shapes.Line(
					new Point<Points>(0, Width / 2),
					new Point<Points>(Width, Width / 2),
					stroke: stroke)
				.WithWidth(Width)
				.WithHeight(Width));
		}

		public static IControl AddButton(Action clicked, IObservable<bool> isEnabled = null)
		{
			isEnabled = isEnabled ?? Observable.Return(true);
			
			var command = isEnabled.Switch(e => Command.Create(e, clicked));
			var stroke = IconStroke(isEnabled);
			
			return Button.Create(command, state => 
				Layout.Layer(
					Shapes.Line(
						new Point<Points>(Width / 2, 0),
						new Point<Points>(Width / 2, Width),
						stroke: stroke),
					Shapes.Line(
						new Point<Points>(0, Width / 2),
						new Point<Points>(Width, Width / 2),
						stroke: stroke))
					.WithWidth(Width)
					.WithHeight(Width));
		}

		static readonly Points Width = 10.0;

		static Stroke IconStroke(IObservable<bool> isEnabled)
		{
			return Stroke.Create(
				thickness: 2,
				brush: isEnabled.Select(c => c
					? Theme.Active
					: Separator.WeakStroke.Brush).Switch());
		}
	}
}