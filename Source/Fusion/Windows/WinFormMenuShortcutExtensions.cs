using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace Outracks.Fusion.Windows
{
	static class WinFormMenuShortcutExtensions
	{
		public static Shortcut ToFormShortcut(this HotKey hotKey)
		{
			const int shiftFlag = 0x10000;
			const int ctrlFlag = 0x20000;
			const int altFlag = 0x40000;

			var modifierFlag = 0;

			if (hotKey.Modifier.HasFlag(ModifierKeys.Shift))
				modifierFlag |= shiftFlag;

			if (hotKey.Modifier.HasFlag(ModifierKeys.Control))
				modifierFlag |= ctrlFlag;

			if (hotKey.Modifier.HasFlag(ModifierKeys.Alt))
				modifierFlag |= altFlag;

			var shortcutCode = modifierFlag | (int)hotKey.Key;
			if (!IsValidShortcut(shortcutCode))
				throw new InvalidEnumArgumentException("Not supported hotkey " + hotKey.ToString());

			return (Shortcut)shortcutCode;
		}

		static bool IsValidShortcut(int shortcutCode)
		{
			var values = Enum.GetValues(typeof(Shortcut)).Cast<int>();
			return values.Contains(shortcutCode);
		}
	}
}