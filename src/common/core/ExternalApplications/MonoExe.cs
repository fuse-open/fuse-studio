using System;
using System.Diagnostics;
using System.IO;
using Uno;

namespace Outracks.IO
{
	[Obsolete("Starting Mono does not work well in Xamarin.Mac apps.")]
	class MonoExe : IExternalApplication
	{
		readonly AbsoluteFilePath _exeFile;
		readonly Optional<AbsoluteFilePath> _mono;

		public MonoExe(AbsoluteFilePath exeFile, Optional<AbsoluteFilePath> mono)
		{
			_exeFile = exeFile;
			_mono = mono;
		}

		public Process Start(Optional<ProcessStartInfo> startInfo)
		{
			var newStartInfo = startInfo.HasValue ? startInfo.Value : new ProcessStartInfo();

			newStartInfo.FileName = _mono.Select(m => m.NativePath).Or("/Library/Frameworks/Mono.framework/Commands/mono");
			newStartInfo.Arguments = _exeFile.NativePath.QuoteSpace() + " " + newStartInfo.Arguments;

			return Process.Start(newStartInfo);
		}

		public Process Open(IAbsolutePath fileName, Optional<ProcessStartInfo> startInfo)
		{
			var newStartInfo = startInfo.Or(new ProcessStartInfo());
			newStartInfo.Arguments = fileName.NativePath.QuoteSpace() + " " + newStartInfo.Arguments;

			return Start(newStartInfo);
		}

		public string Name
		{
			get { return _exeFile.Name.ToString(); }
		}

		public bool Exists
		{
			get { return File.Exists(_exeFile.NativePath); }
		}
	}
}
