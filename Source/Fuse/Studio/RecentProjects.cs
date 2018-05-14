using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Outracks.Fusion;
using Outracks.IO;

namespace Outracks.Fuse.Designer
{
	class ProjectDataPathComparer : IEqualityComparer<ProjectData>
	{
		public bool Equals(ProjectData x, ProjectData y)
		{
			if (ReferenceEquals(x, y))
				return true;

			if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
				return false;

			return x.ProjectPath == y.ProjectPath;
		}

		public int GetHashCode(ProjectData obj)
		{
			return obj.ProjectPath.GetHashCode();
		}
	}

	static class RecentProjects
	{
		static readonly IProperty<Optional<IEnumerable<ProjectData>>> UserSetting =
			UserSettings.List<ProjectData>("RecentProjects");

		public static IObservable<ImmutableList<ProjectData>> All
		{
			get
			{
				return UserSetting.OrEmpty()
					.Select(ProjectData.ExistingProjects)
					.Replay(1).RefCount();
			}
		}

		public static async Task Bump(Fusion.IDocument document, IObservable<string> projectName)
		{
			var filePath = await document.FilePath.NotNone().FirstAsync();
			var list = await All.FirstAsync();
			var name = await projectName.FirstAsync();

			var newList = 
				list.Insert(0, new ProjectData(name, filePath, DateTime.Now))
					.Distinct(new ProjectDataPathComparer());

			UserSetting.Write(Optional.Some(newList), save: true);
		}

		public static async Task Remove(IAbsolutePath path)
		{
			var list = await All.FirstAsync();
			var newList = 
				list.RemoveAll(item => item.ProjectPath.Equals(path))
					.Distinct(new ProjectDataPathComparer());

			UserSetting.Write(Optional.Some(newList), save: true);
		}

	}
}