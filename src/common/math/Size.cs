using System;
using System.Collections.Generic;
using System.IO;

namespace Outracks
{
	public static partial class Size
	{
		// Constructors
		public static Size<T> Create<T>(T both)
		{
			return new Size<T>(both, both);
		}

		public static Size<T> Create<T>(T width, T height)
		{
			return new Size<T>(width, height);
		}

		public static Size<T> Create<T>(T u, T v, Axis2D firstAxis)
		{
			return firstAxis == Axis2D.Horizontal ? Create(u, v) : Create(v, u);
		}

		public static Size<T> Read<T>(BinaryReader reader) where T : INumeric<T>, new()
		{
			return new Size<T>(
				new T().FromDouble(reader.ReadDouble()),
				new T().FromDouble(reader.ReadDouble()));
		}

		public static void Write<T>(BinaryWriter writer, Size<T> size) where T : INumeric<T>
		{
			writer.Write(size.Width.ToDouble());
			writer.Write(size.Height.ToDouble());
		}

		// Transformers

		public static Size<T> WithAxis<T>(this Size<T> size, Axis2D axis, Func<T, T> value)
		{
			return axis == Axis2D.Horizontal
				? new Size<T>(value(size.Width), size.Height)
				: new Size<T>(size.Width, value(size.Height));
		}

		// CanContain

		public static bool CanContain<T>(this Size<T> container, Size<T> content) where T : INumeric<T>
		{
			return container.Width.GreaterThanOrEquals(content.Width) && container.Height.GreaterThanOrEquals(content.Height);
		}

		public static bool HasZeroArea<T>(this Size<T> size) where T : INumeric<T>
		{
			return size.Width.Equals(size.Width.Zero)
				|| size.Height.Equals(size.Width.Zero);
		}

		public static Size<T> Zero<T>() where T : INumeric<T>, new ()
		{
			return Create(new T().Zero);
		}
	}

	public partial struct Size<T> : IEquatable<Size<T>>
	{
		static Size()
		{
			Algebras.Initialize();
		}

		public readonly T Width;
		public readonly T Height;

		public static implicit operator Vector<T>(Size<T> size)
		{
			return size.ToVector();
		}

		public Vector<T> ToVector()
		{
			return new Vector<T>(Width, Height);
		}

		public static implicit operator Size<T>(Vector<T> vector)
		{
			return new Size<T>(vector);
		}

		public Size(Vector<T> vector)
			: this(vector.X, vector.Y)
		{ }

		public Size(T width, T height)
		{
			Width = width;
			Height = height;
		}

		public Size<T> SwapWidthAndHeight()
		{
			return new Size<T>(Height, Width);
		}

		public T this[Axis2D axis]
		{
			get { return axis == Axis2D.Horizontal ? Width : Height; }
		}

		public static bool operator ==(Size<T> left, Size<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Size<T> left, Size<T> right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Size<T> && Equals((Size<T>)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (EqualityComparer<T>.Default.GetHashCode(Width) * 397) ^ EqualityComparer<T>.Default.GetHashCode(Height);
			}
		}

		public bool Equals(Size<T> other)
		{
			return
				Width.Equals(other.Width) &&
					Height.Equals(other.Height);
		}

		public override string ToString()
		{
			return Width + " x " + Height;
		}
	}
}