using System;

namespace Outracks.Fuse
{
	public static class VectorParser
	{
		public static IAttribute<Corners<Points>> GetCorners(this IElement element, string property, Corners<Points> defaultValue)
		{
			return element[property].Convert(
				parse: TryParseCornerRadius,
				serialize: SerializeCornerRadius,
				defaultValue: defaultValue);
		}

		public static IAttribute<Thickness<Points>> GetThickness(this IElement element, string property, Thickness<Points> defaultValue)
		{
			return element[property].Convert(
				parse: TryParseThickness,
				serialize: SerializeThickness,
				defaultValue: defaultValue);
		}

		// Parse

		static Parsed<Corners<Points>> TryParseCornerRadius(string str)
		{
			var parts = str.Split(",");
			switch (parts.Length)
			{
				case 1:
					return parts[0].TryParsePoints()
						.Select(all => new Corners<Points>(all));

				case 2:
					return Parsed.Create(
						stringValue: str,
						value: Optional.Combine(
							parts[0].TryParsePoints().Value,
							parts[1].TryParsePoints().Value,
							(h, v) => new Corners<Points>(h, v)));

				case 4:
					return Parsed.Create(
						stringValue: str,
						value: Optional.Combine(
							parts[0].TryParsePoints().Value,
							parts[1].TryParsePoints().Value,
							parts[2].TryParsePoints().Value,
							parts[3].TryParsePoints().Value,
							(l, t, r, b) => new Corners<Points>(l, t, r, b)));

				default:
					return Parsed.Failure<Corners<Points>>(str);
			}
		}

		static Parsed<Thickness<Points>> TryParseThickness(string str)
		{
			var parts = str.Split(",");
			switch (parts.Length)
			{
				case 1:
					return parts[0].TryParsePoints()
						.Select(all => new Thickness<Points>(all));

				case 2:
					return Parsed.Create(
						stringValue: str,
						value: Optional.Combine(
							parts[0].TryParsePoints().Value,
							parts[1].TryParsePoints().Value,
							(h, v) => new Thickness<Points>(h, v)));

				case 4:
					return Parsed.Create(
						stringValue: str,
						value: Optional.Combine(
							parts[0].TryParsePoints().Value,
							parts[1].TryParsePoints().Value,
							parts[2].TryParsePoints().Value,
							parts[3].TryParsePoints().Value,
							(l, t, r, b) => new Thickness<Points>(l, t, r, b)));

				default:
					return Parsed.Failure<Thickness<Points>>(str);
			}
		}

		public static Func<string, Parsed<Size<T>>> CreateSizeParser<T>(Func<string, Parsed<T>> tryParseLength)
		{
			return value =>
			{
				var parts = value.Split(",");
				if (parts.Length == 1)
					return tryParseLength(parts[0]).Select(Size.Create);

				if (parts.Length == 2)
					return Parsed.Create(value,
						tryParseLength(parts[0]).Value.SelectMany(x =>
							tryParseLength(parts[1]).Value.Select(y =>
								Size.Create(x, y))));

				return Parsed.Failure<Size<T>>(value);
			};
		}

		// Serialize

		static string SerializeCornerRadius(Corners<Points> thickness)
		{
			return new[]
			{
				thickness.LeftTop.Value.ToString("0.##"),
				thickness.RightTop.Value.ToString("0.##"),
				thickness.RightBottom.Value.ToString("0.##"),
				thickness.LeftBottom.Value.ToString("0.##"),
			}.Join(", ");
		}

		static string SerializeThickness(Thickness<Points> thickness)
		{
			return new[]
			{
				thickness.Left.Value.ToString("0.##"),
				thickness.Top.Value.ToString("0.##"),
				thickness.Right.Value.ToString("0.##"),
				thickness.Bottom.Value.ToString("0.##"),
			}.Join(", ");
		}

		public static Func<Size<UxSize>, string> CreateSizeSerializer(Func<UxSize, string> serialize)
		{
			return size => serialize(size.Width) + ", " + serialize(size.Height);
		}
	}
}