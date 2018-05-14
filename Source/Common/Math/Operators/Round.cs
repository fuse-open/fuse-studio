using System;
using System.Reactive.Linq;

namespace Outracks
{

	public static partial class Numeric
	{
		public static IObservable<T> Round<T>(this IObservable<T> self) where T : INumeric<T>
		{
			return self.Select(Round);
		}

		public static T Round<T>(this T self) where T : INumeric<T>
		{
			return self.FromDouble(Math.Round(self.ToDouble()));
		}
	}

	public static partial class Rectangle
	{
		public static IObservable<Rectangle<T>> Round<T>(this IObservable<Rectangle<T>> self) where T : INumeric<T>
		{
			return self.Select(Round);
		}

		public static Rectangle<IObservable<T>> Round<T>(this Rectangle<IObservable<T>> self) where T : INumeric<T>
		{
			return self.Select(Numeric.Round);
		}

		public static Rectangle<T> Round<T>(this Rectangle<T> self) where T : INumeric<T>
		{
			return self.Select(Numeric.Round);
		}
	}

	public static partial class Vector
	{
		public static Vector<IObservable<T>> Round<T>(this Vector<IObservable<T>> self) where T : INumeric<T>
		{
			return self.Select(Numeric.Round);
		}

		public static Vector<T> Round<T>(this Vector<T> self) where T : INumeric<T>
		{
			return self.Select(Numeric.Round);
		}
	}

	public static partial class Point
	{
		public static Point<IObservable<T>> Round<T>(this Point<IObservable<T>> self) where T : INumeric<T>
		{
			return self.Select(Numeric.Round);
		}

		public static Point<T> Round<T>(this Point<T> self) where T : INumeric<T>
		{
			return self.Select(Numeric.Round);
		}
	}

	public static partial class Size
	{
		public static Size<IObservable<T>> Round<T>(this Size<IObservable<T>> self) where T : INumeric<T>
		{
			return self.Select(Numeric.Round);
		}

		public static Size<T> Round<T>(this Size<T> self) where T : INumeric<T>
		{
			return self.Select(Numeric.Round);
		}
	}
}
