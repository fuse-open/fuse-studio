using Uno;
using Uno.Collections;
using System.IO;

namespace Outracks.Simulator.Bytecode
{
	public sealed class ProjectBytecode
	{
		public readonly Lambda Reify;
		public readonly ImmutableList<ProjectDependency> Dependencies;
		public readonly ProjectMetadata Metadata;

		public ProjectBytecode(
			Lambda reify, 
			IEnumerable<ProjectDependency> dependencies,
			ProjectMetadata metadata)
		{
			Reify = reify;
			Dependencies = dependencies.ToImmutableList();
			Metadata = metadata;
		}

		public override string ToString()
		{
			return Reify.ToString();
		}

		public void WriteDataTo(BinaryWriter writer)
		{
			Metadata.WriteDataTo(writer);
			Lambda.Write(Reify, writer);
			List.Write(writer, Dependencies, (Action<ProjectDependency, BinaryWriter>)ProjectDependency.Write);
		}

		public static ProjectBytecode ReadDataFrom(BinaryReader reader)
		{
			var metadata = ProjectMetadata.ReadDataFrom(reader);
			var lambda = Lambda.Read(reader);
			var dependencies = List.Read(reader, (Func<BinaryReader, ProjectDependency>)ProjectDependency.Read);
			return new ProjectBytecode(lambda, dependencies, metadata);
		}

	}
}