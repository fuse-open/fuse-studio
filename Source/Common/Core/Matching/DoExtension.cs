using System;

namespace Outracks
{
	public static class DoExtension
	{
		public static void Do<T1>(this IMatchTypes<T1> self, Action<T1> a1)
			where T1 : class
		{
			var t1 = self as T1; if (t1 != null) a1(t1);
		}

		public static void Do<T1, T2>(this IMatchTypes<T1, T2> self, Action<T1> a1, Action<T2> a2)
			where T1 : class
			where T2 : class
		{
			var t1 = self as T1; if (t1 != null) a1(t1);
			var t2 = self as T2; if (t2 != null) a2(t2);
		}

		public static void Do<T1, T2, T3>(this IMatchTypes<T1, T2, T3> self, Action<T1> a1, Action<T2> a2, Action<T3> a3)
			where T1 : class
			where T2 : class
			where T3 : class
		{
			var t1 = self as T1; if (t1 != null) a1(t1);
			var t2 = self as T2; if (t2 != null) a2(t2);
			var t3 = self as T3; if (t3 != null) a3(t3);
		}

		public static void Do<T1, T2, T3, T4>(this IMatchTypes<T1, T2, T3, T4> self, Action<T1> a1, Action<T2> a2, Action<T3> a3, Action<T4> a4)
			where T1 : class
			where T2 : class
			where T3 : class
			where T4 : class
		{
			var t1 = self as T1; if (t1 != null) a1(t1);
			var t2 = self as T2; if (t2 != null) a2(t2);
			var t3 = self as T3; if (t3 != null) a3(t3);
			var t4 = self as T4; if (t4 != null) a4(t4);
		}
		public static void Do<T1, T2, T3, T4, T5>(this IMatchTypes<T1, T2, T3, T4, T5> self, Action<T1> a1, Action<T2> a2, Action<T3> a3, Action<T4> a4, Action<T5> a5)
			where T1 : class
			where T2 : class
			where T3 : class
			where T4 : class
			where T5 : class
		{
			var t1 = self as T1; if (t1 != null) a1(t1);
			var t2 = self as T2; if (t2 != null) a2(t2);
			var t3 = self as T3; if (t3 != null) a3(t3);
			var t4 = self as T4; if (t4 != null) a4(t4);
			var t5 = self as T5; if (t5 != null) a5(t5);
		}

		public static void Do<T1, T2, T3, T4, T5, T6>(this IMatchTypes<T1, T2, T3, T4, T5, T6> self, Action<T1> a1, Action<T2> a2, Action<T3> a3, Action<T4> a4, Action<T5> a5, Action<T6> a6)
			where T1 : class
			where T2 : class
			where T3 : class
			where T4 : class
			where T5 : class
			where T6 : class
		{
			var t1 = self as T1; if (t1 != null) a1(t1);
			var t2 = self as T2; if (t2 != null) a2(t2);
			var t3 = self as T3; if (t3 != null) a3(t3);
			var t4 = self as T4; if (t4 != null) a4(t4);
			var t5 = self as T5; if (t5 != null) a5(t5);
			var t6 = self as T6; if (t6 != null) a6(t6);
		}

		public static void Do<T1, T2, T3, T4, T5, T6, T7>(this IMatchTypes<T1, T2, T3, T4, T5, T6, T7> self, Action<T1> a1, Action<T2> a2, Action<T3> a3, Action<T4> a4, Action<T5> a5, Action<T6> a6, Action<T7> a7)
			where T1 : class
			where T2 : class
			where T3 : class
			where T4 : class
			where T5 : class
			where T6 : class
			where T7 : class
		{
			var t1 = self as T1; if (t1 != null) a1(t1);
			var t2 = self as T2; if (t2 != null) a2(t2);
			var t3 = self as T3; if (t3 != null) a3(t3);
			var t4 = self as T4; if (t4 != null) a4(t4);
			var t5 = self as T5; if (t5 != null) a5(t5);
			var t6 = self as T6; if (t6 != null) a6(t6);
			var t7 = self as T7; if (t7 != null) a7(t7);
		}
		public static void Do<T1, T2, T3, T4, T5, T6, T7, T8>(this IMatchTypes<T1, T2, T3, T4, T5, T6, T7, T8> self, Action<T1> a1, Action<T2> a2, Action<T3> a3, Action<T4> a4, Action<T5> a5, Action<T6> a6, Action<T7> a7, Action<T8> a8)
			where T1 : class
			where T2 : class
			where T3 : class
			where T4 : class
			where T5 : class
			where T6 : class
			where T7 : class
			where T8 : class
		{
			var t1 = self as T1; if (t1 != null) a1(t1);
			var t2 = self as T2; if (t2 != null) a2(t2);
			var t3 = self as T3; if (t3 != null) a3(t3);
			var t4 = self as T4; if (t4 != null) a4(t4);
			var t5 = self as T5; if (t5 != null) a5(t5);
			var t6 = self as T6; if (t6 != null) a6(t6);
			var t7 = self as T7; if (t7 != null) a7(t7);
			var t8 = self as T8; if (t8 != null) a8(t8);
		}
		public static void Do<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IMatchTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9> self, Action<T1> a1, Action<T2> a2, Action<T3> a3, Action<T4> a4, Action<T5> a5, Action<T6> a6, Action<T7> a7, Action<T8> a8, Action<T9> a9)
			where T1 : class
			where T2 : class
			where T3 : class
			where T4 : class
			where T5 : class
			where T6 : class
			where T7 : class
			where T8 : class
			where T9 : class
		{
			var t1 = self as T1; if (t1 != null) a1(t1);
			var t2 = self as T2; if (t2 != null) a2(t2);
			var t3 = self as T3; if (t3 != null) a3(t3);
			var t4 = self as T4; if (t4 != null) a4(t4);
			var t5 = self as T5; if (t5 != null) a5(t5);
			var t6 = self as T6; if (t6 != null) a6(t6);
			var t7 = self as T7; if (t7 != null) a7(t7);
			var t8 = self as T8; if (t8 != null) a8(t8); 
			var t9 = self as T9; if (t9 != null) a9(t9);
		}
		public static void Do<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this IMatchTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self, Action<T1> a1, Action<T2> a2, Action<T3> a3, Action<T4> a4, Action<T5> a5, Action<T6> a6, Action<T7> a7, Action<T8> a8, Action<T9> a9, Action<T10> a10)
			where T1 : class
			where T2 : class
			where T3 : class
			where T4 : class
			where T5 : class
			where T6 : class
			where T7 : class
			where T8 : class
			where T9 : class
			where T10 : class
		{
			var t1 = self as T1; if (t1 != null) a1(t1);
			var t2 = self as T2; if (t2 != null) a2(t2);
			var t3 = self as T3; if (t3 != null) a3(t3);
			var t4 = self as T4; if (t4 != null) a4(t4);
			var t5 = self as T5; if (t5 != null) a5(t5);
			var t6 = self as T6; if (t6 != null) a6(t6);
			var t7 = self as T7; if (t7 != null) a7(t7);
			var t8 = self as T8; if (t8 != null) a8(t8);
			var t9 = self as T9; if (t9 != null) a9(t9);
			var t10 = self as T10; if (t10 != null) a10(t10);
		}

		public static void Do<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this IMatchTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self, Action<T1> a1, Action<T2> a2, Action<T3> a3, Action<T4> a4, Action<T5> a5, Action<T6> a6, Action<T7> a7, Action<T8> a8, Action<T9> a9, Action<T10> a10, Action<T11> a11, Action<T12> a12)
			where T1 : class
			where T2 : class
			where T3 : class
			where T4 : class
			where T5 : class
			where T6 : class
			where T7 : class
			where T8 : class
			where T9 : class
			where T10 : class
			where T11 : class
			where T12 : class
		{
			var t1 = self as T1; if (t1 != null) a1(t1);
			var t2 = self as T2; if (t2 != null) a2(t2);
			var t3 = self as T3; if (t3 != null) a3(t3);
			var t4 = self as T4; if (t4 != null) a4(t4);
			var t5 = self as T5; if (t5 != null) a5(t5);
			var t6 = self as T6; if (t6 != null) a6(t6);
			var t7 = self as T7; if (t7 != null) a7(t7);
			var t8 = self as T8; if (t8 != null) a8(t8);
			var t9 = self as T9; if (t9 != null) a9(t9);
			var t10 = self as T10; if (t10 != null) a10(t10);
			var t11 = self as T11; if (t11 != null) a11(t11);
			var t12 = self as T12; if (t12 != null) a12(t12);
		}
		public static void Do<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this IMatchTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self, Action<T1> a1, Action<T2> a2, Action<T3> a3, Action<T4> a4, Action<T5> a5, Action<T6> a6, Action<T7> a7, Action<T8> a8, Action<T9> a9, Action<T10> a10, Action<T11> a11, Action<T12> a12, Action<T13> a13)
			where T1 : class
			where T2 : class
			where T3 : class
			where T4 : class
			where T5 : class
			where T6 : class
			where T7 : class
			where T8 : class
			where T9 : class
			where T10 : class
			where T11 : class
			where T12 : class
			where T13 : class
		{
			var t1 = self as T1; if (t1 != null) a1(t1);
			var t2 = self as T2; if (t2 != null) a2(t2);
			var t3 = self as T3; if (t3 != null) a3(t3);
			var t4 = self as T4; if (t4 != null) a4(t4);
			var t5 = self as T5; if (t5 != null) a5(t5);
			var t6 = self as T6; if (t6 != null) a6(t6);
			var t7 = self as T7; if (t7 != null) a7(t7);
			var t8 = self as T8; if (t8 != null) a8(t8);
			var t9 = self as T9; if (t9 != null) a9(t9);
			var t10 = self as T10; if (t10 != null) a10(t10);
			var t11 = self as T11; if (t11 != null) a11(t11);
			var t12 = self as T12; if (t12 != null) a12(t12);
			var t13 = self as T13; if (t13 != null) a13(t13);
		}
		public static void Do<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this IMatchTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self, Action<T1> a1, Action<T2> a2, Action<T3> a3, Action<T4> a4, Action<T5> a5, Action<T6> a6, Action<T7> a7, Action<T8> a8, Action<T9> a9, Action<T10> a10, Action<T11> a11, Action<T12> a12, Action<T13> a13, Action<T14> a14)
			where T1 : class
			where T2 : class
			where T3 : class
			where T4 : class
			where T5 : class
			where T6 : class
			where T7 : class
			where T8 : class
			where T9 : class
			where T10 : class
			where T11 : class
			where T12 : class
			where T13 : class
			where T14 : class
		{
			var t1 = self as T1; if (t1 != null) a1(t1);
			var t2 = self as T2; if (t2 != null) a2(t2);
			var t3 = self as T3; if (t3 != null) a3(t3);
			var t4 = self as T4; if (t4 != null) a4(t4);
			var t5 = self as T5; if (t5 != null) a5(t5);
			var t6 = self as T6; if (t6 != null) a6(t6);
			var t7 = self as T7; if (t7 != null) a7(t7);
			var t8 = self as T8; if (t8 != null) a8(t8);
			var t9 = self as T9; if (t9 != null) a9(t9);
			var t10 = self as T10; if (t10 != null) a10(t10);
			var t11 = self as T11; if (t11 != null) a11(t11);
			var t12 = self as T12; if (t12 != null) a12(t12);
			var t13 = self as T13; if (t13 != null) a13(t13);
			var t14 = self as T14; if (t14 != null) a14(t14);
		}
		public static void Do<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this IMatchTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self, Action<T1> a1, Action<T2> a2, Action<T3> a3, Action<T4> a4, Action<T5> a5, Action<T6> a6, Action<T7> a7, Action<T8> a8, Action<T9> a9, Action<T10> a10, Action<T11> a11, Action<T12> a12, Action<T13> a13, Action<T14> a14, Action<T15> a15)
			where T1 : class
			where T2 : class
			where T3 : class
			where T4 : class
			where T5 : class
			where T6 : class
			where T7 : class
			where T8 : class
			where T9 : class
			where T10 : class
			where T11 : class
			where T12 : class
			where T13 : class
			where T14 : class
			where T15 : class
		{
			var t1 = self as T1; if (t1 != null) a1(t1);
			var t2 = self as T2; if (t2 != null) a2(t2);
			var t3 = self as T3; if (t3 != null) a3(t3);
			var t4 = self as T4; if (t4 != null) a4(t4);
			var t5 = self as T5; if (t5 != null) a5(t5);
			var t6 = self as T6; if (t6 != null) a6(t6);
			var t7 = self as T7; if (t7 != null) a7(t7);
			var t8 = self as T8; if (t8 != null) a8(t8);
			var t9 = self as T9; if (t9 != null) a9(t9);
			var t10 = self as T10; if (t10 != null) a10(t10);
			var t11 = self as T11; if (t11 != null) a11(t11);
			var t12 = self as T12; if (t12 != null) a12(t12);
			var t13 = self as T13; if (t13 != null) a13(t13);
			var t14 = self as T14; if (t14 != null) a14(t14);
			var t15 = self as T15; if (t15 != null) a15(t15);
		}
		public static void Do<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this IMatchTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> self, Action<T1> a1, Action<T2> a2, Action<T3> a3, Action<T4> a4, Action<T5> a5, Action<T6> a6, Action<T7> a7, Action<T8> a8, Action<T9> a9, Action<T10> a10, Action<T11> a11, Action<T12> a12, Action<T13> a13, Action<T14> a14, Action<T15> a15, Action<T16> a16)
			where T1 : class
			where T2 : class
			where T3 : class
			where T4 : class
			where T5 : class
			where T6 : class
			where T7 : class
			where T8 : class
			where T9 : class
			where T10 : class
			where T11 : class
			where T12 : class
			where T13 : class
			where T14 : class
			where T15 : class
			where T16 : class
		{
			var t1 = self as T1; if (t1 != null) a1(t1);
			var t2 = self as T2; if (t2 != null) a2(t2);
			var t3 = self as T3; if (t3 != null) a3(t3);
			var t4 = self as T4; if (t4 != null) a4(t4);
			var t5 = self as T5; if (t5 != null) a5(t5);
			var t6 = self as T6; if (t6 != null) a6(t6);
			var t7 = self as T7; if (t7 != null) a7(t7);
			var t8 = self as T8; if (t8 != null) a8(t8);
			var t9 = self as T9; if (t9 != null) a9(t9);
			var t10 = self as T10; if (t10 != null) a10(t10);
			var t11 = self as T11; if (t11 != null) a11(t11);
			var t12 = self as T12; if (t12 != null) a12(t12);
			var t13 = self as T13; if (t13 != null) a13(t13);
			var t14 = self as T14; if (t14 != null) a14(t14);
			var t15 = self as T15; if (t15 != null) a15(t15);
			var t16 = self as T16; if (t16 != null) a16(t16);
		}
	}
}