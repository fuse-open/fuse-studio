using System;

namespace Outracks.Fuse
{
	public interface IExpression<T>
	{
		T Inherited { get; }
		Optional<Parsed<T>> Local { get; }
		Optional<string> Data { get; }

		Optional<string> Property { get; }
	}


	public static class UxExpression
	{
		public static IExpression<TOut> Select<TIn, TOut>(this IExpression<TIn> input, Func<TIn, TOut> convert, Func<TOut, string> stringify)
		{
			return new MutableExpression<TOut>
			{
				Inherited = convert(input.Inherited),
				Data = input.Data,
				Property = input.Property,
				Local = input.Local.Select(pv =>
					new Parsed<TOut>()
					{
						String = pv.Value.Select(v => stringify(convert(v))).Or(""),
						Value = pv.Value.Select(convert)
					}),
			};
		}

		public static IExpression<T> Inherited<T>(T inherited)
		{
			return new MutableExpression<T>
			{
				Inherited = inherited,
				Local = Optional.None(),
				Data = Optional.None(),
				Property = Optional.None(),
			};
		}

		public static IExpression<T> Local<T>(string local, Func<string, Parsed<T>> parse, T inherited)
		{
			var expression = new MutableExpression<T>
			{
				Inherited = inherited,
				Local = Optional.Some(parse(local)),
				Data = Optional.None(),
				Property = Optional.None(),
			};

			var p = local.TrimStart();
			if (!p.StartsWith("{"))
 				return expression;
			p = p.StripPrefix("{").TrimStart();

			if (!p.StartsWith("Property"))
				return expression;
			p = p.StripPrefix("Property");

			p = p.TrimEnd().StripSuffix("}");

			expression.Property = Optional.Some(p);

			return expression;
		}

		public static IExpression<T> Data<T>(string data, T inherited)
		{
			return new MutableExpression<T>
			{
				Inherited = inherited,
				Local = Optional.None(),
				Data = Optional.Some(data),
				Property = Optional.None(),
			};
		}

		public static IExpression<T> Property<T>(string property, T inherited)
		{
			return new MutableExpression<T>
			{
				Inherited = inherited,
				Local = Optional.None(),
				Data = Optional.None(),
				Property = Optional.Some(property),
			};
		}

		class MutableExpression<T> : IExpression<T>
		{
			public T Inherited { get; set; }
			public Optional<Parsed<T>> Local { get; set; }
			public Optional<string> Data { get; set; }
			public Optional<string> Property { get; set; }
		}
	}
}