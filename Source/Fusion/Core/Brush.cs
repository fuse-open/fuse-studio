using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static class ToBrushExtension
	{
		public static Brush AsBrush(this IObservable<Color> color)
		{
			return new Brush(color.Replay(1).RefCount());
		}

		public static Brush Switch(this IObservable<Brush> brush)
		{
			return new Brush(brush.Select(b => (IObservable<Color>)b).Switch().Replay(1).RefCount());
		}
	}

	public struct Brush : IObservable<Color>
	{
		public static Brush operator |(Brush left, Brush right)
		{
			return left.IsDefault ? right : left;
		}

		public static implicit operator Brush(Color color)
		{
			return new Brush(Observable.Return(color));
		}

		public static Brush Transparent
		{
			get { return new Brush(null); }
		}

		readonly IObservable<Color> _color;

		internal Brush(IObservable<Color> color)
		{
			_color = color;
		}
		
		public bool IsDefault
		{
			get { return _color == null; }
		}

		public Brush Mix(Brush other, IObservable<double> amount)
		{
			return this.CombineLatest(other, amount, (a, b, v) => a.Mix(b, (float)v)).AsBrush();
		}

		public Brush WithAlpha(float alpha)
		{
			if (IsDefault)
				return default(Brush);

			return _color.Select(c => c.WithAlpha(a: alpha)).AsBrush();
		}

		public Brush WithAlpha(IObservable<float> alpha)
		{
			if (IsDefault)
				return default(Brush);

			return _color.CombineLatest(alpha, (c, a) => c.WithAlpha(a: a)).AsBrush();
		}

		public IDisposable Subscribe(IObserver<Color> observer)
		{
			if (_color == null)
			{
				observer.OnNext(Color.Transparent);
				observer.OnCompleted();
				return Disposable.Empty;
			}

			return _color.Subscribe(observer);
		}
	}
}