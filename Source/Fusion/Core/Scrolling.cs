using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public class ZoomAttributes
	{
		public readonly float MinZoom;
		public readonly float MaxZoom;

		public ZoomAttributes(float minZoom, float maxZoom)
		{
			MinZoom = minZoom;
			MaxZoom = maxZoom;
		}
	}

	public static class Scrolling
	{
		public static IControl MakeScrollable(
			this IControl control,
			IObservable<bool> darkTheme = null,
			bool supportsOpenGL = false,
			ZoomAttributes zoomAttributes = null,
			IProperty<Points> verticalOffset = null,
			IProperty<Points> horizontalOffset = null,
			Action<ScrollBounds> onBoundsChanged = null,
			IObservable<Rectangle<Points>> scrollToRectangle = null,
			bool verticalScrollBarVisible = true,
			bool horizontalScrollBarVisible = true)
		{
			return Implementation.Factory(
				control,
				darkTheme ?? Observable.Return(false),
				supportsOpenGL,
				zoomAttributes.ToOptional(),
				onBoundsChanged.ToOptional(),
				scrollToRectangle.ToOptional(),
				verticalScrollBarVisible,
				horizontalScrollBarVisible);
		}

		public static class Implementation
		{
			public static Func<IControl, IObservable<bool>, bool, Optional<ZoomAttributes>, Optional<Action<ScrollBounds>>, Optional<IObservable<Rectangle<Points>>>, bool, bool, IControl> Factory;
		}
	}
}
