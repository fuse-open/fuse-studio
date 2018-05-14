using System;
using System.Diagnostics;
using System.Reflection;
using Outracks.Diagnostics;

namespace Outracks
{
	public static class SystemInfoFactory
	{
		public static Version GetBuildVersion()
		{
			try
			{
				return GetVersion(Assembly.GetExecutingAssembly());
			}
			catch (Exception)
			{
				return new Version(0, 0, 0, 0);
			}
		}

		static Version GetVersion(Assembly assembly)
		{
			try
			{
				var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
				var version = fileVersionInfo.ProductVersion;
				return new Version(version);
			}
			catch (Exception)
			{
				return new Version(0,0,0,0);
			}
		}
	}
}
