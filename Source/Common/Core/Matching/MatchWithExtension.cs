using System;

namespace Outracks
{
	public static class MatchWithExtension
	{

		public static T MatchWith<T, T1>(this IMatchTypes<T1> self, Func<T1, T> a1)
            where T1 : class
        {
            var t1 = self as T1; if (t1 != null) return a1(t1);
            throw new ArgumentException();
        }

		public static T MatchWith<T, T1, T2>(this IMatchTypes<T1, T2> self, Func<T1, T> a1, Func<T2, T> a2)
            where T1 : class
            where T2 : class
        {
            var t1 = self as T1; if (t1 != null) return a1(t1);
            var t2 = self as T2; if (t2 != null) return a2(t2);
            throw new ArgumentException();
        }

        public static T MatchWith<T, T1, T2, T3>(this IMatchTypes<T1, T2, T3> self, Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3)
            where T1 : class
            where T2 : class
            where T3 : class
        {
            var t1 = self as T1; if (t1 != null) return a1(t1);
            var t2 = self as T2; if (t2 != null) return a2(t2);
            var t3 = self as T3; if (t3 != null) return a3(t3);
            throw new ArgumentException();
        }

        public static T MatchWith<T, T1, T2, T3, T4>(this IMatchTypes<T1, T2, T3, T4> self, Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
        {
            var t1 = self as T1; if (t1 != null) return a1(t1);
            var t2 = self as T2; if (t2 != null) return a2(t2);
            var t3 = self as T3; if (t3 != null) return a3(t3);
            var t4 = self as T4; if (t4 != null) return a4(t4);
            throw new ArgumentException();
        }

        public static T MatchWith<T, T1, T2, T3, T4, T5>(this IMatchTypes<T1, T2, T3, T4, T5> self, Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4, Func<T5, T> a5)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            var t1 = self as T1; if (t1 != null) return a1(t1);
            var t2 = self as T2; if (t2 != null) return a2(t2);
            var t3 = self as T3; if (t3 != null) return a3(t3);
            var t4 = self as T4; if (t4 != null) return a4(t4);
            var t5 = self as T5; if (t5 != null) return a5(t5);
            throw new ArgumentException();
        }

        public static T MatchWith<T, T1, T2, T3, T4, T5, T6>(this IMatchTypes<T1, T2, T3, T4, T5, T6> self, Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4, Func<T5, T> a5, Func<T6, T> a6)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            var t1 = self as T1; if (t1 != null) return a1(t1);
            var t2 = self as T2; if (t2 != null) return a2(t2);
            var t3 = self as T3; if (t3 != null) return a3(t3);
            var t4 = self as T4; if (t4 != null) return a4(t4);
            var t5 = self as T5; if (t5 != null) return a5(t5);
            var t6 = self as T6; if (t6 != null) return a6(t6);
            throw new ArgumentException();
        }

        public static T MatchWith<T, T1, T2, T3, T4, T5, T6, T7>(this IMatchTypes<T1, T2, T3, T4, T5, T6, T7> self, Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4, Func<T5, T> a5, Func<T6, T> a6, Func<T7, T> a7)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            var t1 = self as T1; if (t1 != null) return a1(t1);
            var t2 = self as T2; if (t2 != null) return a2(t2);
            var t3 = self as T3; if (t3 != null) return a3(t3);
            var t4 = self as T4; if (t4 != null) return a4(t4);
            var t5 = self as T5; if (t5 != null) return a5(t5);
            var t6 = self as T6; if (t6 != null) return a6(t6);
            var t7 = self as T7; if (t7 != null) return a7(t7);
            throw new ArgumentException();
        }

        public static T MatchWith<T, T1, T2, T3, T4, T5, T6, T7, T8>(this IMatchTypes<T1, T2, T3, T4, T5, T6, T7, T8> self, Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4, Func<T5, T> a5, Func<T6, T> a6, Func<T7, T> a7, Func<T8, T> a8)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
        {
            var t1 = self as T1; if (t1 != null) return a1(t1);
            var t2 = self as T2; if (t2 != null) return a2(t2);
            var t3 = self as T3; if (t3 != null) return a3(t3);
            var t4 = self as T4; if (t4 != null) return a4(t4);
            var t5 = self as T5; if (t5 != null) return a5(t5);
            var t6 = self as T6; if (t6 != null) return a6(t6);
            var t7 = self as T7; if (t7 != null) return a7(t7);
            var t8 = self as T8; if (t8 != null) return a8(t8);
            throw new ArgumentException();
        }

		public static T MatchWith<T, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
			this IMatchTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9> self,
			Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4, Func<T5, T> a5, Func<T6, T> a6, Func<T7, T> a7, Func<T8, T> a8, Func<T9, T> a9)
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
			var t1 = self as T1; if (t1 != null) return a1(t1);
			var t2 = self as T2; if (t2 != null) return a2(t2);
			var t3 = self as T3; if (t3 != null) return a3(t3);
			var t4 = self as T4; if (t4 != null) return a4(t4);
			var t5 = self as T5; if (t5 != null) return a5(t5);
			var t6 = self as T6; if (t6 != null) return a6(t6);
			var t7 = self as T7; if (t7 != null) return a7(t7);
			var t8 = self as T8; if (t8 != null) return a8(t8);
			var t9 = self as T9; if (t9 != null) return a9(t9);
			throw new ArgumentException();
		}

		public static T MatchWith<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
			this IMatchTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> self,
			Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4, Func<T5, T> a5, Func<T6, T> a6, Func<T7, T> a7, Func<T8, T> a8, Func<T9, T> a9, Func<T10, T> a10)
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
			var t1 = self as T1; if (t1 != null) return a1(t1);
			var t2 = self as T2; if (t2 != null) return a2(t2);
			var t3 = self as T3; if (t3 != null) return a3(t3);
			var t4 = self as T4; if (t4 != null) return a4(t4);
			var t5 = self as T5; if (t5 != null) return a5(t5);
			var t6 = self as T6; if (t6 != null) return a6(t6);
			var t7 = self as T7; if (t7 != null) return a7(t7);
			var t8 = self as T8; if (t8 != null) return a8(t8);
			var t9 = self as T9; if (t9 != null) return a9(t9);
			var t10 = self as T10; if (t10 != null) return a10(t10);
			throw new ArgumentException();
		}

		public static T MatchWith<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
			this IMatchTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> self,
			Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4, Func<T5, T> a5, Func<T6, T> a6, Func<T7, T> a7, Func<T8, T> a8, Func<T9, T> a9, Func<T10, T> a10, Func<T11, T> a11)
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
		{
			var t1 = self as T1; if (t1 != null) return a1(t1);
			var t2 = self as T2; if (t2 != null) return a2(t2);
			var t3 = self as T3; if (t3 != null) return a3(t3);
			var t4 = self as T4; if (t4 != null) return a4(t4);
			var t5 = self as T5; if (t5 != null) return a5(t5);
			var t6 = self as T6; if (t6 != null) return a6(t6);
			var t7 = self as T7; if (t7 != null) return a7(t7);
			var t8 = self as T8; if (t8 != null) return a8(t8);
			var t9 = self as T9; if (t9 != null) return a9(t9);
			var t10 = self as T10; if (t10 != null) return a10(t10);
			var t11 = self as T11; if (t11 != null) return a11(t11);
			throw new ArgumentException();
		}

		public static T MatchWith<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
			this IMatchTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> self,
			Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4, Func<T5, T> a5, Func<T6, T> a6, Func<T7, T> a7, Func<T8, T> a8, Func<T9, T> a9, Func<T10, T> a10, Func<T11, T> a11, Func<T12, T> a12)
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
			var t1 = self as T1; if (t1 != null) return a1(t1);
			var t2 = self as T2; if (t2 != null) return a2(t2);
			var t3 = self as T3; if (t3 != null) return a3(t3);
			var t4 = self as T4; if (t4 != null) return a4(t4);
			var t5 = self as T5; if (t5 != null) return a5(t5);
			var t6 = self as T6; if (t6 != null) return a6(t6);
			var t7 = self as T7; if (t7 != null) return a7(t7);
			var t8 = self as T8; if (t8 != null) return a8(t8);
			var t9 = self as T9; if (t9 != null) return a9(t9);
			var t10 = self as T10; if (t10 != null) return a10(t10);
			var t11 = self as T11; if (t11 != null) return a11(t11);
			var t12 = self as T12; if (t12 != null) return a12(t12);
			throw new ArgumentException();
		}

		public static T MatchWith<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
			this IMatchTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> self,
			Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4, Func<T5, T> a5, Func<T6, T> a6, Func<T7, T> a7, Func<T8, T> a8, Func<T9, T> a9, Func<T10, T> a10, Func<T11, T> a11, Func<T12, T> a12, Func<T13, T> a13)
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
			var t1 = self as T1; if (t1 != null) return a1(t1);
			var t2 = self as T2; if (t2 != null) return a2(t2);
			var t3 = self as T3; if (t3 != null) return a3(t3);
			var t4 = self as T4; if (t4 != null) return a4(t4);
			var t5 = self as T5; if (t5 != null) return a5(t5);
			var t6 = self as T6; if (t6 != null) return a6(t6);
			var t7 = self as T7; if (t7 != null) return a7(t7);
			var t8 = self as T8; if (t8 != null) return a8(t8);
			var t9 = self as T9; if (t9 != null) return a9(t9);
			var t10 = self as T10; if (t10 != null) return a10(t10);
			var t11 = self as T11; if (t11 != null) return a11(t11);
			var t12 = self as T12; if (t12 != null) return a12(t12);
			var t13 = self as T13; if (t13 != null) return a13(t13);
			throw new ArgumentException();
		}
		public static T MatchWith<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
		this IMatchTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> self,
		Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4, Func<T5, T> a5, Func<T6, T> a6, Func<T7, T> a7, Func<T8, T> a8, Func<T9, T> a9, Func<T10, T> a10, Func<T11, T> a11, Func<T12, T> a12, Func<T13, T> a13, Func<T14, T> a14)
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
			var t1 = self as T1; if (t1 != null) return a1(t1);
			var t2 = self as T2; if (t2 != null) return a2(t2);
			var t3 = self as T3; if (t3 != null) return a3(t3);
			var t4 = self as T4; if (t4 != null) return a4(t4);
			var t5 = self as T5; if (t5 != null) return a5(t5);
			var t6 = self as T6; if (t6 != null) return a6(t6);
			var t7 = self as T7; if (t7 != null) return a7(t7);
			var t8 = self as T8; if (t8 != null) return a8(t8);
			var t9 = self as T9; if (t9 != null) return a9(t9);
			var t10 = self as T10; if (t10 != null) return a10(t10);
			var t11 = self as T11; if (t11 != null) return a11(t11);
			var t12 = self as T12; if (t12 != null) return a12(t12);
			var t13 = self as T13; if (t13 != null) return a13(t13);
			var t14 = self as T14; if (t14 != null) return a14(t14);
			throw new ArgumentException();
		}


		public static T MatchWith<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
		this IMatchTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> self,
		Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4, Func<T5, T> a5, Func<T6, T> a6, Func<T7, T> a7, Func<T8, T> a8, Func<T9, T> a9, Func<T10, T> a10, Func<T11, T> a11, Func<T12, T> a12, Func<T13, T> a13, Func<T14, T> a14, Func<T15, T> a15)
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
			var t1 = self as T1; if (t1 != null) return a1(t1);
			var t2 = self as T2; if (t2 != null) return a2(t2);
			var t3 = self as T3; if (t3 != null) return a3(t3);
			var t4 = self as T4; if (t4 != null) return a4(t4);
			var t5 = self as T5; if (t5 != null) return a5(t5);
			var t6 = self as T6; if (t6 != null) return a6(t6);
			var t7 = self as T7; if (t7 != null) return a7(t7);
			var t8 = self as T8; if (t8 != null) return a8(t8);
			var t9 = self as T9; if (t9 != null) return a9(t9);
			var t10 = self as T10; if (t10 != null) return a10(t10);
			var t11 = self as T11; if (t11 != null) return a11(t11);
			var t12 = self as T12; if (t12 != null) return a12(t12);
			var t13 = self as T13; if (t13 != null) return a13(t13);
			var t14 = self as T14; if (t14 != null) return a14(t14);
			var t15 = self as T15; if (t15 != null) return a15(t15);
			throw new ArgumentException();
		}

		public static T MatchWith<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
			this IMatchTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> self,
			Func<T1, T> a1, Func<T2, T> a2, Func<T3, T> a3, Func<T4, T> a4, Func<T5, T> a5, Func<T6, T> a6, Func<T7, T> a7, Func<T8, T> a8, Func<T9, T> a9, Func<T10, T> a10, Func<T11, T> a11, Func<T12, T> a12, Func<T13, T> a13, Func<T14, T> a14, Func<T15, T> a15, Func<T16, T> a16)
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
			var t1 = self as T1; if (t1 != null) return a1(t1);
			var t2 = self as T2; if (t2 != null) return a2(t2);
			var t3 = self as T3; if (t3 != null) return a3(t3);
			var t4 = self as T4; if (t4 != null) return a4(t4);
			var t5 = self as T5; if (t5 != null) return a5(t5);
			var t6 = self as T6; if (t6 != null) return a6(t6);
			var t7 = self as T7; if (t7 != null) return a7(t7);
			var t8 = self as T8; if (t8 != null) return a8(t8);
			var t9 = self as T9; if (t9 != null) return a9(t9);
			var t10 = self as T10; if (t10 != null) return a10(t10);
			var t11 = self as T11; if (t11 != null) return a11(t11);
			var t12 = self as T12; if (t12 != null) return a12(t12);
			var t13 = self as T13; if (t13 != null) return a13(t13);
			var t14 = self as T14; if (t14 != null) return a14(t14);
			var t15 = self as T15; if (t15 != null) return a15(t15);
			var t16 = self as T16; if (t16 != null) return a16(t16);
			throw new ArgumentException();
		}
    }

    

}
