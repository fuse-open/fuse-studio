using System;
using System.Reactive.Linq;

namespace Outracks.Fuse
{
	using Fusion;

	public static partial class Attribute
	{
		public static IAttribute<T> Default<T>()
		{
			return Create(
				Observable.Return(UxExpression.Inherited(default(T))),
				Observable.Return(default(T)),
				clear: Command.Disabled,
				write: (t, save) => { },
				isReadOnly: Observable.Return(true),
				stringValue: Property.Constant(""));
		}

		public static IObservable<bool> HasLocalValue<T>(this IAttribute<T> property)
		{
			return property.Expression.Select(e => e.Local.HasValue);
		}
		
		public static IObservable<Optional<T>> LocalValue<T>(this IAttribute<T> property)
		{
			return property.Expression.Select(e => e.Local.SelectMany(l => l.Value));
		}

		public static IAttribute<T> Create<T>(
			IObservable<IExpression<T>> expression,
			IObservable<T> value,
			Command clear,
			Action<T, bool> write,
			IObservable<bool> isReadOnly,
			IProperty<string> stringValue)
		{
			return new Implementation<T>
			{
				Clear = clear,
				Writer = write,
				IsReadOnly = isReadOnly,
				Expression = expression,
				Value = value,
				StringValue = stringValue,
			};
		}

		class Implementation<T> : IAttribute<T>
		{
			public IObservable<T> Value { get; set; }

			public IProperty<string> StringValue { get; set; }
			public IObservable<IExpression<T>> Expression { get; set; }

			public IObservable<bool> IsReadOnly { get; set; }
			
			public Command Clear { get; set; }

			public Action<T, bool> Writer { private get; set; }

			public void Write(T value, bool save)
			{
				Writer(value, save);
			}

			public IDisposable Subscribe(IObserver<T> observer)
			{
				return Value.Subscribe(observer);
			}
		}
	}
}