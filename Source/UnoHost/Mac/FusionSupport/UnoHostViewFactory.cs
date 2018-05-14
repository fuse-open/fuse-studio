using System;
using System.Reactive.Linq;
using AppKit;

namespace Outracks.UnoHost.OSX.FusionSupport
{
	using Protocol;

	public class UnoHostViewFactory
	{
		public IObservable<FocusState> Focus { get; private set; }
		public NSView View { get; private set; }

		public static UnoHostViewFactory Create(IObservable<IBinaryMessage> messagesFromHost, IObserver<IBinaryMessage> messagesToHost)
		{
			var surfaceCache = new SurfaceCache();

			var serverView = new ServerView(surfaceCache)
			{
				WantsBestResolutionOpenGLSurface = true
			};

			Observable
				.Merge(
					serverView.Events.Select(CocoaEventMessage.Compose),
					serverView.Size.CombineLatest(serverView.Density, (s,d) => ResizeMessage.Compose(new SizeData(s, d))))
				.Subscribe(messagesToHost.OnNext);

			messagesFromHost
				.SelectSome(NewSurfaceMessage.TryParse)
				.Subscribe(surfaceCache.SwapAndUpdateCache, e => Console.WriteLine(e));

			return new UnoHostViewFactory()
			{
				View = serverView,
				Focus = serverView.Focus
			};
		}
	}
}