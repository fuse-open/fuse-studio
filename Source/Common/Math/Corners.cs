using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Outracks
{
	public enum Corner
	{
		LeftTop,
		RightTop,
		RightBottom,
		LeftBottom
	}

	public static class Corners
	{
		public static Corners<Points> Zero = new Corners<Points>(0);

		public static Corners<IObservable<T>> Transpose<T>(this IObservable<Corners<T>> self)
		{
			return Create(
				self.Select(s => s.LeftTop),
				self.Select(s => s.RightTop),
				self.Select(s => s.RightBottom),
				self.Select(s => s.LeftBottom));
		}

		public static Corners<IObservable<T>> Inverse<T>(this Corners<IObservable<T>> self)
			where T: INumeric<T>
		{
			return self.Select(t => t.Inverse());
		}
		
		public static IObservable<Corners<T>> Inverse<T>(this IObservable<Corners<T>> self)
			where T : INumeric<T>
		{
			return self.Select(t => t.Inverse());
		}
		
		public static Corners<T> Inverse<T>(this Corners<T> self)
			where T : INumeric<T>
		{
			return self.Select(t => t.Inverse());
		}

		public static IObservable<Corners<T>> Transpose<T>(this Corners<IObservable<T>> self)
		{
			return Observable.CombineLatest(
				self.LeftTop,
				self.RightTop,
				self.RightBottom,
				self.LeftBottom,
				Create);
		}

		public static Corners<T> Create<T>(T leftTop, T rightTop, T rightBottom, T leftBottom)
		{
			return new Corners<T>(leftTop, rightTop, rightBottom, leftBottom);
		}

		public static Corners<T> Create<T>(Func<Corner, T> corner)
		{
			return new Corners<T>(corner(Corner.LeftTop), corner(Corner.RightTop), corner(Corner.RightBottom), corner(Corner.LeftBottom));
		}
	
		public static Corners<TOut> Select<TIn, TOut>(this Corners<TIn> self, Func<TIn, TOut> transform)
		{
			return new Corners<TOut>(
				leftTop: transform(self.LeftTop),
				rightTop: transform(self.RightTop),
				rightBottom: transform(self.RightBottom),
				leftBottom: transform(self.LeftBottom));
		}

		public static IEnumerable<Tuple<Corner, T>> Where<T>(this Corners<T> self, Func<Corner, T, bool> predicate)
		{
			if (predicate(Corner.LeftBottom, self.LeftBottom))
				yield return Tuple.Create(Corner.LeftBottom, self.LeftBottom);
			if (predicate(Corner.LeftTop, self.LeftTop))
				yield return Tuple.Create(Corner.LeftTop, self.LeftTop);
			if (predicate(Corner.RightBottom, self.RightBottom))
				yield return Tuple.Create(Corner.RightBottom, self.RightBottom);
			if (predicate(Corner.RightTop, self.RightTop))
				yield return Tuple.Create(Corner.RightTop, self.RightTop);
		}

		public static bool CornersAreEqual<T>(this Corners<T> self)
		{
			return self.LeftBottom.Equals(self.LeftTop) 
				&& self.LeftTop.Equals(self.RightBottom) 
				&& self.RightBottom.Equals(self.RightTop);
		}
	}
	public sealed class Corners<T> : IEquatable<Corners<T>>
	{

		public readonly T LeftTop;
		public readonly T RightTop;
		public readonly T RightBottom;
		public readonly T LeftBottom;
		
		public Corners(T horizontal, T vertical)
			: this (horizontal, vertical, horizontal, vertical)
		{ }

		public Corners(T all)
			: this (all, all, all, all)
		{ }

		public Corners(T leftTop, T rightTop, T rightBottom, T leftBottom)
		{
			LeftTop = leftTop;
			RightTop = rightTop;
			RightBottom = rightBottom;
			LeftBottom = leftBottom;
		}


		public Corners<T> With(
			Optional<T> leftTop = default(Optional<T>),
			Optional<T> rightTop = default(Optional<T>),
			Optional<T> rightBottom = default(Optional<T>),
			Optional<T> leftBottom = default(Optional<T>))
		{
			return new Corners<T>(
				leftTop.Or(LeftTop),
				rightTop.Or(RightTop),
				rightBottom.Or(RightBottom),
				leftBottom.Or(LeftBottom));
		}

		public Corners<T> With(Corners<bool> cornersToChange, T newValue)
		{
			return new Corners<T>(
				cornersToChange.LeftTop ? newValue : LeftTop,
				cornersToChange.RightTop ? newValue : RightTop,
				cornersToChange.RightBottom ? newValue : RightBottom,
				cornersToChange.LeftBottom ? newValue : LeftBottom);
		}

		public Corners<T> With(Corner corner, Func<T, T> value)
		{
			switch (corner)
			{
				case Corner.LeftBottom:
					return With(leftBottom: value(LeftBottom));
				case Corner.LeftTop:
					return With(leftTop: value(LeftTop));
				case Corner.RightBottom:
					return With(rightBottom: value(RightBottom));
				case Corner.RightTop:
					return With(rightTop: value(RightTop));
				default:
					throw new InvalidEnumArgumentException();
			}
		}

		public bool Equals(Corners<T> other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return EqualityComparer<T>.Default.Equals(LeftTop, other.LeftTop) && EqualityComparer<T>.Default.Equals(RightTop, other.RightTop) && EqualityComparer<T>.Default.Equals(RightBottom, other.RightBottom) && EqualityComparer<T>.Default.Equals(LeftBottom, other.LeftBottom);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is Corners<T> && Equals((Corners<T>)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = EqualityComparer<T>.Default.GetHashCode(LeftTop);
				hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(RightTop);
				hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(RightBottom);
				hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(LeftBottom);
				return hashCode;
			}
		}

		public static bool operator ==(Corners<T> a, Corners<T> b)
		{
			return Equals(a, b);
		}

		public static bool operator !=(Corners<T> a, Corners<T> b)
		{
			return !Equals(a, b);
		}

		public override string ToString()
		{
			return "{" + LeftTop + ", " + RightTop + ", " + RightBottom + ", " + LeftBottom + "}";
		}

		public T this[Corner corner]
		{
			get
			{
				switch (corner)
				{
					case Corner.LeftTop: return LeftTop;
					case Corner.RightTop: return RightTop;
					case Corner.LeftBottom: return LeftBottom;
					case Corner.RightBottom: return RightBottom;
				}
				throw new ArgumentOutOfRangeException();
			}
		}
	}
}