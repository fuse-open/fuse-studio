using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public enum Cursor
	{
		Normal,
		ResizeVertically,
		ResizeHorizontally,
		Grab,
		Grabbing,
		Pointing,
		Text,
	}

	public static class Cursors
	{
		public static IControl SetCursor(this IControl control, Cursor color)
		{
			return control.SetCursor(Observable.Return(color));
		}

		public static IControl SetCursor(this IControl control, Func<IControl, IObservable<Cursor>> color)
		{
			return control.SetCursor(color(control));
		}

		public static IControl SetCursor(this IControl control, IObservable<Cursor> color)
		{
			return Implementation.Set(control, color);
		}

		public static class Implementation
		{
			public static Func<IControl, IObservable<Cursor>, IControl> Set;
		}
	}

}