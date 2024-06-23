using System.IO;
using System.Text;
using Outracks.UnoDevelop.CodeNinja;

namespace Outracks.Fuse.CodeAssistance
{
	public class Logger : ILog
	{
		public StringBuilder _sb = new StringBuilder();
		public void Clear() {}

		public TextWriter TextWriter
		{
			get
			{
				return new StringWriter(_sb);
			}
		}

		public string Text
		{
			get { return _sb.ToString(); }
		}

		public void Show() {}
		public void Mute() {}
		public void Unmute() {}
	}
}