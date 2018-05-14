using System.IO;
using Uno;
using Uno.Collections;

namespace Outracks.Simulator.Bytecode
{
	public sealed class ProjectDependency : IEquatable<ProjectDependency>
	{
		public readonly string Path;
		public readonly string Descriptor;

		public ProjectDependency(string path, string descriptor)
		{
			Path = path;
			Descriptor = descriptor;
		}

		public static void Write( ProjectDependency dependency, BinaryWriter writer)
		{
			writer.Write(dependency.Path);
			writer.Write(dependency.Descriptor);
		}

		public static ProjectDependency Read(BinaryReader reader)
		{
			var path = reader.ReadString();
			var descriptor = reader.ReadString();
			return new ProjectDependency(path, descriptor);
		}

		public bool Equals(ProjectDependency other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Path.Equals(other.Path) && string.Equals(Descriptor, other.Descriptor);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is ProjectDependency && Equals((ProjectDependency)obj);
		}

		public override int GetHashCode()
		{
			//unchecked
			{
				return (Path.GetHashCode() * 397) ^ Descriptor.GetHashCode();
			}
		}

		public static bool operator ==(ProjectDependency left, ProjectDependency right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(ProjectDependency left, ProjectDependency right)
		{
			return !Equals(left, right);
		}

		public override string ToString()
		{
			return "{ Path: " + Path + ", Descriptor: " + Descriptor + " }";
		}
	}
	
}