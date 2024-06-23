using Uno;
using Uno.Collections;

namespace Outracks
{
	public static partial class Optional
	{
		public static Optional<TValue> TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
		{
			TValue value;
			if (dict.TryGetValue(key, out value))
				return Optional.Some<TValue>(value);

			return Optional.None<TValue>();
		}
	}
}
