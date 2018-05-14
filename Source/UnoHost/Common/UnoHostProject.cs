using System;
using System.Reflection;
using Outracks.Simulator.Bytecode;
using Outracks.Simulator.Client;

namespace Outracks.UnoHost
{
	using IO;

	public class UnoHostProject
	{
		public static UnoHostProject Load(AbsoluteFilePath metadataFile, IFileSystem fileSystem)
		{
			using (var file = fileSystem.OpenRead(metadataFile))
				return new UnoHostProject(metadataFile, DotNetBuild.ReadFrom(file));
		}

		readonly AbsoluteDirectoryPath _metadataDir;
		readonly DotNetBuild _build;
		readonly Lazy<Assembly> _assembly;

		public UnoHostProject (AbsoluteFilePath metadataPath, DotNetBuild build)
		{
			_build = build;
			_metadataDir = metadataPath.ContainingDirectory;
			_assembly = new Lazy<Assembly>(() => Assembly.LoadFrom((_metadataDir / build.Assembly).NativePath));
		}

		public void ExecuteStartupCode()
		{
			try
			{
				LoadFrom(TypeName.Parse(_build.EntrypointClass), _assembly.Value)
					.GetConstructor(new Type[0])
					.Invoke(new object[0]);
			}
			catch (TargetInvocationException tie)
			{
				tie.InnerException.RethrowWithStackTrace();
				throw tie.InnerException;
			}
		}

		static Type LoadFrom(TypeName typeName, Assembly assembly)
		{
			if (typeName.IsParameterizedGenericType)
				throw new Exception("Type is a parameterized generic type");

			var name = typeName.FullName;
			var type = assembly.GetType(name);
			if (type == null)
				throw new Exception("Could not load type " + typeName.FullName);
			return type;
		}

	}
}