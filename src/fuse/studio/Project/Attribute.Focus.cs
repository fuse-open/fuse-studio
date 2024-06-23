using System;
using System.Reactive.Linq;
using Outracks.Fusion;

namespace Outracks.Fuse
{
	public static class FocusProperty
	{
		public static IAttribute<TOut> Focus<TIn, TOut>(
			this IAttribute<TIn> property,
			Func<TIn, TOut> select,
			Func<TIn, TOut, TIn> combine,
			Func<string, Parsed<TOut>> parse,
			Func<TOut, string> serialize,
			TOut inheritedValue)
		{
			return property.Focus(
				c => c.Local
					.SelectMany(l => l.Value)
					.MatchWith(
						some: local => UxExpression.Local(serialize(select(local)), parse, inheritedValue),
						none: () => UxExpression.Inherited(inheritedValue)),
				select,
				combine);
		}

		public static IAttribute<TOut> Focus<TIn, TOut>(
			this IAttribute<TIn> property,
			Func<IExpression<TIn>, IExpression<TOut>> selectExpression,
			Func<TIn, TOut> select,
			Func<TIn, TOut, TIn> combine,
			Optional<Command> clear = default(Optional<Command>))
		{
			return Attribute.Create(
				property.Expression.Select(selectExpression),
				property.Select(@select),
				clear.Or(property.Clear),
				(value, save) => property.Take(1).Subscribe(lastValue => property.Write(combine(lastValue, value), save)),
				property.IsReadOnly,
				property.StringValue);
		}
	}
}