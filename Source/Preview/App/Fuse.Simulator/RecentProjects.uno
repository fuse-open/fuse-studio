using Uno.IO;
using Uno.Collections;
using Outracks.Simulator.Bytecode;
using Outracks.Simulator.Protocol;
using Uno;

namespace Fuse.Simulator
{
	class BytecodeCache
	{
		public readonly ProjectBytecode Bytecode;
		public readonly BytecodeUpdated []BytecodeUpdates;

		public BytecodeCache(ProjectBytecode bytecode, BytecodeUpdated []bytecodeUpdates)
		{
			Bytecode = bytecode;
			BytecodeUpdates = bytecodeUpdates;
		}

		public void WriteDataTo(System.IO.BinaryWriter writer)
		{
			Bytecode.WriteDataTo(writer);
			writer.Write(BytecodeUpdates.Length);
			for(var i = 0;i < BytecodeUpdates.Length;++i)
			{
				BytecodeUpdates[i].WriteDataTo(writer);
			}
		}

		public static BytecodeCache ReadDataFrom(System.IO.BinaryReader reader)
		{
			var bytecode = ProjectBytecode.ReadDataFrom(reader);
			var length = reader.ReadInt32();
			var updates = new BytecodeUpdated[length];
			for(var i = 0;i < length;++i)
			{
				updates[i] = BytecodeUpdated.ReadDataFrom(reader);
			}
			return new BytecodeCache(bytecode, updates);
		}
	}

	class Project
	{
		public readonly int Identifier;
		public readonly string Name;
		public readonly string CachePath;
		
		public Project(int identifier, string name, string cachePath)
		{
			Identifier = identifier;
			Name = name;
			CachePath = cachePath;
		}

		public override string ToString()
		{
			return "Project: { Identifier: " + Identifier + ", Name: " + Name + ", CachePath: " + CachePath + " }";
		}
	}

	class RecentProjects
	{
		readonly string _configPath;
		readonly Dictionary<int, Project> _recentProjects = new Dictionary<int, Project>();
		readonly BundleManager _bundleManager;

		public IEnumerable<Project> Projects
		{
			get { return _recentProjects.Values; }
		}

		public event Action<IEnumerable<Project>> RecentProjectsChanged;

		public RecentProjects(BundleManager bundleManager)
		{
			_bundleManager = bundleManager;
			_configPath = Path.Combine(BundleManager.GetDataDir(), "recentProjects");
			TryLoadRecentList(_configPath);
		}

		public Project AddProject(string name)
		{
			var identifier = NextIdentifier();
			var cachePath = identifier.ToString();
			var project = new Project(identifier, name, cachePath);
			_recentProjects[identifier] = project;
			SaveRecentList(_configPath);
			OnRecentProjectsChanged();
			return project;
		}

		void SaveRecentList(string configPath)
		{
			try
			{
				var stream = File.Open(configPath, FileMode.Create);
				using(var writer = new StreamWriter(stream))
				{
					writer.WriteLine(Projects.Count());
					foreach(var project in Projects)
					{
						writer.WriteLine(project.Identifier);
						writer.WriteLine(project.Name);
						writer.WriteLine(project.CachePath);
					}
				}
			}
			catch(Exception e)
			{
				Logger.Error("Failed to save recent projects: + " + e.ToString());
			}
		}

		void TryLoadRecentList(string configPath)
		{
			try
			{
				var stream = File.OpenRead(configPath);
				using(var reader = new StreamReader(stream))
				{
					int length = int.Parse(reader.ReadLine());
					for(var i = 0;i < length;++i)
					{
						var project = new Project(int.Parse(reader.ReadLine()), reader.ReadLine(), reader.ReadLine());
						_recentProjects[project.Identifier] = project;
					}
				}
			}
			catch(Exception e)
			{
				Logger.Error("Failed to load recent projects: + " + e.ToString());
			}
		}

		public bool IsCached(Project project)
		{
			return _bundleManager.HasBundle(project.CachePath);
		}

		public void UpdateCache(Project project, BytecodeCache bc)
		{
			OnRecentProjectsChanged();
			_bundleManager.CreateCopyOfBundleTo(_bundleManager.GetBundle(), project.CachePath, bc);
		}

		public BytecodeCache GetCache(Project project)
		{
			return _bundleManager.UpdateBundleFrom(_bundleManager.GetBundle(), project.CachePath);
		}

		public Project GetProject(int identifier)
		{
			return _recentProjects[identifier];
		}

		void OnRecentProjectsChanged()
		{
			if(RecentProjectsChanged != null)
				RecentProjectsChanged(Projects);
		}

		public int NextIdentifier()
		{
			int id = 0;
			foreach(var key in _recentProjects.Keys)
			{
				id = Math.Max(id, key);
			}
			return ++id;
		}
	}
}