using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static class WebView
	{
		public static IControl Create(Uri uri, Action<Navigating> onNavigating = null)
		{
			return Implementation.UrlFactory(uri, onNavigating ?? (_ => { }));
		}

		public static IControl Create(IObservable<string> content)
		{
			return Implementation.StringFactory(content);
		}

		public static IControl Create(string content)
		{
			return Implementation.StringFactory(Observable.Return(content));
		}

		public static class Implementation
		{
			public static Func<Uri, Action<Navigating>, IControl> UrlFactory;
			public static Func<IObservable<string>, IControl> StringFactory;
		}
	}


	public class Navigating
	{
		public Navigating(Uri destination)
		{
			Destination = destination;
		}

		public Uri Destination { get; private set; }
		public bool Cancel { get; set; }
	}

}