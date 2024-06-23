using System.Windows.Forms;
using System.Windows.Media;
using OpenGL;
using Outracks.Fusion.Windows;
using Uno.Platform;

namespace Outracks.UnoHost.Windows
{
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


		public static void Clear(this IGL gl, Color color, float depth)
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

		public static void SetCursor(this Control control, Cursor cursor)
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
				case Cursor.Default: return Cursors.Default;
				case Cursor.Pointer: return Cursors.Default;
				case Cursor.Text: return Cursors.Default;
				case Cursor.SizeH: return Cursors.SizeWE;
				case Cursor.SizeV: return Cursors.SizeNS;
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

		public static Optional<Key> ToKey(this Keys key)
		{
			switch (key)
			{
				case Keys.Back: return Key.Backspace;

				case Keys.RWin:
				case Keys.LWin:
					return Key.OSKey;

				case Keys.Tab: return Key.Tab;
				case Keys.Enter: return Key.Enter;

				case Keys.ShiftKey: return Key.ShiftKey;
				case Keys.ControlKey: return Key.ControlKey;
				case Keys.Alt: return Key.AltKey;

				case Keys.CapsLock: return Key.CapsLock;
				case Keys.Escape: return Key.Escape;
				case Keys.Space: return Key.Space;
				case Keys.PageUp: return Key.PageUp;
				case Keys.PageDown: return Key.PageDown;
				case Keys.End: return Key.End;
				case Keys.Home: return Key.Home;

				case Keys.Left: return Key.Left;
				case Keys.Up: return Key.Up;
				case Keys.Right: return Key.Right;
				case Keys.Down: return Key.Down;

				case Keys.Insert: return Key.Insert;
				case Keys.Delete: return Key.Delete;

				//case WinForms.Keys.Oemplus: return Key.Plus;
				//case WinForms.Keys.OemMinus: return Key.Minus;

				//case WinForms.Keys.Add: return Key.Add;
				//case WinForms.Keys.Subtract: return Key.Subtract;
			}

			if (Keys.D0 <= key && key <= Keys.D9)
				return Key.D0 + (key - Keys.D0);

			if (Keys.A <= key && key <= Keys.Z)
				return Key.A + (key - Keys.A);

			if (Keys.NumPad0 <= key && key <= Keys.NumPad9)
				return Optional.None();

			if (Keys.F1 <= key && key <= Keys.F12)
				return Key.F1 + (key - Keys.F1);


			return Optional.None();
		}


	}
}