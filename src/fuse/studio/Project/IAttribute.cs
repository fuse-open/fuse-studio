using System;
using Outracks.Fusion;

namespace Outracks.Fuse
{
	public interface IAttribute<T> : IProperty<T>
	{
		IProperty<string> StringValue { get; }
		IObservable<IExpression<T>> Expression { get; }

		Command Clear { get; }
	}
}