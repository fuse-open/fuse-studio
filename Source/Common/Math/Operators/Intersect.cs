using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static partial class Rectangle
	{
		public static IObservable<Rectangle<T>> Intersect<T>(
			this IObservable<Rectangle<T>> self,
			IObservable<Rectangle<T>> other)
			where T : INumeric<T>
		{
			return self.CombineLatest(other, Intersect);
		}

		public static Rectangle<T> Intersect<T>(this Rectangle<T> self, Rectangle<T> other)
			where T : INumeric<T>
		{
			var left = self.Left().Max(other.Left());
			var top = self.Top().Max(other.Top());
			
			return FromSides(
				left: left,
				top: top,
				right: self.Right().Min(other.Right()).Max(left),
				bottom: self.Bottom().Min(other.Bottom()).Max(top));
		}

		public static Rectangle<IObservable<T>> Intersect<T>(this Rectangle<IObservable<T>> self, Rectangle<T> other)
			where T : INumeric<T>
		{
			var left = self.Left().Max(other.Left());
			var top = self.Top().Max(other.Top());

			return FromSides(
				left: left,
				top: top,
				right: self.Right().Min(other.Right()).Max(left),
				bottom: self.Bottom().Min(other.Bottom()).Max(top));
		}

		public static Rectangle<IObservable<T>> Intersect<T>(this Rectangle<IObservable<T>> self, Rectangle<IObservable<T>> other)
			where T : INumeric<T>
		{
			var left = self.Left().Max(other.Left());
			var top = self.Top().Max(other.Top());

			return FromSides(
				left: left,
				top: top,
				right: self.Right().Min(other.Right()).Max(left),
				bottom: self.Bottom().Min(other.Bottom()).Max(top));
		}
	}
}