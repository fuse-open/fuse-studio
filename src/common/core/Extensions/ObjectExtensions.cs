using System;

namespace Outracks
{
	public static class ObjectExtensions
	{
		public static void ThrowIfNull(this object self, string argument)
		{
			if (self == null) throw new ArgumentNullException(argument);
		}

		public static T ThrowIfNull<T>(this T self, Exception e) where T : class
		{
			if (self == null) throw e;
			return self;
		}

		public static void DoIfNotNull<T>(this T t, Action<T> action) where T : class
		{
			if (ReferenceEquals(t, null)) return;
			action(t);
		}
	}
}