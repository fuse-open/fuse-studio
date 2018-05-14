using System;
using System.IO;
using System.Linq;

namespace Outracks
{
	public static partial class MatrixExtensions
	{
		public static void Write(BinaryWriter writer, Matrix matrix)
		{
			writer.Write(matrix.M11);
			writer.Write(matrix.M12);
			writer.Write(matrix.M13);
			writer.Write(matrix.M14);

			writer.Write(matrix.M21);
			writer.Write(matrix.M22);
			writer.Write(matrix.M23);
			writer.Write(matrix.M24);

			writer.Write(matrix.M31);
			writer.Write(matrix.M32);
			writer.Write(matrix.M33);
			writer.Write(matrix.M34);

			writer.Write(matrix.M41);
			writer.Write(matrix.M42);
			writer.Write(matrix.M43);
			writer.Write(matrix.M44);
		}

		public static Matrix Read(BinaryReader reader)
		{
			return new Matrix(
				reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(),
				reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(),
				reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(),
				reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
		}

		public static Point<T> Transform<T>(this Point<T> p, Matrix m) where T : INumeric<T>, new()
		{
			return new Point<T>(
				new T().FromDouble(p.X.ToDouble() * m.M11 + p.Y.ToDouble() * m.M21 + m.M41),
				new T().FromDouble(p.X.ToDouble() * m.M12 + p.Y.ToDouble() * m.M22 + m.M42));
		}
	}

	public partial class Matrix
	{
		static Matrix()
		{
			Algebras.Initialize();
		}

		public readonly double M11, M12, M13, M14;
		public readonly double M21, M22, M23, M24;
		public readonly double M31, M32, M33, M34;
		public readonly double M41, M42, M43, M44;

		public static Matrix Translate(double x = 0.0, double y = 0.0, double z = 0.0)
		{
			return new Matrix(
				1, 0, 0, x,
				0, 1, 0, y,
				0, 0, 1, 0,
				0, 0, 0, 1);
		}

		public static Matrix Rotate(double theta)
		{
			var cos = Math.Cos(theta);
			var sin = Math.Sin(theta);
			return new Matrix(
				cos, -sin, 0, 0,
				sin,  cos, 0, 0,
				  0,    0, 1, 0,
				  0,    0, 0, 1);
		}

		public static Matrix Multiply(params Matrix[] matrices)
		{
			return matrices.Aggregate(Multiply);
		}

		public static Matrix Multiply(Matrix left, Matrix right)
		{
			return new Matrix(
				Dot(left.GetRow(1), right.GetColumn(1)),
				Dot(left.GetRow(1), right.GetColumn(2)),
				Dot(left.GetRow(1), right.GetColumn(3)),
				Dot(left.GetRow(1), right.GetColumn(4)),
				Dot(left.GetRow(2), right.GetColumn(1)),
				Dot(left.GetRow(2), right.GetColumn(2)),
				Dot(left.GetRow(2), right.GetColumn(3)),
				Dot(left.GetRow(2), right.GetColumn(4)),
				Dot(left.GetRow(3), right.GetColumn(1)),
				Dot(left.GetRow(3), right.GetColumn(2)),
				Dot(left.GetRow(3), right.GetColumn(3)),
				Dot(left.GetRow(3), right.GetColumn(4)),
				Dot(left.GetRow(4), right.GetColumn(1)),
				Dot(left.GetRow(4), right.GetColumn(2)),
				Dot(left.GetRow(4), right.GetColumn(3)),
				Dot(left.GetRow(4), right.GetColumn(4)));
		}

		static double Dot(
			Tuple<double, double, double, double> left,
			Tuple<double, double, double, double> right)
		{
			return left.Item1 * right.Item1
				+ left.Item2 * right.Item2
				+ left.Item3 * right.Item3
				+ left.Item4 * right.Item4;
		}

		public Matrix(
			double m11, double m12, double m13, double m14, 
			double m21, double m22, double m23, double m24, 
			double m31, double m32, double m33, double m34, 
			double m41, double m42, double m43, double m44)
		{
			M11 = m11; M12 = m12; M13 = m13; M14 = m14;
			M21 = m21; M22 = m22; M23 = m23; M24 = m24;
			M31 = m31; M32 = m32; M33 = m33; M34 = m34;
			M41 = m41; M42 = m42; M43 = m43; M44 = m44;
		}

		public Tuple<double, double, double, double> GetColumn(int column)
		{
			if (column == 1) return Tuple.Create(M11, M21, M31, M41);
			if (column == 2) return Tuple.Create(M12, M22, M32, M42);
			if (column == 3) return Tuple.Create(M13, M23, M33, M43);
			if (column == 4) return Tuple.Create(M14, M24, M34, M44);

			throw new ArgumentOutOfRangeException("column", column, "Row number was out of range. Range is [1,4].");
		}

		public Tuple<double, double, double, double> GetRow(int row)
		{
			if (row == 1) return Tuple.Create(M11, M12, M13, M14);
			if (row == 2) return Tuple.Create(M21, M22, M23, M24);
			if (row == 3) return Tuple.Create(M31, M32, M33, M34);
			if (row == 4) return Tuple.Create(M41, M42, M43, M44);

			throw new ArgumentOutOfRangeException("row", row, "Row number was out of range. Range is [1,4].");
		}
	}
}