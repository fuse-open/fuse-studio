using System.Reactive.Linq;
using Outracks.Fusion;

namespace Outracks.Fuse.Studio.Inspector
{
	static class StackIcon
	{
		public static IControl Create(Axis2D axis, Brush brush)
		{
			var four = Observable.Return<Points>(3);
			var two = Observable.Return<Points>(1);
			var gentle = Observable.Return(new CornerRadius(1));

			return Layout
				.Stack(
					axis == Axis2D.Horizontal ? Direction2D.LeftToRight : Direction2D.TopToBottom,
					Shapes.Rectangle(fill: brush, cornerRadius: gentle).WithDimension(axis, four),
					Control.Empty.WithDimension(axis, two),
					Shapes.Rectangle(fill: brush, cornerRadius: gentle).WithDimension(axis, four),
					Control.Empty.WithDimension(axis, two),
					Shapes.Rectangle(fill: brush, cornerRadius: gentle).WithDimension(axis, four))
				.WithDimension(axis.Opposite(), Observable.Return<Points>(12))
				.Center()
				.WithSize(new Size<Points>(12,16))
				;
		}
	}
}