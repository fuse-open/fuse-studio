using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static partial class Layout
	{
		public static IControl DirectionalGrid(this IObservable<IEnumerable<IControl>> controls, Axis2D axis, int numSegments)
		{
			if(numSegments <= 0)
				throw new ArgumentException("Segment count must higher than '0'", "numSegments");

			return controls.Select(
					ctrls =>
					{
						var segments = new List<IControl>[numSegments];
						var i = 0;
						foreach (var item in ctrls)
						{
							var idx = i % numSegments;
							if (segments[idx] == null)
								segments[idx] = new List<IControl>();
							segments[idx].Add(item);
							++i;
						}

						var stacks = segments.Select(
							l => l == null
								? Control.Empty
								: Stack(axis == Axis2D.Horizontal ? Direction2D.TopToBottom : Direction2D.LeftToRight, l)
						).ToImmutableList();

						return Subdivide(axis, stacks);
					})
				.Switch();
		}
	}
}
