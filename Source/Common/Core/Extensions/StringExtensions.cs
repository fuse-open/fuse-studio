using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Outracks
{
	public static class Base64StringTools
	{
		public static string Obfuscated(this string value)
		{
			var sha512 = new SHA512Managed();
			return BitConverter.ToString(sha512.ComputeHash(Encoding.UTF8.GetBytes(value)));
		}

		static public string ToBase64(this string toEncode)
		{
			byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);
			return Convert.ToBase64String(toEncodeAsBytes);
		}

		static public Optional<string> FromBase64(this string encodedData)
		{
			try
			{
				byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);
				return Encoding.UTF8.GetString(encodedDataAsBytes);
			}
			catch (Exception)
			{
				return Optional.None();
			}
		}
	}
	

	public static class StringCaptialization
	{

		public static string Capitalize(this string str)
		{
			if (String.IsNullOrEmpty(str) || Char.IsUpper(str, 0))
				return str;

			return Char.ToUpperInvariant(str[0]) + str.Substring(1);
		}

		public static string Uncapitalize(this string str)
		{
			if (String.IsNullOrEmpty(str) || Char.IsLower(str, 0))
				return str;

			return Char.ToLowerInvariant(str[0]) + str.Substring(1);
		}

	}

	public static class StringTrimming
	{
		public static string Indent(this string str)
		{
			return str.Split('\n').Select(s => "\t" + s).Join("\n");
		}

		static readonly char[] Digits = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

		public static string TrimEndDigits(this string str)
		{
			return str.TrimEnd(Digits);
		}

		public static string StripSuffix(this string path, string suffix, StringComparison comparisonType = StringComparison.InvariantCulture)
		{
			return path.EndsWith(suffix, comparisonType)
				? path.Substring(0, path.Length - suffix.Length)
				: path;
		}

		public static string StripPrefix(this string path, string prefix, StringComparison comparisonType = StringComparison.InvariantCulture)
		{
			return path.StartsWith(prefix, comparisonType)
				? path.Substring(prefix.Length)
				: path;
		}
	}

	public static class StringSplitting
	{
		public static string AfterLastIfAny(this string s, string seperator)
		{
			var index = s.LastIndexOf(seperator, System.StringComparison.Ordinal);
			return index == -1 ? s : s.Substring(index + seperator.Length);
		}

		public static string AfterLast(this string s, string seperator)
		{
			var index = s.LastIndexOf(seperator, System.StringComparison.Ordinal);
			if (index == -1)
				throw new ArgumentException();
			return s.Substring(index + seperator.Length);
		}

		public static string BeforeLast(this string s, string seperator)
		{
			var index = s.LastIndexOf(seperator, System.StringComparison.Ordinal);
			if (index == -1)
				throw new ArgumentException();
			return s.Substring(0, index);

		}

		public static string AfterFirst(this string s, string seperator)
		{
			var index = s.IndexOf(seperator, System.StringComparison.Ordinal);
			if (index == -1)
				throw new ArgumentException();
			return s.Substring(index + seperator.Length);
		}
		public static string BeforeFirst(this string s, string seperator)
		{
			var index = s.IndexOf(seperator, System.StringComparison.Ordinal);
			if (index == -1)
				throw new ArgumentException();
			return s.Substring(0, index);

		}

		public static string[] Split(this string s, string seperator)
        {
            return s.Split(new[] { seperator }, StringSplitOptions.None);
        }

        public static string Join(this IEnumerable<string> si, string separator)
        {
            var s = si.ToArray();
            if (s.HasCount(0)) return "";
            if (s.HasCount(1)) return s.First();
            return s.Aggregate((a, b) => a + separator + b);
        }

	
    }

	public static class FormatStringWith
	{
		public static string FormatWith(this string format, params object[] args)
		{
			return string.Format(format, args);
		}
	}

}