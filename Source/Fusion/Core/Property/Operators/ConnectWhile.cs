using System;

namespace Outracks
{
	public static partial class Property
	{
		public static IProperty<T> ConnectWhile<T>(this IProperty<T> self, IObservable<bool> condition)
		{
			return AsProperty(
				value: ((IObservable<T>) self).ConnectWhile(condition),
				write: self.Write,
				isReadOnly: self.IsReadOnly.ConnectWhile(condition));
		}
	}
}