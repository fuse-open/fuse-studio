using System;

namespace Outracks.Fuse
{
	public static class BooleanParser
	{
		public static IAttribute<bool> GetBoolean(this IElement element, string property, bool defaultValue)
		{
			return element[property].Convert(
				parse: TryParseBoolean,
				serialize: t => t.ToString(),
				defaultValue: defaultValue);
		}

		static Parsed<bool> TryParseBoolean(string str)
		{
			bool value;
			if (Boolean.TryParse(str, out value))
				return Parsed.Success(value, str);

			return Parsed.Failure<bool>(str);
		}
	}
}