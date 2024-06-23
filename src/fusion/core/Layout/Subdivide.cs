using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using Outracks.Diagnostics;

namespace Outracks.Fusion
{
	public static partial class Layout
	{
		public static IControl Subdivide(Axis2D axis, params IControl[] divisions)
		{
			return Subdivide(axis, divisions.Select(d => new Subdivision(d, 1)).ToImmutableList());
		}

		public static IControl Subdivide(Axis2D axis, IImmutableList<IControl> divisions)
		{
			return Subdivide(axis, divisions.Select(d => new Subdivision(d, 1)).ToImmutableList());
		}

		public static IControl SubdivideHorizontally(IObservable<IEnumerable<IControl>> divisions)
		{
			return divisions.Select(d => Subdivide(Axis2D.Horizontal, d.ToImmutableList())).Switch();
		}

		public static IControl SubdivideHorizontally(params IControl[] divisions)
		{
			return Subdivide(Axis2D.Horizontal, divisions.ToImmutableList());
		}

		public static IControl SubdivideHorizontally(params Subdivision[] divisions)
		{
			return Subdivide(Axis2D.Horizontal, divisions.ToImmutableList());
		}

		public static IControl SubdivideHorizontallyWithSeparator(IControl separator, params IControl[] divisions)
		{
			var divs = new IControl[divisions.Length];
			for (var d = 0; d < divisions.Length - 1; ++d)
			{
				divs[d] = Dock().Right(separator).Fill(divisions[d]);
			}
			divs[divisions.Length - 1] = divisions[divisions.Length - 1];
			return SubdivideHorizontally(divs);
		}

		public static IControl SubdivideVertically(params IControl[] divisions)
		{
			return Subdivide(Axis2D.Vertical, divisions.ToImmutableList());
		}

		public static IControl SubdivideVertically(params Subdivision[] divisions)
		{
			return Subdivide(Axis2D.Vertical, divisions.ToImmutableList());
		}

		public static IControl Subdivide(Axis2D axis, params Subdivision[] divisions)
		{
			return Subdivide(axis, divisions.ToImmutableList());
		}

		public static IControl Subdivide(Axis2D axis, IImmutableList<Subdivision> divisions)
		{
		    if (axis == Axis2D.Vertical && Platform.IsMac)
		        divisions = divisions.Reverse().ToImmutableList();

			var u = axis;
			var v = axis.Opposite();

			var sizesV = divisions.Select(d => d.Control.DesiredSize[v]);

			return Layer((isRooted, availableSize, parentFrame) =>
				{
					var frame = ObservableMath.RectangleWithSize(parentFrame.Size);

					var columns = divisions.Select(d => d.Span).ToArray();
					var intervals = frame[axis].Subdivide(columns);
					var columnCount = columns.Sum();

					var tmp = new IControl[intervals.Length];
					for (int i = 0; i < intervals.Length; i++)
					{
						var span = divisions[i].Span;
						var ratio = span / columnCount;
						var node = divisions[i].Control;
						tmp[i] = node.WithFrame(
							frame.WithAxis(axis, intervals[i]),
							availableSize.WithAxis(axis, ov => ov.Mul(ratio)));
					}

					return tmp;
				})
				.WithSize(desiredSize: Size.Create(
					Observable.Return<Points>(0),
					sizesV.Aggregate(Observable.Return<Points>(0), (a, b) => a.Max(b)),
					firstAxis: u));
		}

		public static Subdivision Span(this IControl control, double span)
		{
			return new Subdivision(control, span);
		}
	}

	public sealed class Subdivision
	{
		public readonly IControl Control;
		public readonly double Span;

		public Subdivision(IControl control, double span)
		{
			Control = control;
			Span = span;
		}
	}
}