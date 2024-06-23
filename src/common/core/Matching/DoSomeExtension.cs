using System;

namespace Outracks
{
	public static class DoSomeExtension
	{
		public static void DoSome<TArg, T1, T2>(
			this TArg t,
			Action<T1> a1,
			Action<T2> a2)
			where T1 : TArg
			where T2 : TArg
		{
			if (t is T1) a1((T1)t);
			if (t is T2) a2((T2)t);
		}
		public static void DoSome<TArg, T1, T2, T3>(
			this TArg t,
			Action<T1> a1,
			Action<T2> a2,
			Action<T3> a3)
			where T1 : TArg
			where T2 : TArg
			where T3 : TArg
		{
			if (t is T1) a1((T1)t);
			if (t is T2) a2((T2)t);
			if (t is T3) a3((T3)t);

		}
	}
}