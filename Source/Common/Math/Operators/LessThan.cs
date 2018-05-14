using System;

namespace Outracks
{
	public static partial class Comparable
	{
		public static bool LessThanOrEquals<T>(this T value, T other) where T : IComparable<T>
		{
			return value.CompareTo(other) <= 0;
		}

		public static bool LessThan<T>(this T value, T other) where T : IComparable<T>
		{
			return value.CompareTo(other) < 0;
		}
	}
}