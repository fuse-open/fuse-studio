using System;
using System.Collections.Generic;
using System.Linq;
using Outracks.IO;
using System.Collections.Immutable;

namespace Outracks.Fuse.Designer
{
	class ProjectData : IEquatable<ProjectData>
	{
		public readonly string Name;
		public readonly AbsoluteFilePath ProjectPath;
		public readonly DateTime? LastOpened; //Optional doesn't work well with user settings, so use Nullable instead

		public ProjectData(string name, AbsoluteFilePath projectPath, DateTime? lastOpened)
		{
			Name = name;
			ProjectPath = projectPath;
			LastOpened = lastOpened;
		}

		public bool Equals(ProjectData other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(Name, other.Name) 
				&& Equals(ProjectPath, other.ProjectPath)
				&& Equals(LastOpened, other.LastOpened);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((ProjectData)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Name != null ? Name.GetHashCode() : 0;
				hashCode = (hashCode * 397) ^ (ProjectPath != null ? ProjectPath.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (LastOpened == null ? LastOpened.GetHashCode() : 0);
				return hashCode;
			}
		}

		public static bool operator ==(ProjectData left, ProjectData right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(ProjectData left, ProjectData right)
		{
			return !Equals(left, right);
		}


		public static ImmutableList<ProjectData> ExistingProjects(IEnumerable<ProjectData> pd)
		{
			var shell = new Shell();
			return pd.Where(p => shell.Exists(p.ProjectPath)).ToImmutableList();
		}
	}

}