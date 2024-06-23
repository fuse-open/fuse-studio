using System;
using System.Globalization;
using System.Linq;
using Outracks.Fusion;
using Uno.UX.Markup;

namespace Outracks.Fuse
{
	public static class ColorParser
	{
		public static IAttribute<Color> GetColor(this IElement element, string property, Color defaultValue)
		{
			return element[property].Convert(
				parse: TryParseColor,
				serialize: Serialize,
				defaultValue: defaultValue);
		}


		public static string Serialize(Color color)
		{
			return WriteHex(color.R, color.G, color.B, color.A);
		}

		public static string WriteHex(params double[] values)
		{
			var fullHex = DumbWriteHex(values);
			// try to remove fourth FF component
			if (values.Length == 4 && fullHex.EndsWith("FF"))
				fullHex = fullHex.StripSuffix("FF");

			// try with short form
			var shortHex = "#" + fullHex[1] + fullHex[3] + fullHex[5];
			var shortHexValue = TryReadComponents(shortHex, 3).Or(new double[0]);
			var shortHexAsFull = DumbWriteHex(shortHexValue);
			if (shortHexAsFull == fullHex)
				return shortHex;

			// otherwise full hex (that might lack the last part)
			return fullHex;
		}

		static string DumbWriteHex(double[] values)
		{
			var s = "#";
			foreach (var value in values)
				s += ((int)(value * 255.0)).ToString("X2");
			return s;
		}

		public static string WriteComponents(double[] values)
		{
			if (values.Distinct().IsSingle())
				return values.FirstOrDefault().ToString("0.##", CultureInfo.InvariantCulture);

			return values.Select(v => v.ToString("0.##", CultureInfo.InvariantCulture)).Join(", ");
		}
		public static Optional<double[]> TryReadComponents(string value, int componentCount)
		{
			try
			{
				var parsedeValue = AtomicValueParser.ParseFloatVector<float>(value, componentCount, FileSourceInfo.Unknown);
				return parsedeValue.Components.Select(c => (double)c.Value).ToArray();
			}
			catch (Exception)
			{
				return Optional.None();
			}
			/*
			var parts = value.Split(",");
			var components = new double[parts.Length];

			for (int i = 0; i < parts.Length; i++)
			{
				if (!double.TryParse(parts[i], NumberStyles.Any, CultureInfo.InvariantCulture, out components[i]))
					return Optional.None();
			}

			if (components.Length == componentCount)
				return components;

			if (components.Length == 1)
			{
				var uniformResult = new double[componentCount];
				for (int i = 0; i < componentCount; i++)
					uniformResult[i] = components[0];
				return uniformResult;
			}
			*/

		}
		public static Parsed<Color> TryParseColor(string str)
		{
			if (str.StartsWith("#"))
				return new Parsed<Color>
				{
					String = str,
					Value = TryParseColorHex(str.AfterFirst("#")),
				};

			var vector = str.Split(",").Select(v => v.Trim()).ToArray();
			switch (vector.Length)
			{
				case 3:
					return new Parsed<Color>
					{
						String = str,
						Value =
							ScalarParser.TryParseFloat(vector[0]).Value.SelectMany(r =>
							ScalarParser.TryParseFloat(vector[1]).Value.SelectMany(g =>
							ScalarParser.TryParseFloat(vector[2]).Value.Select(b =>
								new Color(r, g, b, 1))))
					};
				case 4:
					return new Parsed<Color>
					{
						String = str,
						Value =
							ScalarParser.TryParseFloat(vector[0]).Value.SelectMany(r =>
							ScalarParser.TryParseFloat(vector[1]).Value.SelectMany(g =>
							ScalarParser.TryParseFloat(vector[2]).Value.SelectMany(b =>
							ScalarParser.TryParseFloat(vector[3]).Value.Select(a =>
								new Color(r, g, b, a)))))
					};
			}
			return Parsed.Failure<Color>(str);
		}

		static Optional<Color> TryParseColorHex(string hex)
		{
			switch (hex.Length)
			{
				case 3: return TryParseColorHex111(hex);
				case 4: return TryParseColorHex1111(hex);
				case 6: return TryParseColorHex222(hex);
				case 8: return TryParseColorHex2222(hex);
				default: return Optional.None();
			}
		}

		static Optional<Color> TryParseColorHex2222(string hex)
		{
			return
				ScalarParser.ParseHex(hex.Substring(0, 2)).SelectMany(r =>
				ScalarParser.ParseHex(hex.Substring(2, 2)).SelectMany(g =>
				ScalarParser.ParseHex(hex.Substring(4, 2)).SelectMany(b =>
				ScalarParser.ParseHex(hex.Substring(6, 2)).Select(a => new Color(r, g, b, a)))));
		}


		static Optional<Color> TryParseColorHex222(string hex)
		{
			return
				ScalarParser.ParseHex(hex.Substring(0, 2)).SelectMany(r =>
				ScalarParser.ParseHex(hex.Substring(2, 2)).SelectMany(g =>
				ScalarParser.ParseHex(hex.Substring(4, 2)).Select(b => new Color(r, g, b, 1))));
		}

		static Optional<Color> TryParseColorHex1111(string hex)
		{
			return
				ScalarParser.ParseHex(hex.Substring(0, 1)).SelectMany(r =>
				ScalarParser.ParseHex(hex.Substring(1, 1)).SelectMany(g =>
				ScalarParser.ParseHex(hex.Substring(2, 1)).SelectMany(b =>
				ScalarParser.ParseHex(hex.Substring(3, 1)).Select(a => new Color(r, g, b, a)))));
		}

		static Optional<Color> TryParseColorHex111(string hex)
		{
			return
				ScalarParser.ParseHex(hex.Substring(0, 1)).SelectMany(r =>
				ScalarParser.ParseHex(hex.Substring(1, 1)).SelectMany(g =>
				ScalarParser.ParseHex(hex.Substring(2, 1)).Select(b => new Color(r, g, b, 1))));
		}
	}
}