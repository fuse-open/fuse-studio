using System;
using System.Collections.Generic;

namespace Outracks
{
	public static class EqualityComparer
	{
		public static IEqualityComparer<T> CreateDefault<T>(Func<T, object> getIdentity)
		{
			return Create<T>((a, b) => getIdentity(a).Equals(getIdentity(b)), a => getIdentity(a).GetHashCode());
		}

		public static IEqualityComparer<T> Create<T>(Func<T, T, bool> equals, Func<T, int> getHashCode)
		{
			return new AnonymousEqualityComparer<T>(equals, getHashCode);
		}

	}

	class AnonymousEqualityComparer<T> : IEqualityComparer<T>
	{
		readonly Func<T, T, bool> _equals;
		readonly Func<T, int> _getHashCode;
		public AnonymousEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
		{
			_equals = equals;
			_getHashCode = getHashCode;
		}

		public bool Equals(T x, T y)
		{
			return _equals(x, y);
		}

		public int GetHashCode(T obj)
		{
			return _getHashCode(obj);
		}
	}
}