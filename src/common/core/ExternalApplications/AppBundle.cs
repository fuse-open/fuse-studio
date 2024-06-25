using System.Diagnostics;
using System.IO;
using Uno;

namespace Outracks.IO
{
	class AppBundle : IExternalApplication
	{
		readonly IAbsolutePath _appBundle;

		public AppBundle(IAbsolutePath appBundle)
		{
			_appBundle = appBundle;
		}

		public Process Start(Optional<ProcessStartInfo> startInfo)
		{
			var newStartInfo = startInfo.HasValue ? startInfo.Value : new ProcessStartInfo();

			newStartInfo.FileName = "open";
			newStartInfo.Arguments = _appBundle.NativePath.QuoteSpace() + " --args " + newStartInfo.Arguments;

			return Process.Start(newStartInfo);
		}

		public Process Open(IAbsolutePath fileName, Optional<ProcessStartInfo> startInfo)
		{
			var newStartInfo = startInfo.Or(new ProcessStartInfo());

			newStartInfo.FileName = "open";
			newStartInfo.Arguments = "-a " + _appBundle.NativePath.QuoteSpace() + " " + fileName.NativePath.QuoteSpace() + " --args " + newStartInfo.Arguments;

			return Process.Start(newStartInfo);
		}

		public string Name
		{
			get { return _appBundle.Name; }
		}

		public bool Exists
		{
			get { return Directory.Exists(_appBundle.NativePath); }
		}
	}
}
