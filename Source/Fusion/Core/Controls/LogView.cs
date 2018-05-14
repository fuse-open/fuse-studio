using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static class LogView
	{
		/// <param name="stream">
		/// An observable sequence of characters to append to the end of the log, where '\n' moves to the next line. 
		/// The caller is responsible for ending this stream when the view is no longer in use.
		/// </param>
		public static IControl Create(IObservable<string> stream = null, IObservable<Color> color = null, IObservable<Unit> clear = null, IObservable<bool> darkTheme = null)
		{
			return Implementation.Factory(
				stream ?? Observable.Never<string>(), 
				color ?? Observable.Return(Color.White), 
				clear ?? Observable.Never<Unit>(), 
				darkTheme ?? Observable.Return(false));
		}

		public static class Implementation
		{
			public static Func<IObservable<string>, IObservable<Color>, IObservable<Unit>, IObservable<bool>, IControl> Factory;
		}

	}
}