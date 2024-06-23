using System;
using System.Linq;
using Outracks.Fusion;

namespace Outracks.Fuse.Stage
{
	class BoxFactory
	{
		public static IControl CreateSpacingBox(
			IObservable<Rectangle<Points>> bounds,
			IObservable<Thickness<Points>> margin,
			IObservable<Thickness<Points>> padding)
		{
			return Layout.Layer(
				AcquireBox(
					outer: bounds,
					inner: bounds.Deflate(padding/*.CombineLatest(_paddingCues, Thickness.CollapseEdgesExcept)*/.Max(Thickness.Zero)),
					color: Theme.Padding),
				AcquireBox(
					outer: bounds.Inflate(margin/*.CombineLatest(_marginCues, Thickness.CollapseEdgesExcept)*/.Max(Thickness.Zero)),
					inner: bounds,
					color: Theme.Margin));
		}

		static IControl AcquireBox(IObservable<Rectangle<Points>> outer, IObservable<Rectangle<Points>> inner, Brush color)
		{
			return CreateBox(outer.Transpose(), inner.Transpose(), color);
		}

		static IControl CreateBox(Rectangle<IObservable<Points>> outer, Rectangle<IObservable<Points>> inner, Brush color)
		{
			var fills = new[]
			{
				outer.With(left: inner.Right()),
				outer.With(right: inner.Left()),
				inner.With(top: outer.Top(), bottom: inner.Top()),
				inner.With(bottom: outer.Bottom(), top: inner.Bottom()),
				inner.With(bottom: outer.Top()),
			};

			var strokes = new[]
			{
				inner, outer,
			};

			return fills
				.Select(fill => Shapes
					.Rectangle(fill: color.WithAlpha(0.2f))
					.WithFixedPosition(fill))
				.Concat(strokes
					.Select(stroke => Shapes
						.Rectangle(stroke: Stroke.Create(1, color))
						.WithFixedPosition(stroke)))
				.Layer();
		}

	}
}