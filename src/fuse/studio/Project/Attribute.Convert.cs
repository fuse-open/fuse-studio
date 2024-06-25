using System;
using System.Reactive.Linq;
using Outracks.Fusion;

namespace Outracks.Fuse
{
	public static class ConvertProperty
	{
		public static IAttribute<TResult> ConvertExpression<TArgs, TResult>(
			this IAttribute<TArgs> self,
			Func<TArgs, TResult> convert,
			Func<TResult, TArgs> convertBack)
		{
			return Attribute.Create(
				self.Expression.Select(e => e.Select(convert, t => t.ToString())),
				self.Select(convert),
				self.Clear,
				(t, save) => self.Write(convertBack(t), save),
				self.IsReadOnly,
				self.StringValue);
		}

		public static IAttribute<TTo> Convert<TTo>(
			this IProperty<Optional<string>> property,
			Func<string, Parsed<TTo>> parse,
			Func<TTo, string> serialize,
			TTo defaultValue)
		{
			return Attribute.Create(
				expression: property.Select(v =>
					v.MatchWith(
						some: value => UxExpression.Local(value, parse, defaultValue),
						none: () => UxExpression.Inherited(defaultValue))),
				value: property.SelectSome(v =>
					v.MatchWith(
						some: prop => parse(prop).Value,
						none: () => Optional.Some(defaultValue))),
				clear: Command.Enabled(() => property.Write(Optional.None())),
				write: (value, save) => property.Write(serialize(value), save),
				isReadOnly: property.IsReadOnly,
				stringValue: property.OrEmpty());
		}
	}
}