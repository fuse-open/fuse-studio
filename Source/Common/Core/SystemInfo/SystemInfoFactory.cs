using System;
using System.Diagnostics;
using System.Reflection;
using Outracks.Diagnostics;

namespace Outracks
{
	public static class SystemInfoFactory
	{
		public static string GetBuildVersion(Assembly assembly)
		{
			return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0-dev";
		}
	}
}
