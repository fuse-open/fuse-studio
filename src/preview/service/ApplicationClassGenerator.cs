using System.Collections.Generic;
using System.Linq;
using System.Net;
using Outracks.IO;
using Outracks.IPC;
using Outracks.Simulator;
using Outracks.Simulator.Bytecode;
using Outracks.Simulator.Protocol;

namespace Fuse.Preview
{
	public static class ApplicationClassGenerator
	{
		public static string CreateApplicationClass(BuildProject args, string projectName,  TypeName generatedClassName)
		{
			return CreateApplicationClass(
				NetworkHelper
					.GetInterNetworkIps()
					.Select(ip => new IPEndPoint(ip, 12124)),
				AbsoluteFilePath.Parse(args.ProjectPath),
				args.Defines,
				projectName,
				args.OutputDir,
				generatedClassName);
		}

		public static IEnumerable<string> Dependencies
		{
			get
			{
				yield return "Fuse.Preview.Core";
				yield return "Outracks.Simulator";
				yield return "UnoCore";
				yield return "Uno.Collections";
				yield return "Uno.Net.Sockets";
			}
		}

		static string CreateApplicationClass(
			IEnumerable<IPEndPoint> proxyEndpoints,
			AbsoluteFilePath projectPath,
			IEnumerable<string> defines,
			string projectName,
			string outputDir,
			TypeName generatedClassName)
		{
			return DefaultUsings.Select(n => "using " + n.FullName + ";").Join("\n") +
@"
namespace " + generatedClassName.ContainingType.Value.FullName + @"
{
	public class " + generatedClassName.Name + @" : Outracks.Simulator.Client.Application
	{
		public GeneratedApplication()
			: base(
				new []
				{" +
					proxyEndpoints.Select(proxy => "new Uno.Net.IPEndPoint(Uno.Net.IPAddress.Parse(\"" + proxy.Address + "\"), " + proxy.Port + ")").Join(", ") +
				@"}," +
				"\"" + projectPath.NativePath.Replace("\\", "\\\\") + "\"," +
				@"new string[]
				{ " +
					defines.Select(d => "\"" + d + "\"").Join(", ") +
				@"})
		{
			"+"Runtime.Bundle.Initialize(\""+ projectName +"\");\n" + @"
			if defined(DotNet)
				Reflection = new DotNetReflectionWrapper(DotNetReflection.Load(" + "\"" + outputDir.Replace("\\", "/") + "\"" + @"));
			if defined(CPLUSPLUS)
				Reflection = new NativeReflection(new SimpleTypeMap());
		}
	}
}";
		}

		static IEnumerable<NamespaceName> DefaultUsings
		{
			get
			{
				yield return new NamespaceName("Uno");
				yield return new NamespaceName("Uno.Collections");
				yield return new NamespaceName("Uno.UX");
				yield return new NamespaceName("Uno.IO");
				yield return new NamespaceName("Outracks.Simulator");
				yield return new NamespaceName("Outracks.Simulator.Bytecode");
				yield return new NamespaceName("Outracks.Simulator.Runtime");
				yield return new NamespaceName("Outracks.Simulator.Client");
			}
		}

	}

}

