using System;
using Outracks.Diagnostics;
using Outracks.IO;

namespace Outracks.Fuse
{
	public class FuseKiller : IFuseKiller
	{
		readonly IFuseKiller _impl;
		public FuseKiller(IReport log, AbsoluteDirectoryPath fuseRoot)
		{
			if (Platform.IsMac)
			{
				_impl = new MacFuseKiller(log, fuseRoot);
			}
			else if(Platform.IsWindows)
			{
				_impl = new WinFuseKiller(log, fuseRoot);
			}
			else
				throw new PlatformNotSupportedException();
		}

		public void Execute(ColoredTextWriter console)
		{
			_impl.Execute(console);
		}
	}
}