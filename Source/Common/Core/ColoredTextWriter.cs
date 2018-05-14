using System;
using System.IO;
using System.Text;

namespace Outracks
{
	public class ColoredTextWriter : TextWriter
	{
		public static ColoredTextWriter Out = new ColoredTextWriter(
			Console.Out,
			c =>
			{
				if (c.HasValue)
					Console.ForegroundColor = c.Value;
				else
					Console.ResetColor();
			});

		public static ColoredTextWriter Error = new ColoredTextWriter(
			Console.Error,
			c =>
			{
				if (c.HasValue)
					Console.ForegroundColor = c.Value;
				else
					Console.ResetColor();
			});


		readonly TextWriter _textWriter;
		readonly Action<ConsoleColor?> _setColor;

		ConsoleColor? _currentColor;

		public ColoredTextWriter(TextWriter textWriter, Action<ConsoleColor?> setColor)
		{
			_textWriter = textWriter;
			_setColor = setColor;
		}

		public IDisposable PushColor(ConsoleColor? color)
		{
			var oldColor = _currentColor;
			_setColor(color);
			_currentColor = color;
			return Disposable.Create(
				() =>
				{
					_setColor(oldColor);
					_currentColor = oldColor;
				});
		}

		#region Delegation to _textWriter

		public override void Write(string value)
		{
			_textWriter.Write(value);
		}

		public override void WriteLine()
		{
			_textWriter.WriteLine();
		}

		public override void WriteLine(char value)
		{
			_textWriter.WriteLine(value);
		}

		public override void WriteLine(char[] buffer)
		{
			_textWriter.WriteLine(buffer);
		}

		public override void WriteLine(char[] buffer, int index, int count)
		{
			_textWriter.WriteLine(buffer, index, count);
		}

		public override void WriteLine(bool value)
		{
			_textWriter.WriteLine(value);
		}

		public override void WriteLine(int value)
		{
			_textWriter.WriteLine(value);
		}

		public override void WriteLine(uint value)
		{
			_textWriter.WriteLine(value);
		}

		public override void WriteLine(long value)
		{
			_textWriter.WriteLine(value);
		}

		public override void WriteLine(ulong value)
		{
			_textWriter.WriteLine(value);
		}

		public override void WriteLine(float value)
		{
			_textWriter.WriteLine(value);
		}

		public override void WriteLine(double value)
		{
			_textWriter.WriteLine(value);
		}

		public override void WriteLine(decimal value)
		{
			_textWriter.WriteLine(value);
		}

		[System.Security.SecuritySafeCritical]  // auto-generated 
		public override void WriteLine(String value)
		{
			_textWriter.WriteLine(value);
		}

		public override void WriteLine(Object value)
		{
			_textWriter.WriteLine(value);
		}

		public override void WriteLine(String format, Object arg0)
		{
			_textWriter.WriteLine(String.Format(FormatProvider, format, arg0));
		}

		public override void WriteLine(String format, Object arg0, Object arg1)
		{
			_textWriter.WriteLine(String.Format(FormatProvider, format, arg0, arg1));
		}

		public override void WriteLine(String format, Object arg0, Object arg1, Object arg2)
		{
			_textWriter.WriteLine(String.Format(FormatProvider, format, arg0, arg1, arg2));
		}

		public override void WriteLine(String format, params Object[] arg)
		{
			_textWriter.WriteLine(String.Format(FormatProvider, format, arg));
		} 

		public override void Flush()
		{
			_textWriter.Flush();
		}

		public override void Close()
		{
			_textWriter.Close();
		}

		public override void Write(char c)
		{
			_textWriter.Write(c);
		}

		public override Encoding Encoding
		{
			get { return _textWriter.Encoding; }
		}

		#endregion
	}
}