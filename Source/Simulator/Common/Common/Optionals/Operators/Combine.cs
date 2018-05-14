using System;
using Uno;

namespace Outracks
{

	public static partial class Optional
	{
		public static Optional<T5> Combine<T1, T2, T3, T4, T5>(Optional<T1> o1, Optional<T2> o2, Optional<T3> o3, Optional<T4> o4, Func<T1,T2,T3,T4,T5> combine)
		{
			if (o1.HasValue && o2.HasValue && o3.HasValue && o4.HasValue)
				return combine(o1.Value, o2.Value, o3.Value, o4.Value);

			return None();
		}

		public static Optional<T3> Combine<T1, T2, T3>(Optional<T1> o1, Optional<T2> o2, Func<T1, T2, T3> o3)
		{
			if (o1.HasValue && o2.HasValue)
				return o3(o1.Value, o2.Value);

			return None();
		}

		public static Optional<Tuple<T1, T2, T3>> Combine<T1, T2, T3>(Optional<T1> o1, Optional<T2> o2, Optional<T3> o3)
		{
			if (o1.HasValue && o2.HasValue && o3.HasValue)
				return Tuple.Create(o1.Value, o2.Value, o3.Value);

			return None();
		}
		public static Optional<Tuple<T1, T2>> Combine<T1, T2>(Optional<T1> o1, Optional<T2> o2)
		{
			if (o1.HasValue && o2.HasValue)
				return Tuple.Create(o1.Value, o2.Value);

			return None();
		}
	}
}