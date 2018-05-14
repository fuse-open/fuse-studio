using System;
using System.Globalization;

namespace Outracks.Fuse
{
	public static class TimeSpanParser
	{
		public static IAttribute<TimeSpan> GetTimeSpan(this IElement element, string property, TimeSpan defaultValue)
		{
			return element[property].Convert(
				parse: TryParseTimeSpan,
				serialize: d => d.TotalSeconds.ToString(CultureInfo.InvariantCulture),
				defaultValue: defaultValue);
		}

		static Parsed<TimeSpan> TryParseTimeSpan(string str)
		{
			double value;
			if (double.TryParse(str.StripSuffix("s"), out value))
				return Parsed.Success(TimeSpan.FromSeconds(value), str);

			return Parsed.Failure<TimeSpan>(str);
		}
	}
}