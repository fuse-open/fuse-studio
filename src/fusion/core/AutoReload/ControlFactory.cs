using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using Outracks.IO;

namespace Outracks.Fusion.AutoReload
{
	public static class ControlFactory
	{
		public static Optional<Assembly> Compile(IEnumerable<AbsoluteFilePath> files, IProgress<string> log, IEnumerable<string> referencedAssemblies)
		{
			var provider = new CSharpCodeProvider();
			var parameters = new CompilerParameters();

			parameters.ReferencedAssemblies.AddRange(referencedAssemblies.ToArray());
			parameters.GenerateInMemory = true;
			parameters.GenerateExecutable = false;

			log.Report("Begin auto-reload\n");
			var results = provider.CompileAssemblyFromFile(parameters, files.Select(f => f.NativePath).ToArray());
			if (results.Errors.HasErrors)
			{
				foreach (var errors in results.Errors.OfType<CompilerError>())
				{
					log.Report("Error at line " + errors.Line + ": " + errors.ErrorText + "\n");
				}
				return Optional.None();
			}

			return results.CompiledAssembly;
		}

		public static Optional<IControl> CreateControl(Assembly assembly, string factoryMethodName, object[] args, IProgress<string> log)
		{
			var contentCreateMethod = assembly
				.GetTypes()
				.Select(x => x.GetMethod(factoryMethodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic))
				.Where(x => x != null)
				.FirstOrDefault(x => x.ReturnType == typeof(IControl));

			if (contentCreateMethod == null)
			{
				log.Report("Unable to find class that has a public static '" + factoryMethodName + "' method\n");
				return Optional.None();
			}
			log.Report("Loading type " + contentCreateMethod.DeclaringType.FullName + "\n");

			var content = (IControl)contentCreateMethod
				.Invoke(null, args);

			return Optional.Some(content);
		}
	}
}