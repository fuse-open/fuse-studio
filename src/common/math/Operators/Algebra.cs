using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static class Algebra<T>
	{
		public static Func<T, T, T> Sub;
		public static Func<T, T, T> Add;
	}

	public static class Algebras
	{
		static bool _initialized = false;

		public static void Initialize()
		{
			if (_initialized) return;

			Algebra<double>.Add = (a, b) => a + b;
			Algebra<double>.Sub = (a, b) => a - b;

			Algebra<int>.Add = (a, b) => a + b;
			Algebra<int>.Sub = (a, b) => a - b;

			Algebra<Points>.Add = (a, b) => a.Add(b);
			Algebra<Points>.Sub = (a, b) => a.Sub(b);

			Algebra<Pixels>.Add = (a, b) => a.Add(b);
			Algebra<Pixels>.Sub = (a, b) => a.Sub(b);

			Algebra<Inches>.Add = (a, b) => a.Add(b);
			Algebra<Inches>.Sub = (a, b) => a.Sub(b);

			Algebra<IObservable<Points>>.Add = (a, b) => a.CombineLatest(b, (a2, b2) => a2.Add(b2));
			Algebra<IObservable<Points>>.Sub = (a, b) => a.CombineLatest(b, (a2, b2) => a2.Sub(b2));

			Algebra<ClipSpaceUnits>.Add = (a, b) => a.Add(b);
			Algebra<ClipSpaceUnits>.Sub = (a, b) => a.Sub(b);

			_initialized = true;
		}
	}
}
