using System.IO;
using System.Text;

namespace Outracks.Fuse.CodeAssistanceService
{
	public class Logger : Outracks.UnoDevelop.CodeNinja.ILog
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