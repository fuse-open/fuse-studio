using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace Fuse.Installer.Gui.Domain
{
	public class Logger
	{
		public Logger()
		{
			_vcredistlogTxt = "VcRedistLog.txt";
			if (File.Exists(_vcredistlogTxt))
				File.AppendAllText(_vcredistlogTxt, "ThreadId: " + System.Threading.Thread.CurrentThread.ManagedThreadId + "----------------------Reset-------------------\r\n");

			File.WriteAllText(_vcredistlogTxt, "Clean\r\n");
		}

		static Logger _logger;
		string _vcredistlogTxt;

		public static Logger Instance
		{
			get
			{
				return _logger ?? (_logger = new Logger());
			}
		}

		public void Trace(string message,
		                  [CallerMemberName] string memberName = "",
		                  [CallerFilePath] string sourceFilePath = "",
		                  [CallerLineNumber] int sourceLineNumber = 0)
		{
			/*using (var streamWriter = File.AppendText(_vcredistlogTxt))
			{
				streamWriter.Write(DateTime.Now.ToString(CultureInfo.InvariantCulture));
				streamWriter.Write("\t" + Path.GetFileNameWithoutExtension(sourceFilePath));
				streamWriter.Write("\t" + memberName);
				streamWriter.Write("\t" + sourceLineNumber);
				streamWriter.Write("\t " + message);
				streamWriter.WriteLine();
			}*/
		}
	}
}