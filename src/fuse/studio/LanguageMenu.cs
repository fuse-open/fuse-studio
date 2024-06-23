using Outracks.Fusion;
using Outracks.Fusion.Dialogs;
using System;

namespace Outracks.Fuse
{
	public static class LanguageMenu
	{
		public static readonly IProperty<Language> CurrentLanguage = UserSettings
																		.Enum<Language>("Language")
																		.Or(Strings.Culture.GetLanguage())
																		.AutoInvalidate();

		public static Menu Menu = Menu.Option(
									value: Language.English,
									name: "English",
									property: CurrentLanguage)
								+ Menu.Option(
									value: Language.French,
									name: "Français",
									property: CurrentLanguage)
								+ Menu.Option(
									value: Language.Korean,
									name: "한국인",
									property: CurrentLanguage);

		public static void Initialize()
		{
			var firstInvoke = true;

			CurrentLanguage.Subscribe(lang => {
				Strings.Culture = lang.GetCulture();
				Texts.Update();

				if (firstInvoke)
				{
					firstInvoke = false;
					return;
				}

				MessageBox.BringToFront();
				MessageBox.Show(Strings.LanguageUpdated_Text, "fuse X", MessageBoxType.Information);

				// Avoid getting two popups in a row
				firstInvoke = true;
			});
		}
	}
}
