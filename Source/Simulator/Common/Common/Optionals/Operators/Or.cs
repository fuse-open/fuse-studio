using Uno;
using Uno.Collections;

namespace Outracks
{
	public static partial class Optional
	{


		public static T Or<T>(this Optional<T> self, T fallback) 
		{
			return self.HasValue
				? self.Value
				: fallback;
		}

		public static Optional<T> Or<T>(this Optional<T> self, Optional<T> fallback)
		{
			return self.HasValue
				? self
				: fallback;
		}

		public static T Or<T>(this Optional<T> self, Func<T> fallback) 
		{
			return self.HasValue
				? self.Value
				: fallback();
		}

		public static Optional<T> Or<T>(this Optional<T> self, Func<Optional<T>> fallback) 
		{
			return self.HasValue
				? Some(self.Value)
				: fallback();
		}


		public static T OrDefault<T>(this Optional<T> self)
		{
			return self.Or(default(T));
		}

		public static IEnumerable<T> OrEmpty<T>(this Optional<IEnumerable<T>> self)
		{
			return self.HasValue
				? self.Value
				: new T[0];
		}


		public static T OrThrow<T>(this Optional<T> self)
		{
			return self.Value;
		}

		public static T OrThrow<T>(this Optional<T> self, Exception e)
		{
			if (!self.HasValue)
				throw e;
			return self.Value;
		}

	}
}