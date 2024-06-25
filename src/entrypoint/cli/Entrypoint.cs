using System;
using System.Linq;

namespace Outracks.Fuse
{
	public static class Entrypoint
	{
		[STAThread]
		public static int Main(string[] cmdArgs)
		{
			return Program.Run(cmdArgs.ToList());
		}
	}

}
