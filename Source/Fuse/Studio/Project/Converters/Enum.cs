using System;

namespace Outracks.Fuse
{
	public static class EnumParser
	{
		public static IAttribute<T> GetEnum<T>(this IElement element, string property, T defaultValue)
			where T : struct
		{
			return element[property].Convert(
				parse: TryParseEnum<T>,
				serialize: t => t.ToString(),
				defaultValue: defaultValue);
		}

		static Parsed<T> TryParseEnum<T>(string str)
			where T : struct
		{
			T value;
			if (Enum.TryParse(str, out value) && Enum.IsDefined(typeof(T), value))
				return Parsed.Success(value, str);

			return Parsed.Failure<T>(str);
		} 
	}
}