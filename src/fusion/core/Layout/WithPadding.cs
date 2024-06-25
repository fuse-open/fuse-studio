using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static partial class Layout
	{
		public static IControl WithPadding(
			this IControl control,
			IObservable<Thickness<Points>> frame)
		{
			return control.WithPadding(
				frame.Select(f => f.Left),
				frame.Select(f => f.Top),
				frame.Select(f => f.Right),
				frame.Select(f => f.Bottom));
		}

		public static IControl WithPadding(
			this IControl control,
			Thickness<Points> thickness)
		{
			return control.WithPadding(
				thickness.Left,
				thickness.Top,
				thickness.Right,
				thickness.Bottom);
		}

		public static IControl WithPadding(
			this IControl control,
			Optional<Points> left = default(Optional<Points>),
			Optional<Points> top = default(Optional<Points>),
			Optional<Points> right = default(Optional<Points>),
			Optional<Points> bottom = default(Optional<Points>))
		{
			return control.WithPadding(
				left.Select(Observable.Return).OrDefault(),
				top.Select(Observable.Return).OrDefault(),
				right.Select(Observable.Return).OrDefault(),
				bottom.Select(Observable.Return).OrDefault());
		}

		public static IControl WithPadding(
			this IControl control,
			Func<IControl, IObservable<Points>> left = null,
			Func<IControl, IObservable<Points>> top = null,
			Func<IControl, IObservable<Points>> right = null,
			Func<IControl, IObservable<Points>> bottom = null)
		{
			return control.WithPadding(
				left == null ? null : left(control),
				top == null ? null : top(control),
				right == null ? null : right(control),
				bottom == null ? null : bottom(control));
		}

		public static IControl WithPadding(
			this IControl control,
			IObservable<Points> left = null,
			IObservable<Points> top = null,
			IObservable<Points> right = null,
			IObservable<Points> bottom = null)
		{
			var tmp = Thickness
				.Create(left, top, right, bottom)
				.FlipVerticallyOnMac();

			left = tmp.Left;
			top = tmp.Top;
			right = tmp.Right;
			bottom = tmp.Bottom;

			control = control.WithFrame(
				frame =>
				{
					if (left != null)   frame = Rectangle.FromPositionSize(frame.Position.X.Add(left), frame.Position.Y,          frame.Width.Sub(left),  frame.Height);
					if (top != null)    frame = Rectangle.FromPositionSize(frame.Position.X,           frame.Position.Y.Add(top), frame.Width,            frame.Height.Sub(top));
					if (right != null)  frame = Rectangle.FromPositionSize(frame.Position.X,           frame.Position.Y,          frame.Width.Sub(right), frame.Height);
					if (bottom != null) frame = Rectangle.FromPositionSize(frame.Position.X,           frame.Position.Y,          frame.Width,            frame.Height.Sub(bottom));
					return frame;
				},
				availableSize =>
				{
					if (left != null)   availableSize = availableSize.WithAxis(Axis2D.Horizontal, p => p.Sub(left));
					if (top != null)    availableSize = availableSize.WithAxis(Axis2D.Vertical,   p => p.Sub(top));
					if (right != null)  availableSize = availableSize.WithAxis(Axis2D.Horizontal, p => p.Sub(right));
					if (bottom != null) availableSize = availableSize.WithAxis(Axis2D.Vertical,   p => p.Sub(bottom));
					return availableSize;
				});

			var desiredSize = control.DesiredSize;

			if (left != null)   desiredSize = desiredSize.WithAxis(Axis2D.Horizontal, p => p.Add(left));
			if (top != null)    desiredSize = desiredSize.WithAxis(Axis2D.Vertical,   p => p.Add(top));
			if (right != null)  desiredSize = desiredSize.WithAxis(Axis2D.Horizontal, p => p.Add(right));
			if (bottom != null) desiredSize = desiredSize.WithAxis(Axis2D.Vertical,   p => p.Add(bottom));

			return control.WithSize(desiredSize);
		}
	}
}