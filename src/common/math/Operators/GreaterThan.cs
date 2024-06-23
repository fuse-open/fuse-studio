using System;

namespace Outracks
{
	public static partial class Comparable
	{
		public static bool GreaterThanOrEquals<T>(this T value, T other) where T : IComparable<T>
		{
			return value.CompareTo(other) >= 0;
		}

		public static bool GreaterThan<T>(this T value, T other) where T : IComparable<T>
		{
			return value.CompareTo(other) > 0;
		}
	}
}