using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Uno.Build;
using Uno.Compiler.Backends.CIL;

namespace Outracks.Simulator.Client
{
	public class DotNetBuild
	{
		public static string SaveMetadata(BuildResult buildResult)
		{
			var cilBackend = (CilResult)buildResult.BackendResult;
			var assemblyPath = GetDllPath(buildResult);
			
			var metadata = new DotNetBuild(
				assembly: Path.GetFileName(assemblyPath),
				entrypointClass: buildResult.Compiler.Data.MainClass.FullName,
				generatedTypeNames:
					cilBackend.AllUnoTypeNames.ToDictionary(
						keySelector: name => name,
						elementSelector: cilBackend.GetCilTypeFromUnoName));

			using (var metadataFile = File.OpenWrite(GetMetadataPath(buildResult.OutputDirectory)))
				metadata.WriteTo(metadataFile);

			return assemblyPath;
		}

		public static string GetDllPath(BuildResult buildResult)
		{
			var cilBackend = (CilResult)buildResult.BackendResult;
			var outputDir = buildResult.OutputDirectory;
			var assemblyName = cilBackend.AssemblyName;
			var dllFilename = assemblyName + ".dll";
			return Path.Combine(outputDir, dllFilename);
		}

		public static string GetMetadataPath(string outputDir)
		{
			return Path.Combine(outputDir, "metadata.json");
		}

		public static DotNetBuild LoadMetadata(string outputDir)
		{
			using (var stream = File.OpenRead(GetMetadataPath(outputDir)))
				return ReadFrom(stream);
		}

		public static DotNetBuild ReadFrom(Stream stream)
		{
			return Parse(JToken.ReadFrom(new JsonTextReader(new StreamReader(stream))));
		}

		public static DotNetBuild Parse(JToken json)
		{
			return new DotNetBuild(
				assembly: json["Assembly"].ToObject<string>(),
				entrypointClass: json["EntrypointClass"].ToObject<string>(),
				generatedTypeNames: json["GeneratedTypeNames"].ToObject<Dictionary<JToken, JToken>>().ToDictionary(
					kv => kv.Key.ToObject<string>(),
					kv => kv.Value.ToObject<string>()));
		}

		public void WriteTo(Stream stream)
		{
			using (var writer = new JsonTextWriter(new StreamWriter(stream)))
				Serialize().WriteTo(writer);
		}

		public JObject Serialize()
		{
			return new JObject
			{
				{ "Assembly", JToken.FromObject(Assembly) },
				{ "EntrypointClass", JToken.FromObject(EntrypointClass) },
				{ "GeneratedTypeNames", JToken.FromObject(GeneratedTypeNames) },
			};
		}

		public readonly string Assembly;
		public readonly string EntrypointClass;
		public readonly IDictionary<string, string> GeneratedTypeNames;

		public DotNetBuild(
			string assembly, 
			string entrypointClass,
			IDictionary<string, string> generatedTypeNames)
		{
			Assembly = assembly;
			EntrypointClass = entrypointClass;
			GeneratedTypeNames = generatedTypeNames;
		}
	}
}