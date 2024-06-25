using ModifierKeys = Uno.Platform.EventModifiers;

namespace Outracks.UnoCore.WinFormsSupport
{
	/*
	public static class WinFormsInputState
	{
		public static InputState Query()
		{
			return new InputState(
				GetModifierKeysState(),
				GetMouseButtonState());
		}

		public static ModifierKeys GetModifierKeysState()
		{
			return
				GetState(ModifierKeys.AltKey) |
				GetState(ModifierKeys.ControlKey) |
				GetState(ModifierKeys.ShiftKey) |
				GetState(ModifierKeys.MetaKey);
		}

		public static MouseButtons GetMouseButtonState()
		{
			return
				GetState(MouseButtons.Left) |
				GetState(MouseButtons.Middle) |
				GetState(MouseButtons.Right) |
				GetState(MouseButtons.X1) |
				GetState(MouseButtons.X2);
		}

		static ModifierKeys GetState(ModifierKeys key)
		{
			return (GetKeyState(key.ToKeys() | Keys.Modifiers) & 0x8000) != 0 || System.Windows.Forms.Control.ModifierKeys.HasFlag(key.ToKeys())
				? key
				: 0;
		}

		static MouseButtons GetState(MouseButtons button)
		{
			return (GetKeyState(button.ToKeys()) & 0x8000) != 0
				? button
				: MouseButtons.None;
		}

		public static bool GetKeyStateOnlyWorksForModifiers(Key key)
		{
			var modifierKey = key.ToModifierKey();
			return GetState(modifierKey).HasFlag(modifierKey);
		}

		static ModifierKeys ToModifierKey(this Key key)
		{
			// You may think this function do not belong here, however it is platform dependent.
			// Control key is both ControlKey and Metakey on Windows
			// While on Mac control key is only ControlKey, while Command is both OSKey and Metakey.
			switch (key)
			{
				case Key.ControlKey:
					return ModifierKeys.ControlKey;
				case Key.MetaKey:
					return ModifierKeys.MetaKey;
				case Key.AltKey:
					return ModifierKeys.AltKey;
				case Key.ShiftKey:
					return ModifierKeys.ShiftKey;
			}

			return 0;
		}

		[DllImport("user32.dll")]
		static extern short GetKeyState(Keys vKey);

		static Keys ToKeys(this ModifierKeys key)
		{
			switch (key)
			{
				case ModifierKeys.AltKey:
					return Keys.Alt;

				case ModifierKeys.ControlKey:
					return Keys.Control;

				case ModifierKeys.MetaKey:
					return Keys.Control;

				case ModifierKeys.ShiftKey:
					return Keys.Shift;
			}
			throw new ArgumentException();
		}

		static Keys ToKeys(this MouseButtons button)
		{
			switch (button)
			{
				case MouseButtons.Left:
					return Keys.LButton;

				case MouseButtons.Middle:
					return Keys.MButton;

				case MouseButtons.Right:
					return Keys.RButton;

				case MouseButtons.X1:
					return Keys.XButton1;

				case MouseButtons.X2:
					return Keys.XButton2;
			}
			throw new ArgumentException();
		}
	}*/
}
