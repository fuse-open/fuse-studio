using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static class TextExtension
	{
		public static Text AsText(this IObservable<string> observable)
		{
			return new Text(observable.Replay(1).RefCount());
		}

		public static Text Switch(this IObservable<Text> brush)
		{
			return new Text(brush.Select(b => (IObservable<string>)b).Switch().Replay(1).RefCount());
		}
	}

	public struct Text : IObservable<string>
	{
		public static Text operator +(Text left, Text right)
		{
			return new Text(Observable.CombineLatest(left, right, (l, r) => l + r));
		}

		public static implicit operator Text(string constant)
		{
			return new Text(Observable.Return(constant));
		}

		public static Text Empty
		{
			get { return new Text(null); }
		}

		readonly IObservable<string> _observable;

		internal Text(IObservable<string> observable)
		{
			_observable = observable;
		}

		/// <summary>
		/// Returns true IFF this instance is default(Text), aka Text.Empty
		/// In this case Subscribe will always push an empty string
		/// </summary>
		public bool IsDefault
		{
			get { return _observable == null; }
		}

		public IDisposable Subscribe(IObserver<string> observer)
		{
			if (_observable == null)
			{
				observer.OnNext("");
				observer.OnCompleted();
				return Disposable.Empty;
			}

			return _observable.Subscribe(observer);
		}
	}
}
