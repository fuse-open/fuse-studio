namespace Outracks
{
	public static partial class Property
	{
		public static IProperty<string> OrEmpty(this IProperty<Optional<string>> property)
		{
			return property.Convert(
				convert: p => p.Or(""),
				convertBack: s => string.IsNullOrEmpty(s) ? Optional.None() : Optional.Some(s));
		}

		public static IProperty<bool> OrFalse(this IProperty<Optional<bool>> property)
		{
			return property.Convert(
				convert: a => a.Or(false),
				convertBack: b => b ? Optional.Some(true) : Optional.None());
		}

		public static IProperty<bool> OrTrue(this IProperty<Optional<bool>> property)
		{
			return property.Convert(
				convert: b => b.Or(true),
				convertBack: b => b ? Optional.None() : Optional.Some(false));
		}

		public static IProperty<T> Or<T>(this IProperty<Optional<T>> property, T value)
		{
			return property.Convert(
				convert: v => v.Or(value),
				convertBack: Optional.Some);
		}
	}
}