using System;
using System.IO;
using System.Text;
using Outracks;

namespace Fuse.Preview
{
	public class TextWriterAdapter : TextWriter
	{
		readonly Guid _buildId;
		readonly IProgress<IBinaryMessage> _dst;

		public TextWriterAdapter(Guid buildId, IProgress<IBinaryMessage> dst)
		{
			_buildId = buildId;
			_dst = dst;
		}

		public override void Write(char value)
		{
			Write(new string(value, 1));
		}

		public override void WriteLine(string value)
		{
			Write(value + "\n");
		}

		public override void WriteLine()
		{
			Write("\n");
		}

		public override void Write(string value)
		{
			// TODO: fix all colors
			_dst.Report(new BuildLogged(
				value, 
				System.Console.ForegroundColor == System.ConsoleColor.Red 
					? Optional.Some(ConsoleColor.Red) 
					: Optional.None<ConsoleColor>(), 
				_buildId));
		}

		public override Encoding Encoding
		{
			get { return Encoding.UTF8; }
		}

	}
}