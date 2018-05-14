using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks
{
	public static class ReactiveExtensions
	{
		// Switch

		public static Rectangle<IObservable<TOut>> Switch<TIn, TOut>(this IObservable<TIn> self, Func<TIn, Rectangle<IObservable<TOut>>> selector)
		{
			return self.Select(selector).Switch();
		}

		public static Size<IObservable<TOut>> Switch<TIn, TOut>(this IObservable<TIn> self, Func<TIn, Size<IObservable<TOut>>> selector)
		{
			return self.Select(selector).Switch();
		}

		// Switch2

		public static Rectangle<IObservable<T>> Switch<T>(this IObservable<Rectangle<IObservable<T>>> self)
		{
			return new Rectangle<IObservable<T>>(
				self.Select(r => r.HorizontalInterval).Switch(),
				self.Select(r => r.VerticalInterval).Switch());
		}

		public static Interval<IObservable<T>> Switch<T>(this IObservable<Interval<IObservable<T>>> self)
		{
			return Interval.FromOffsetLength(
				self.Select(r => r.Offset).Switch(),
				self.Select(r => r.Length).Switch());
		}

		public static Size<IObservable<T>> Switch<T>(this IObservable<Size<IObservable<T>>> self)
		{
			return new Size<IObservable<T>>(
				self.Select(r => r.Width).Switch(),
				self.Select(r => r.Height).Switch());
		}

		// Distinct until changed

		public static Rectangle<IObservable<T>> DistinctUntilChanged<T>(this Rectangle<IObservable<T>> self)
		{
			return self.Select(c => c.DistinctUntilChanged());
		}

		public static Interval<IObservable<T>> DistinctUntilChanged<T>(this Interval<IObservable<T>> self)
		{
			return self.Select(c => c.DistinctUntilChanged());
		}


		public static Size<IObservable<T>> DistinctUntilChanged<T>(this Size<IObservable<T>> self)
		{
			return self.Select(c => c.DistinctUntilChanged());
		}

		// Replay

		public static Rectangle<IConnectableObservable<T>> Replay<T>(this Rectangle<IObservable<T>> self, int bufferSize)
		{
			return self.Select(c => c.Replay(bufferSize));
		}

		public static Interval<IConnectableObservable<T>> Replay<T>(this Interval<IObservable<T>> self, int bufferSize)
		{
			return self.Select(c => c.Replay(bufferSize));
		}

		public static Size<IConnectableObservable<T>> Replay<T>(this Size<IObservable<T>> self, int bufferSize)
		{
			return self.Select(c => c.Replay(bufferSize));
		}

		// Connect

		public static Rectangle<IDisposable> Connect<T>(this Rectangle<IConnectableObservable<T>> self)
		{
			return self.Select(c => c.Connect());
		}

		public static Size<IDisposable> Connect<T>(this Size<IConnectableObservable<T>> self)
		{
			return self.Select(c => c.Connect());
		}

		// RefCount

		public static Rectangle<IObservable<T>> RefCount<T>(this Rectangle<IConnectableObservable<T>> self)
		{
			return self.Select(c => c.RefCount());
		}

		public static Interval<IObservable<T>> RefCount<T>(this Interval<IConnectableObservable<T>> self)
		{
			return self.Select(c => c.RefCount());
		}

		public static Size<IObservable<T>> RefCount<T>(this Size<IConnectableObservable<T>> self)
		{
			return self.Select(c => c.RefCount());
		}
	}
}