using System;

namespace Outracks.Fuse
{
	using Fusion;

	public interface IAttribute<T> : IProperty<T>
	{
		IProperty<string> StringValue { get; }
		IObservable<IExpression<T>> Expression { get; }

		Command Clear { get; }
	}
}