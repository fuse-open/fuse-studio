using System.Reactive.Linq;

namespace Outracks.Fusion.IntegrationTests
{
	static class ControlTestExtensions
	{
		public static void MountRoot(this IControl control)
		{
			var size = Size.Create(Observable.Return(new Points(4)), Observable.Return(new Points(4)));
			control.Mount(
				new MountLocation.Mutable
				{
					IsRooted = Observable.Return(true),
					AvailableSize = size,
					NativeFrame = ObservableMath.RectangleWithSize(size)
				});
		}
	}
}