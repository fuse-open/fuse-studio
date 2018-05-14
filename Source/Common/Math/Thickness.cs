using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Outracks
{
	public static class Thickness
	{
		public static Thickness<Points> Zero = new Thickness<Points>(0);

		public static Thickness<IObservable<T>> Transpose<T>(this IObservable<Thickness<T>> self)
		{
			return Create(
				self.Select(s => s.Left),
				self.Select(s => s.Top),
				self.Select(s => s.Right),
				self.Select(s => s.Bottom));
		}

		public static Thickness<IObservable<T>> Inverse<T>(this Thickness<IObservable<T>> self)
			where T: INumeric<T>
		{
			return self.Select(Inverse);
		}
		
		public static IObservable<Thickness<T>> Inverse<T>(this IObservable<Thickness<T>> self)
			where T : INumeric<T>
		{
			return self.Select(t => t.Inverse());
		}
		
		public static IObservable<T> Inverse<T>(this IObservable<T> self)
			where T: INumeric<T>
		{
			return self.Select(t => t.Inverse());
		}

		public static Thickness<T> Inverse<T>(this Thickness<T> self)
			where T : INumeric<T>
		{
			return self.Select(t => t.Inverse());
		}

		public static IObservable<Thickness<T>> Transpose<T>(this Thickness<IObservable<T>> self)
		{
			return Observable.CombineLatest(
				self.Left,
				self.Top,
				self.Right,
				self.Bottom,
				Create);
		}
		public static Thickness<T> Create<T>(Func<RectangleEdge, T> edgeFactory)
		{
			return new Thickness<T>(
				edgeFactory(RectangleEdge.Left), 
				edgeFactory(RectangleEdge.Top), 
				edgeFactory(RectangleEdge.Right), 
				edgeFactory(RectangleEdge.Bottom));
		}

		public static Thickness<T> Create<T>(T left, T top, T right, T bottom)
		{
			return new Thickness<T>(left, top, right, bottom);
		}
		public static Thickness<TOut> Select<TIn, TOut>(this Thickness<TIn> self, Func<TIn, RectangleEdge, TOut> transform)
		{
			return new Thickness<TOut>(
				left: transform(self.Left, RectangleEdge.Left),
				top: transform(self.Top, RectangleEdge.Top),
				right: transform(self.Right, RectangleEdge.Right),
				bottom: transform(self.Bottom, RectangleEdge.Bottom));
		}
		public static Thickness<TOut> Select<TIn, TOut>(this Thickness<TIn> self, Func<TIn, TOut> transform)
		{
			return new Thickness<TOut>(
				left: transform(self.Left),
				top: transform(self.Top),
				right: transform(self.Right),
				bottom: transform(self.Bottom));
		}

		public static Thickness<Points> CollapseEdgesExcept(this Thickness<Points> self, RectangleEdges edges)
		{
			return self.CollapseEdges(~edges);
		}

		public static Thickness<Points> CollapseEdges(this Thickness<Points> self, RectangleEdges edges)
		{
			return self.With(
				left: edges.HasFlag(RectangleEdges.Left) ? Optional.Some<Points>(0) : Optional.None(),
				top: edges.HasFlag(RectangleEdges.Top) ? Optional.Some<Points>(0) : Optional.None(),
				right: edges.HasFlag(RectangleEdges.Right) ? Optional.Some<Points>(0) : Optional.None(),
				bottom: edges.HasFlag(RectangleEdges.Bottom) ? Optional.Some<Points>(0) : Optional.None());
		}
	}
	public sealed class Thickness<T> : IEquatable<Thickness<T>>
	{

		public readonly T Left;
		public readonly T Top;
		public readonly T Right;
		public readonly T Bottom;
		
		public Thickness(T horizontal, T vertical)
			: this (horizontal, vertical, horizontal, vertical)
		{ }

		public Thickness(T all)
			: this (all, all, all, all)
		{ }

		public Thickness(T left, T top, T right, T bottom)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

		public T this[RectangleEdge edge]
		{
			get
			{
				switch (edge)
				{
					case RectangleEdge.Left:
						return Left;
					case RectangleEdge.Top:
						return Top;
					case RectangleEdge.Right:
						return Right;
					case RectangleEdge.Bottom:
						return Bottom;
				}
				throw new ArgumentOutOfRangeException();
			}
		}

		public Thickness<T> With(
			Optional<T> left = default(Optional<T>),
			Optional<T> top = default(Optional<T>),
			Optional<T> right = default(Optional<T>),
			Optional<T> bottom = default(Optional<T>))
		{
			return new Thickness<T>(
				left.Or(Left),
				top.Or(Top),
				right.Or(Right),
				bottom.Or(Bottom));
		}

		public bool Equals(Thickness<T> other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return EqualityComparer<T>.Default.Equals(Left, other.Left) && EqualityComparer<T>.Default.Equals(Top, other.Top) && EqualityComparer<T>.Default.Equals(Right, other.Right) && EqualityComparer<T>.Default.Equals(Bottom, other.Bottom);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is Thickness<T> && Equals((Thickness<T>)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = EqualityComparer<T>.Default.GetHashCode(Left);
				hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(Top);
				hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(Right);
				hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(Bottom);
				return hashCode;
			}
		}

		public static bool operator ==(Thickness<T> left, Thickness<T> right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Thickness<T> left, Thickness<T> right)
		{
			return !Equals(left, right);
		}

		public override string ToString()
		{
			return "{" + Left + ", " + Top + ", " + Right + ", " + Bottom + "}";
		}
	}
}