using System;

namespace Outracks
{
	public static class MatchSomeExtension
	{
		public static Optional<T> MatchSome<TArg, T, T1>(
			this TArg t,
			Func<T1, T> a1)
			where T1 : TArg
		{
			if (t is T1) return a1((T1)t);
			return Optional.None();
		}
		public static Optional<T> MatchSome<TArg, T, T1, T2>(
			this TArg t,
			Func<T1, T> a1, Func<T2, T> a2)
			where T1 : TArg
			where T2 : TArg
		{
			if (t is T1) return a1((T1)t);
			if (t is T2) return a2((T2)t);
			return Optional.None();
		}
		public static Optional<T> MatchSome<TArg, T, T1, T2, T3>(
			this TArg t,
			Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3)
			where T1 : TArg
			where T2 : TArg
			where T3 : TArg
		{
			if (t is T1) return a1((T1)t);
			if (t is T2) return a2((T2)t);
			if (t is T3) return a3((T3)t);
			return Optional.None();
		}
		public static Optional<T> MatchSome<TArg, T, T1, T2, T3, T4>(
			this TArg t,
			Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4)
			where T1 : TArg
			where T2 : TArg
			where T3 : TArg
			where T4 : TArg
		{
			if (t is T1) return a1((T1)t);
			if (t is T2) return a2((T2)t);
			if (t is T3) return a3((T3)t);
			if (t is T4) return a4((T4)t);
			return Optional.None();
		}
		public static Optional<T> MatchSome<TArg, T, T1, T2, T3, T4, T5>(
			this TArg t,
			Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4, Func<T5, T> a5)
			where T1 : TArg
			where T2 : TArg
			where T3 : TArg
			where T4 : TArg
			where T5 : TArg
		{
			if (t is T1) return a1((T1)t);
			if (t is T2) return a2((T2)t);
			if (t is T3) return a3((T3)t);
			if (t is T4) return a4((T4)t);
			if (t is T5) return a5((T5)t);
			return Optional.None();
		}
		public static Optional<T> MatchSome<TArg, T, T1, T2, T3, T4, T5, T6>(
			this TArg t,
			Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4, Func<T5, T> a5, Func<T6, T> a6)
			where T1 : TArg
			where T2 : TArg
			where T3 : TArg
			where T4 : TArg
			where T5 : TArg
			where T6 : TArg
		{
			if (t is T1) return a1((T1)t);
			if (t is T2) return a2((T2)t);
			if (t is T3) return a3((T3)t);
			if (t is T4) return a4((T4)t);
			if (t is T5) return a5((T5)t);
			if (t is T6) return a6((T6)t);
			return Optional.None();
		}
		public static Optional<T> MatchSome<TArg, T, T1, T2, T3, T4, T5, T6, T7>(
			this TArg t,
			Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4, Func<T5, T> a5, Func<T6, T> a6, Func<T7, T> a7)
			where T1 : TArg
			where T2 : TArg
			where T3 : TArg
			where T4 : TArg
			where T5 : TArg
			where T6 : TArg
			where T7 : TArg
		{
			if (t is T1) return a1((T1)t);
			if (t is T2) return a2((T2)t);
			if (t is T3) return a3((T3)t);
			if (t is T4) return a4((T4)t);
			if (t is T5) return a5((T5)t);
			if (t is T6) return a6((T6)t);
			if (t is T7) return a7((T7)t);
			return Optional.None();
		}
		public static Optional<T> MatchSome<TArg, T, T1, T2, T3, T4, T5, T6, T7, T8>(
			this TArg t,
			Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4, Func<T5, T> a5, Func<T6, T> a6, Func<T7, T> a7, Func<T8, T> a8)
			where T1 : TArg
			where T2 : TArg
			where T3 : TArg
			where T4 : TArg
			where T5 : TArg
			where T6 : TArg
			where T7 : TArg
			where T8 : TArg
		{
			if (t is T1) return a1((T1)t);
			if (t is T2) return a2((T2)t);
			if (t is T3) return a3((T3)t);
			if (t is T4) return a4((T4)t);
			if (t is T5) return a5((T5)t);
			if (t is T6) return a6((T6)t);
			if (t is T7) return a7((T7)t);
			if (t is T8) return a8((T8)t);
			return Optional.None();
		}
		public static Optional<T> MatchSome<TArg, T, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
			this TArg t,
			Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4, Func<T5, T> a5, Func<T6, T> a6, Func<T7, T> a7, Func<T8, T> a8, Func<T9, T> a9)
			where T1 : TArg
			where T2 : TArg
			where T3 : TArg
			where T4 : TArg
			where T5 : TArg
			where T6 : TArg
			where T7 : TArg
			where T8 : TArg
			where T9 : TArg
		{
			if (t is T1) return a1((T1)t);
			if (t is T2) return a2((T2)t);
			if (t is T3) return a3((T3)t);
			if (t is T4) return a4((T4)t);
			if (t is T5) return a5((T5)t);
			if (t is T6) return a6((T6)t);
			if (t is T7) return a7((T7)t);
			if (t is T8) return a8((T8)t);
			if (t is T9) return a9((T9)t);
			return Optional.None();
		}
		public static Optional<T> MatchSome<TArg, T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
			this TArg t,
			Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4, Func<T5, T> a5, Func<T6, T> a6, Func<T7, T> a7, Func<T8, T> a8, Func<T9, T> a9, Func<T10, T> a10)
			where T1 : TArg
			where T2 : TArg
			where T3 : TArg
			where T4 : TArg
			where T5 : TArg
			where T6 : TArg
			where T7 : TArg
			where T8 : TArg
			where T9 : TArg
			where T10 : TArg
		{
			if (t is T1) return a1((T1)t);
			if (t is T2) return a2((T2)t);
			if (t is T3) return a3((T3)t);
			if (t is T4) return a4((T4)t);
			if (t is T5) return a5((T5)t);
			if (t is T6) return a6((T6)t);
			if (t is T7) return a7((T7)t);
			if (t is T8) return a8((T8)t);
			if (t is T9) return a9((T9)t);
			if (t is T10) return a10((T10)t);
			return Optional.None();
		}
		public static Optional<T> MatchSome<TArg, T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
			this TArg t,
			Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4, Func<T5, T> a5, Func<T6, T> a6, Func<T7, T> a7, Func<T8, T> a8, Func<T9, T> a9, Func<T10, T> a10, Func<T11, T> a11)
			where T1 : TArg
			where T2 : TArg
			where T3 : TArg
			where T4 : TArg
			where T5 : TArg
			where T6 : TArg
			where T7 : TArg
			where T8 : TArg
			where T9 : TArg
			where T10 : TArg
			where T11 : TArg
		{
			if (t is T1) return a1((T1)t);
			if (t is T2) return a2((T2)t);
			if (t is T3) return a3((T3)t);
			if (t is T4) return a4((T4)t);
			if (t is T5) return a5((T5)t);
			if (t is T6) return a6((T6)t);
			if (t is T7) return a7((T7)t);
			if (t is T8) return a8((T8)t);
			if (t is T9) return a9((T9)t);
			if (t is T10) return a10((T10)t);
			if (t is T11) return a11((T11)t);
			return Optional.None();
		}
		public static Optional<T> MatchSome<TArg, T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
			this TArg t,
			Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4, Func<T5, T> a5, Func<T6, T> a6, Func<T7, T> a7, Func<T8, T> a8, Func<T9, T> a9, Func<T10, T> a10, Func<T11, T> a11, Func<T12, T> a12)
			where T1 : TArg
			where T2 : TArg
			where T3 : TArg
			where T4 : TArg
			where T5 : TArg
			where T6 : TArg
			where T7 : TArg
			where T8 : TArg
			where T9 : TArg
			where T10 : TArg
			where T11 : TArg
			where T12 : TArg
		{
			if (t is T1) return a1((T1) t);
			if (t is T2) return a2((T2)t);
			if (t is T3) return a3((T3)t);
			if (t is T4) return a4((T4)t);
			if (t is T5) return a5((T5)t);
			if (t is T6) return a6((T6)t);
			if (t is T7) return a7((T7)t);
			if (t is T8) return a8((T8)t);
			if (t is T9) return a9((T9)t);
			if (t is T10) return a10((T10)t);
			if (t is T11) return a11((T11)t);
			if (t is T12) return a12((T12)t);
			return Optional.None();
		}
	}
}