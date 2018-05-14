using System.Reactive.Concurrency;

namespace Outracks.Fusion.OSX
{
	public static class OverlayImplementation
	{
		public static void Initialize(IScheduler dispatcher)
		{
			Overlay.Initialize((background, foreground) => 
				Layout.Layer(background, foreground)
					.WithSize(background.DesiredSize));
		}
	}
}