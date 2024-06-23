using System;

namespace Outracks.Fuse
{
	public static class ScalarParser
	{
		public static IAttribute<double> GetDouble(this IElement element, string property, double defaultValue)
		{
			return element[property].Convert(
				parse: TryParseDouble,
				serialize: d => d.ToString("0.###"),
				defaultValue: defaultValue);
		}

		public static IAttribute<int> GetInt(this IElement element, string property, int defaultValue)
		{
			return element[property].Convert(
				parse: TryParseInt,
				serialize: d => d.ToString(),
				defaultValue: defaultValue);
		}

		public static Optional<float> ParseHex(string hex)
		{
			try
			{
				return Convert.ToUInt32(hex, 16) / (float)Math.Pow(16, hex.Length);
			}
			catch (Exception)
			{
				return Optional.None();
			}
		}

		public static Parsed<float> TryParseFloat(string str)
		{
			float value;
			if (float.TryParse(str, out value))
				return Parsed.Success(value, str);

			return Parsed.Failure<float>(str);
		}

		public static Parsed<double> TryParseDouble(string str)
		{
			double value;
			if (double.TryParse(str, out value))
				return Parsed.Success(value, str);

			return Parsed.Failure<double>(str);
		}

		public static Parsed<int> TryParseInt(string str)
		{
			int value;
			if (int.TryParse(str, out value))
				return Parsed.Success(value, str);

			return Parsed.Failure<int>(str);
		}
	}
}