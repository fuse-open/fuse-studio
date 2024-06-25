using System;

namespace Outracks
{
	public interface INumeric<T> :
		IEquatable<T>,
		IComparable<T>,
		IRing<T>
	{
		T FromDouble(double value);
		double ToDouble();
	}
}