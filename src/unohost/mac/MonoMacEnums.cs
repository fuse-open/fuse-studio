using System;
using System.Linq;
using AppKit;
using Uno.Platform;

namespace Outracks.UnoHost.Mac
{
	public static class MonoMacEnums
	{
		public static Optional<Key> InterpretAsKeyEvent(this NSEvent theEvent)
		{
			return ((NSKey)theEvent.KeyCode).ToKey()
				.Or(((NSFunctionKey)theEvent.KeyCode).ToKey());
		}

		public static Optional<string> InterpretAsTextEvent(this NSEvent theEvent)
		{
			if (!IsTextInputEvent (theEvent.ModifierFlags))
				return Optional.None();

			var characters = theEvent.Characters.Where (CharacterIsNotSpecial).ToArray();
			if (characters.Length == 0)
				return Optional.None();

			return new String(characters);
		}

		static bool IsTextInputEvent(NSEventModifierMask modifierFlags)
		{
			return (modifierFlags & NSEventModifierMask.CommandKeyMask) == 0;
		}

		static bool CharacterIsNotSpecial(char character)
		{
			return char.IsLetterOrDigit (character) || char.IsSymbol (character) || char.IsWhiteSpace (character) || char.IsPunctuation (character) || char.IsNumber (character) || char.IsSeparator (character);
		}

		/*
		public static NSEventModifierMask ToNSEventModifierMask(this ModifierKeys keys)
		{
			var mask = (NSEventModifierMask)0;

			if (keys.HasFlag (ModifierKeys.Alt))
				mask |= NSEventModifierMask.AlternateKeyMask;
			if (keys.HasFlag (ModifierKeys.Command))
				mask |= NSEventModifierMask.CommandKeyMask;
			if (keys.HasFlag (ModifierKeys.Control))
				mask |= NSEventModifierMask.ControlKeyMask;
			if (keys.HasFlag (ModifierKeys.Shift))
				mask |= NSEventModifierMask.ShiftKeyMask;

			return mask;
		}

		public static ModifierKeys ToModifierKeys(this NSEventModifierMask mask)
		{
			var keys = ModifierKeys.None;

			if (mask.HasFlag (NSEventModifierMask.CommandKeyMask))
				keys |= ModifierKeys.Command | ModifierKeys.Meta;
			if (mask.HasFlag (NSEventModifierMask.ControlKeyMask))
				keys |= ModifierKeys.Control;
			if (mask.HasFlag (NSEventModifierMask.ShiftKeyMask))
				keys |= ModifierKeys.Shift;
			if (mask.HasFlag (NSEventModifierMask.AlternateKeyMask))
				keys |= ModifierKeys.Alt;

			return keys;
		}
		*/
		public static MouseButtons ToMouseButtons(this uint nsButtons)
		{
			var buttons = MouseButtons.None;

			if ((nsButtons & (1 << 0)) != 0)
				buttons |= MouseButtons.Left;
			if ((nsButtons & (1 << 1)) != 0)
				buttons |= MouseButtons.Right;
			if ((nsButtons & (1 << 2)) != 0)
				buttons |= MouseButtons.Middle;
			// TODO: maybe incorrect
			return buttons;
		}
		/*
		public static string ToKeyEquivalent (this Key key)
		{
			switch (key)
			{
				case Key.None: return "";
				case Key.Space: return " ";
				case Key.D0: return "0";
				case Key.D1: return "1";
				case Key.D2: return "2";
				case Key.D3: return "3";
				case Key.D4: return "4";
				case Key.D5: return "5";
				case Key.D6: return "6";
				case Key.D7: return "7";
				case Key.D8: return "8";
				case Key.D9: return "9";
				case Key.Plus: return "+";
				case Key.Minus: return "-";
				case Key.NumPad0: return "0";
				case Key.NumPad1: return "1";
				case Key.NumPad2: return "2";
				case Key.NumPad3: return "3";
				case Key.NumPad4: return "4";
				case Key.NumPad5: return "5";
				case Key.NumPad6: return "6";
				case Key.NumPad7: return "7";
				case Key.NumPad8: return "8";
				case Key.NumPad9: return "9";
				case Key.Add: return "+";
				case Key.Subtract: return "-";
			}
			return key.ToString ().ToLower ();
		}
		*/

		public static Optional<Key> ToKey (this NSKey key)
		{
			switch (key)
			{
				case NSKey.Delete:
					return Key.Backspace;
				case NSKey.Tab:
					return Key.Tab;
				case NSKey.Return:
					return Key.Enter;
				case NSKey.Shift:
				case NSKey.RightShift:
					return Key.ShiftKey;
				case NSKey.Control:
				case NSKey.RightControl:
					return Key.ControlKey;

				case NSKey.CapsLock:
					return Key.CapsLock;

				case NSKey.Escape:
					return Key.Escape;

				case NSKey.Space:
					return Key.Space;

				case NSKey.PageUp:
					return Key.PageUp;

				case NSKey.PageDown:
					return Key.PageDown;

				case NSKey.End:
					return Key.End;

				case NSKey.Home:
					return Key.Home;

				case NSKey.LeftArrow:
					return Key.Left;

				case NSKey.UpArrow:
					return Key.Up;

				case NSKey.RightArrow:
					return Key.Right;

				case NSKey.DownArrow:
					return Key.Down;

				case NSKey.ForwardDelete:
					return Key.Delete;

				case NSKey.D0:
					return Key.D0;

				case NSKey.D1:
					return Key.D1;

				case NSKey.D2:
					return Key.D2;

				case NSKey.D3:
					return Key.D3;

				case NSKey.D4:
					return Key.D4;

				case NSKey.D5:
					return Key.D5;

				case NSKey.D6:
					return Key.D6;

				case NSKey.D7:
					return Key.D7;

				case NSKey.D8:
					return Key.D8;

				case NSKey.D9:
					return Key.D9;

				case NSKey.A:
					return Key.A;

				case NSKey.B:
					return Key.B;

				case NSKey.C:
					return Key.C;

				case NSKey.D:
					return Key.D;

				case NSKey.E:
					return Key.E;

				case NSKey.F:
					return Key.F;

				case NSKey.G:
					return Key.G;

				case NSKey.H:
					return Key.H;

				case NSKey.I:
					return Key.I;

				case NSKey.J:
					return Key.J;

				case NSKey.K:
					return Key.K;

				case NSKey.L:
					return Key.L;

				case NSKey.M:
					return Key.M;

				case NSKey.N:
					return Key.N;

				case NSKey.O:
					return Key.O;

				case NSKey.P:
					return Key.P;

				case NSKey.Q:
					return Key.Q;

				case NSKey.R:
					return Key.R;

				case NSKey.S:
					return Key.S;

				case NSKey.T:
					return Key.T;

				case NSKey.U:
					return Key.U;

				case NSKey.V:
					return Key.V;

				case NSKey.W:
					return Key.W;

				case NSKey.X:
					return Key.X;

				case NSKey.Y:
					return Key.Y;
				case NSKey.Z:
					return Key.Z;

				case NSKey.Keypad0:
					return Key.NumPad0;

				case NSKey.Keypad1:
					return Key.NumPad1;

				case NSKey.Keypad2:
					return Key.NumPad2;

				case NSKey.Keypad3:
					return Key.NumPad3;

				case NSKey.Keypad4:
					return Key.NumPad4;

				case NSKey.Keypad5:
					return Key.NumPad5;

				case NSKey.Keypad6:
					return Key.NumPad6;

				case NSKey.Keypad7:
					return Key.NumPad7;

				case NSKey.Keypad8:
					return Key.NumPad8;

				case NSKey.Keypad9:
					return Key.NumPad9;

				case NSKey.F1:
					return Key.F1;

				case NSKey.F2:
					return Key.F2;

				case NSKey.F3:
					return Key.F3;

				case NSKey.F4:
					return Key.F4;

				case NSKey.F5:
					return Key.F5;

				case NSKey.F6:
					return Key.F6;

				case NSKey.F7:
					return Key.F7;

				case NSKey.F8:
					return Key.F8;

				case NSKey.F9:
					return Key.F9;

				case NSKey.F10:
					return Key.F10;

				case NSKey.F11:
					return Key.F11;

				case NSKey.F12:
					return Key.F12;

				case NSKey.Command:
					return Key.OSKey;

				default:
					return Optional.None();
			}
		}

		public static Optional<Key> ToKey(this NSFunctionKey key)
		{
			switch (key)
			{
				case NSFunctionKey.Menu:
					return Key.AltKey;

				case NSFunctionKey.Break:
					return Key.Break;

				case NSFunctionKey.Insert:
					return Key.Insert;

				default:
					return Optional.None();
			}
		}
	}
}

