using System.Globalization;

namespace Outracks.Fuse
{
	public enum Language
	{
		English,
		Korean,
		French,
	}

	public static class LocaleCulture
	{
		public static void Initialize(CultureInfo cultureInfo)
		{
			Strings.Culture = cultureInfo.GetLanguage().GetCulture();
		}

		public static Language GetLanguage(this CultureInfo cultureInfo)
		{
			switch (cultureInfo.Name)
			{
				case "fr-FR":
					return Language.French;
				case "ko-KR":
					return Language.Korean;
				default:
					return Language.English;
			}
		}

		public static CultureInfo GetCulture(this Language language)
		{
			switch (language)
			{
				default:
					return CultureInfo.GetCultureInfo("en-US");
				case Language.Korean:
					return CultureInfo.GetCultureInfo("ko-KR");
				case Language.French:
					return CultureInfo.GetCultureInfo("fr-FR");
			}
		}
	}
}
