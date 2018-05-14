using OpenGL;
using Outracks.Fusion.Windows;
using Uno;
using Uno.Platform;

namespace Outracks.UnoHost.Windows
{
	using Fuse;

	static class WinFormsMarshalling
	{
		public static Uno.Platform.MouseButton ToUno(this System.Windows.Forms.MouseButtons buttons)
		{
			if (buttons.HasFlag(System.Windows.Forms.MouseButtons.Left)) return Uno.Platform.MouseButton.Left;
			if (buttons.HasFlag(System.Windows.Forms.MouseButtons.Right)) return Uno.Platform.MouseButton.Right;
			if (buttons.HasFlag(System.Windows.Forms.MouseButtons.Middle)) return Uno.Platform.MouseButton.Middle;
			if (buttons.HasFlag(System.Windows.Forms.MouseButtons.XButton1)) return Uno.Platform.MouseButton.X1;
			if (buttons.HasFlag(System.Windows.Forms.MouseButtons.XButton2)) return Uno.Platform.MouseButton.X2;
			return 0;
		}


		public static void Clear(this IGL gl, System.Windows.Media.Color color, float depth)
		{
			gl.ClearColor(
				((float)color.R) / 255,
				((float)color.G) / 255,
				((float)color.B) / 255,
				((float)color.A) / 255);
			gl.ClearDepth(depth);
			gl.Clear(GLClearBufferMask.ColorBufferBit | GLClearBufferMask.DepthBufferBit);
		}


		public static Point<Pixels> ToPoint(this System.Drawing.Point point)
		{
			return new Point<Pixels>(point.X, point.Y);
		}

		public static Size<Pixels> ToSize(this System.Drawing.Size size)
		{
			return new Size<Pixels>(size.Width, size.Height);
		}

		public static MouseButton ToMouseButton(this System.Windows.Forms.MouseButtons mb)
		{
			switch (mb)
			{
				default:
					// TODO: report warning ?
				case System.Windows.Forms.MouseButtons.Left:
					return MouseButton.Left;
				case System.Windows.Forms.MouseButtons.Middle:
					return MouseButton.Middle;
				case System.Windows.Forms.MouseButtons.Right:
					return MouseButton.Right;
				case System.Windows.Forms.MouseButtons.XButton1:
					return MouseButton.X1;
				case System.Windows.Forms.MouseButtons.XButton2:
					return MouseButton.X2;
			}
		}

		public static void SetCursor(this System.Windows.Forms.Control control, Cursor cursor)
		{
			if (cursor == Cursor.None)
				System.Windows.Forms.Cursor.Hide();
			else
			{
				control.Cursor = cursor.ToCursor();
				System.Windows.Forms.Cursor.Show();
			}
		}

		public static System.Windows.Forms.Cursor ToCursor(this Cursor cursor)
		{
			switch (cursor)
			{
				case Cursor.Default: return System.Windows.Forms.Cursors.Default;
				case Cursor.Pointer: return System.Windows.Forms.Cursors.Default;
				case Cursor.Text: return System.Windows.Forms.Cursors.Default;
				case Cursor.SizeH: return System.Windows.Forms.Cursors.SizeWE;
				case Cursor.SizeV: return System.Windows.Forms.Cursors.SizeNS;
				case Cursor.Grab: return NativeResources.GrabCursor;
				case Cursor.Grabbing: return NativeResources.GrabbingCursor;
			}
			return null;
		}

		public static Size<Pixels> ToPixelSize(this System.Drawing.Size size)
		{
			return new Size<Pixels>(size.Width, size.Height);
		}

		public static Rectangle<Pixels> ToPixelRectangle(this System.Drawing.Rectangle rect)
		{
			return Rectangle.FromSides<Pixels>(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		public static Optional<Key> ToKey(this System.Windows.Forms.Keys key)
		{
			switch (key)
			{
				case System.Windows.Forms.Keys.Back: return Key.Backspace;

				case System.Windows.Forms.Keys.RWin:
				case System.Windows.Forms.Keys.LWin:
					return Key.OSKey;

				case System.Windows.Forms.Keys.Tab: return Key.Tab;
				case System.Windows.Forms.Keys.Enter: return Key.Enter;

				case System.Windows.Forms.Keys.ShiftKey: return Key.ShiftKey;
				case System.Windows.Forms.Keys.ControlKey: return Key.ControlKey;
				case System.Windows.Forms.Keys.Alt: return Key.AltKey;

				case System.Windows.Forms.Keys.CapsLock: return Key.CapsLock;
				case System.Windows.Forms.Keys.Escape: return Key.Escape;
				case System.Windows.Forms.Keys.Space: return Key.Space;
				case System.Windows.Forms.Keys.PageUp: return Key.PageUp;
				case System.Windows.Forms.Keys.PageDown: return Key.PageDown;
				case System.Windows.Forms.Keys.End: return Key.End;
				case System.Windows.Forms.Keys.Home: return Key.Home;

				case System.Windows.Forms.Keys.Left: return Key.Left;
				case System.Windows.Forms.Keys.Up: return Key.Up;
				case System.Windows.Forms.Keys.Right: return Key.Right;
				case System.Windows.Forms.Keys.Down: return Key.Down;

				case System.Windows.Forms.Keys.Insert: return Key.Insert;
				case System.Windows.Forms.Keys.Delete: return Key.Delete;

				//case WinForms.Keys.Oemplus: return Key.Plus;
				//case WinForms.Keys.OemMinus: return Key.Minus;

				//case WinForms.Keys.Add: return Key.Add;
				//case WinForms.Keys.Subtract: return Key.Subtract;
			}

			if (System.Windows.Forms.Keys.D0 <= key && key <= System.Windows.Forms.Keys.D9)
				return Key.D0 + (key - System.Windows.Forms.Keys.D0);

			if (System.Windows.Forms.Keys.A <= key && key <= System.Windows.Forms.Keys.Z)
				return Key.A + (key - System.Windows.Forms.Keys.A);

			if (System.Windows.Forms.Keys.NumPad0 <= key && key <= System.Windows.Forms.Keys.NumPad9)
				return Optional.None();

			if (System.Windows.Forms.Keys.F1 <= key && key <= System.Windows.Forms.Keys.F12)
				return Key.F1 + (key - System.Windows.Forms.Keys.F1);


			return Optional.None();
		}
		
	
	}
}