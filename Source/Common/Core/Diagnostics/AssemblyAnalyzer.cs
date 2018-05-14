using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Outracks
{
	public static class AssemblyAnalyzer
	{
		public static void EnsureSameArchitectureInLoadedNonMSILAssemblies()
		{
			AppDomain
				.CurrentDomain
				.GetAssemblies()
				.Where(IsArchitectureDependent)
				.EnsureSameArchitecture();
		}

		public static bool IsArchitectureDependent(this Assembly assembly)
		{
			var architecture = assembly.GetArchitecture();
			return architecture != ProcessorArchitecture.MSIL && architecture != ProcessorArchitecture.None;
		}

		public static void EnsureSameArchitecture(this IEnumerable<Assembly> assembliesu)
		{
			var assemblies = assembliesu.ToArray();
			if (assemblies.Length < 2) return;

			var referenceAssembly = assemblies[0];
			var wantedArchitecture = referenceAssembly.GetArchitecture();

			var assembliesWithWrongArchiecture =
				assemblies
				.Where(asm => asm.GetArchitecture() != wantedArchitecture)
				.ToArray();

			if (assembliesWithWrongArchiecture.Length != 0)
				throw new Exception(
					"The following assemblies was not built with same architecture as " + referenceAssembly.FullName + " (" + wantedArchitecture + "): \n" +
					assembliesWithWrongArchiecture.Select(asm => asm.FullName + " (" + asm.GetArchitecture() + ")").Join("\n"));
		}

		public static ProcessorArchitecture GetArchitecture(this Assembly assembly)
		{
			return assembly.GetName().ProcessorArchitecture;
		}
	}
}