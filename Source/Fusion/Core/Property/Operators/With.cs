using System;

namespace Outracks
{
	public static partial class Property
	{
		public static IProperty<TResult> With<TResult>(
			this IProperty<TResult> self,
			IObservable<TResult> value = null,
			Action<TResult, bool> write = null,
			IObservable<bool> isReadOnly = null)
		{
			return AsProperty(
				value ?? self,
				write ?? self.Write,
				isReadOnly ?? self.IsReadOnly);
		}
	}
}