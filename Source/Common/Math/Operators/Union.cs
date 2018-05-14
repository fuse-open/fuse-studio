using System;

namespace Outracks
{
	public static partial class Rectangle
	{
		public static Rectangle<T> Union<T>(this Rectangle<T> self, Rectangle<T> other)
			where T : INumeric<T>
		{
			return FromSides(
				left: self.Left().Min(other.Left()),
				top: self.Top().Min(other.Top()),
				right: self.Right().Max(other.Right()),
				bottom: self.Bottom().Max(other.Bottom()));
		}
	}

}