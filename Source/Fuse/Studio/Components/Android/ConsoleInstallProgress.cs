using System;
using Outracks.AndroidManager;

namespace Outracks.Fuse
{
	class ConsoleInstallProgress : IProgress<InstallerEvent>
	{
		public void Report(InstallerEvent value)
		{
			Console.WriteLine(value);
		}
	}
}