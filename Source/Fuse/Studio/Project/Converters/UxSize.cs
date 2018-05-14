using System;

namespace Outracks.Fuse
{
	public static class UxSizeParser
	{
		public static IAttribute<Size<UxSize>> GetSize2(this IElement element, string property, Size<UxSize> defaultValue)
		{
			return element[property].Convert(
				parse: VectorParser.CreateSizeParser(TryParseLength),
				serialize: VectorParser.CreateSizeSerializer(Serialize),
				defaultValue: defaultValue);
		}

		public static IAttribute<UxSize> GetSize(this IElement element, string property, UxSize defaultValue)
		{
			return element[property].Convert(
				parse: TryParseLength,
				serialize: Serialize,
				defaultValue: defaultValue);
		}

		public static IAttribute<Points> GetPoints(this IElement element, string property, Points defaultValue)
		{
			return element[property].Convert(
				parse: TryParsePoints,
				serialize: Serialize,
				defaultValue: defaultValue);
		}

		public static IAttribute<Points> ToPoints(this IAttribute<double> property, double pointIncrement = 1.0)
		{
			return property.ConvertExpression(t => new Points(t / pointIncrement), t => t.Value * pointIncrement);
		}

		// UxSize = Percentages | Points | Pixels

		public static Parsed<UxSize> TryParseLength(string str)
		{
			return str.TryParsePoints().Select(UxSize.Points)
				|| str.TryParsePixels().Select(UxSize.Pixels)
				|| str.TryParsePercentages().Select(UxSize.Percentages);
		}

		public static string Serialize(UxSize len)
		{
			return
				len.PointsValue.Select(Serialize).Or(() =>
				len.PixelsValue.Select(Serialize).Or(() =>
				len.PercentagesValue.Select(Serialize).OrThrow()));
		}

		// Percentages

		public static Parsed<Percentages> TryParsePercentages(this string str)
		{
			if (!str.EndsWith("%"))
				return ScalarParser.TryParseDouble(str).Select(d => new Percentages(d));

			double value;
			if (double.TryParse(str.StripSuffix("%"), out value))
				return Parsed.Success(new Percentages(value), str);

			return Parsed.Failure<Percentages>(str);
		}

		public static string Serialize(Percentages d)
		{
			return (d.Value).ToString("0") + "%";
		}

		// Points

		public static Parsed<Points> TryParsePoints(this string str)
		{
			double value;
			if (Double.TryParse(str.StripSuffix("pt").StripSuffix("p"), out value))
				return Parsed.Success(new Points(value), str);

			return Parsed.Failure<Points>(str);
		}
		public static string Serialize(Points points)
		{
			return points.Value.ToString("0.#");
		}

		// Pixels

		public static Parsed<Pixels> TryParsePixels(this string str)
		{
			double value;
			if (Double.TryParse(str.StripSuffix("px"), out value))
				return Parsed.Success(new Pixels(value), str);

			return Parsed.Failure<Pixels>(str);
		}

		public static string Serialize(Pixels points)
		{
			return points.Value.ToString("0.#") + "px";
		}
	}
}