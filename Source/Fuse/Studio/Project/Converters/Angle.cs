namespace Outracks.Fuse
{
	public static class AngleParser
	{
		public static IAttribute<double> GetAngle(this IElement element, string property, double defaultValue)
		{
			return element[property].Convert(
				parse: TryParseAngle,
				serialize: d => d.ToString("0"),
				defaultValue: defaultValue);
		}

		static Parsed<double> TryParseAngle(string str)
		{
			double value;
			if (double.TryParse(str.StripSuffix("deg"), out value))
				return Parsed.Success(value, str);

			return Parsed.Failure<double>(str);
		}
	}
}