using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using WpfKey = System.Windows.Input.Key;
using WpfModifier = System.Windows.Input.ModifierKeys;

namespace Outracks.Fusion.Windows
{
	public static class Marshalling
	{
		public static Point<Points> ToFusion(this System.Windows.Point point)
		{
			return new Point<Points>(point.X, point.Y);
		}

		public static Size<Points> ToFusion(this System.Windows.Size size)
		{
			return Size.Create<Points>(size.Width, size.Height);
		}

		public static System.Windows.Size ToWpf(this Size<Points> size)
		{
			return new System.Windows.Size(size.Width, size.Height);
		}

		public static System.Windows.TextAlignment ToWpf(this TextAlignment textAlignment)
		{
			switch (textAlignment)
			{
				case TextAlignment.Left: return System.Windows.TextAlignment.Left;
				case TextAlignment.Right: return System.Windows.TextAlignment.Right;
				case TextAlignment.Center: return System.Windows.TextAlignment.Center;
			}
			throw new NotImplementedException();
		}

		public static Transform ToWpfScaleRotation(this Matrix matrix)
		{
			return new MatrixTransform(matrix.M11, matrix.M12, matrix.M21, matrix.M22, 0, 0);
		}

		/*
		public static IObservable<Brush> ToBrush(this Fusion.Brush brush, Dispatcher dispatcher)
		{
			return brush.Color.Switch(color => dispatcher.InvokeAsync(() => new SolidColorBrush(color.ToColor())));
		}
		*/

		public static System.Windows.Media.Color ToColor(this Color value)
		{
			return new System.Windows.Media.Color()
			{
				R = (byte) (value.R * 255),
				G = (byte) (value.G * 255),
				B = (byte) (value.B * 255),
				//A = (byte) (value.A * 254 + 1) // Using opacity of 0 in WPF seems to cause different click-through behavior when in a transparent vs opaque window
				A = (byte) (value.A * 255)
			};
		}
		public static Color ToFusion(this System.Windows.Media.Color value)
		{
			return Color.FromBytes(
				r: value.R,
				g: value.G,
				b: value.B,
				a: value.A);
		}

		public static DoubleCollection ToDashArray(this StrokeDashArray value)
		{
			var result = new DoubleCollection(value.Data.Length);
			foreach(var val in value.Data)
				result.Add(val);

			return result;
		}

		public static HotKey ToHotKey(this InputGestureCollection gestures)
		{
			if (gestures.Count < 1)
				return HotKey.None;

			var gesture = gestures[0] as KeyGesture;
			if (gesture == null)
				return HotKey.None;

			return HotKey.Create(gesture.Modifiers.ToFusion(), gesture.Key.ToFusion());
		}

		public static KeyGesture ToWpfGesture(this HotKey hotkey)
		{
			var key = hotkey.Key.ToWpfKey();
			var modiferKeys = hotkey.Modifier.ToWpfModifierKeys();
			return new KeyGesture(key.Value, modiferKeys, key.Value.ToShortcutString(modiferKeys));
		}

		public static ModifierKeys ToFusion(this WpfModifier modifier)
		{
			var key = ModifierKeys.None;
			if (modifier.HasFlag(WpfModifier.Alt)) key |= ModifierKeys.Alt;
			if (modifier.HasFlag(WpfModifier.Control)) key |= ModifierKeys.Control;
			if (modifier.HasFlag(WpfModifier.Shift)) key |= ModifierKeys.Shift;
			if (modifier.HasFlag(WpfModifier.Windows)) key |= ModifierKeys.Windows; //throw new NotImplementedException("Windows modifier key"); // key |= EtoKey.Windows;
			return key;
		}

		public static WpfModifier ToWpfModifierKeys(this ModifierKeys keys)
		{
			var key = WpfModifier.None;
			if (keys.HasFlag(ModifierKeys.Alt)) key |= WpfModifier.Alt;
			if (keys.HasFlag(ModifierKeys.Control)) key |= WpfModifier.Control;
			if (keys.HasFlag(ModifierKeys.Shift)) key |= WpfModifier.Shift;
			if (keys.HasFlag(ModifierKeys.Windows)) key |= WpfModifier.Windows; //throw new NotImplementedException("Windows modifier key"); // key |= EtoKey.Windows;
			if (keys.HasFlag(ModifierKeys.Command)) key |= WpfModifier.Windows; //throw new NotImplementedException("Command modifier key"); // key |= EtoKey.Command;
			if (keys.HasFlag(ModifierKeys.Meta)) key |= WpfModifier.Control;
			return key;
		}

		public static Optional<WpfKey> ToWpfKey(this Key key)
		{
			switch (key)
			{
				case Key.Backspace: return WpfKey.Back;

				case Key.Tab: return WpfKey.Tab;
				case Key.Enter: return WpfKey.Enter;

				case Key.LeftShift: return WpfKey.LeftShift;
				case Key.RightShift: return WpfKey.RightShift;
				case Key.LeftCtrl: return WpfKey.LeftCtrl;
				case Key.RightCtrl: return WpfKey.RightCtrl;
				case Key.LeftAlt: return WpfKey.LeftAlt;
				case Key.RightAlt: return WpfKey.RightAlt;

				case Key.CapsLock: return WpfKey.CapsLock;
				case Key.Escape: return WpfKey.Escape;
				case Key.Space: return WpfKey.Space;
				case Key.PageUp: return WpfKey.PageUp;
				case Key.PageDown: return WpfKey.PageDown;
				case Key.End: return WpfKey.End;
				case Key.Home: return WpfKey.Home;

				case Key.Left: return WpfKey.Left;
				case Key.Up: return WpfKey.Up;
				case Key.Right: return WpfKey.Right;
				case Key.Down: return WpfKey.Down;

				case Key.Insert: return WpfKey.Insert;
				case Key.Delete: return WpfKey.Delete;

				case Key.Plus: return WpfKey.OemPlus;
				case Key.Minus: return WpfKey.OemMinus;

				case Key.Add: return WpfKey.Add;
				case Key.Subtract: return WpfKey.Subtract;
			}

			if (Key.D0 <= key && key <= Key.D9)
				return WpfKey.D0 + (key - Key.D0);

			if (Key.A <= key && key <= Key.Z)
				return WpfKey.A + (key - Key.A);

			if (Key.NumPad0 <= key && key <= Key.NumPad9)
				return Optional.None();

			if (Key.F1 <= key && key <= Key.F12)
				return WpfKey.F1 + (key - Key.F1);

			return Optional.None();
		}

		public static Key ToFusion(this WpfKey key)
		{
			switch (key)
			{
				case WpfKey.Back: return Key.Backspace;

				case WpfKey.Tab: return Key.Tab;
				case WpfKey.Enter: return Key.Enter;

				case WpfKey.LeftShift: return Key.LeftShift;
				case WpfKey.RightShift: return Key.RightShift;
				case WpfKey.LeftCtrl: return Key.LeftCtrl;
				case WpfKey.RightCtrl: return Key.RightCtrl;
				case WpfKey.LeftAlt: return Key.LeftAlt;
				case WpfKey.RightAlt: return Key.RightAlt;

				case WpfKey.CapsLock: return Key.CapsLock;
				case WpfKey.Escape: return Key.Escape;
				case WpfKey.Space: return Key.Space;
				case WpfKey.PageUp: return Key.PageUp;
				case WpfKey.PageDown: return Key.PageDown;
				case WpfKey.End: return Key.End;
				case WpfKey.Home: return Key.Home;

				case WpfKey.Left: return Key.Left;
				case WpfKey.Up: return Key.Up;
				case WpfKey.Right: return Key.Right;
				case WpfKey.Down: return Key.Down;

				case WpfKey.Insert: return Key.Insert;
				case WpfKey.Delete: return Key.Delete;

				case WpfKey.OemPlus: return Key.Plus;
				case WpfKey.OemMinus: return Key.Minus;

				case WpfKey.Add: return Key.Add;
				case WpfKey.Subtract: return Key.Subtract;
			}

			if (WpfKey.D0 <= key && key <= WpfKey.D9)
				return Key.D0 + (key - WpfKey.D0);

			if (WpfKey.A <= key && key <= WpfKey.Z)
				return Key.A + (key - WpfKey.A);

			if (WpfKey.NumPad0 <= key && key <= WpfKey.NumPad9)
				return Key.D0 + (key - WpfKey.D0);

			if (WpfKey.F1 <= key && key <= WpfKey.F12)
				return Key.F1 + (key - WpfKey.F1);

			return Key.None;
		}

		static void AppendSeparator(StringBuilder sb, string separator, string value)
		{
			if (sb.Length > 0)
				sb.Append(separator);
			sb.Append(value);
		}

		static readonly Dictionary<WpfKey, string> keymap = new Dictionary<WpfKey, string> {
			{ WpfKey.D0, "0" },
			{ WpfKey.D1, "1" },
			{ WpfKey.D2, "2" },
			{ WpfKey.D3, "3" },
			{ WpfKey.D4, "4" },
			{ WpfKey.D5, "5" },
			{ WpfKey.D6, "6" },
			{ WpfKey.D7, "7" },
			{ WpfKey.D8, "8" },
			{ WpfKey.D9, "9" },

			{ WpfKey.Subtract, "-" },
			{ WpfKey.Add, "+" },
			{ WpfKey.OemPlus, "+" },
			{ WpfKey.OemMinus, "-" },
			{ WpfKey.Divide, "/" },
			{ WpfKey.OemPeriod, "." },
			{ WpfKey.OemBackslash, "\\" },

			{ WpfKey.OemSemicolon, ";" }
		};

		public static string ToShortcutString(this WpfKey key, WpfModifier modifier, string separator = "+")
		{
			var sb = new StringBuilder();
			if (modifier.HasFlag(WpfModifier.Control))
				AppendSeparator(sb, separator, "Ctrl");
			if (modifier.HasFlag(WpfModifier.Shift))
				AppendSeparator(sb, separator, "Shift");
			if (modifier.HasFlag(WpfModifier.Alt))
				AppendSeparator(sb, separator, "Alt");

			string val;
			if (keymap.TryGetValue(key, out val))
				AppendSeparator(sb, separator, val);
			else
				AppendSeparator(sb, separator, key.ToString());

			return sb.ToString();
		}
	}
}