using System;
using System.IO;

namespace Outracks.Simulator.Protocol
{
	public class BuildProject : IBinaryMessage
	{
		public static readonly string MessageType = "BuildProject";
		public string Type { get { return MessageType; } }

		public Guid Id { get; set; }
		public string ProjectPath { get; private set; }
		public ImmutableList<string> Defines { get; private set; }
		public bool BuildLibraries { get; private set; }
		public bool Verbose { get; private set; }
		public string OutputDir { get; set; }

		public BuildProject(
			string projectPath,
			ImmutableList<string> defines,
			bool buildLibraries,
			bool verbose,
			string outputDir = "")
		{
			ProjectPath = projectPath;
			Defines = defines;
			BuildLibraries = buildLibraries;
			Verbose = verbose;
			OutputDir = outputDir;
		}
			
		public void WriteDataTo(BinaryWriter writer)
		{
			writer.WriteGuid(Id);
			writer.Write(ProjectPath);
			List.Write(writer, Defines, (Action<string, BinaryWriter>)WriteDefine);
			writer.Write(BuildLibraries);
			writer.Write(Verbose);
			writer.Write(OutputDir);
		}

		public static BuildProject ReadDataFrom(BinaryReader reader)
		{
			var id = reader.ReadGuid();
			var projectPath = reader.ReadString();
			var defines = List.Read(reader, (Func<BinaryReader, string>)ReadDefine);
			var buildLibraries = reader.ReadBoolean();
			var verbose = reader.ReadBoolean();
			var outputDir = reader.ReadString();
			return new BuildProject(projectPath, defines, buildLibraries, verbose, outputDir)
			{
				Id = id,
			};
		}

		static void WriteDefine(string str, BinaryWriter writer)
		{
			writer.Write(str);
		}

		static string ReadDefine(BinaryReader reader)
		{
			return reader.ReadString();
		}

	}
}