using Uno;
using Uno.Net;
using Uno.Net.Sockets;
using Uno.Collections;
using Outracks.Simulator;
using Uno.Threading;
using Outracks;
using System.IO;
using Outracks.Simulator.Client;
using Outracks.Simulator.Protocol;
using Outracks.Simulator.Runtime;
using Outracks.Simulator.Bytecode;

namespace Fuse.Simulator
{
	class ProjectRuntime : IDisposable
	{
		public readonly ISimulatorClient Client;
		public readonly int ProjectId;

		public bool IsDisposed { get; private set; }

		public ProjectRuntime(int projectId, ISimulatorClient client)
		{
			ProjectId = projectId;
			Client = client;
		}

		public void Dispose()
		{
			Client.Dispose();
			IsDisposed = true;
		}
	}

	static class SimulatorManager
	{
		static readonly Dictionary<int,ProjectRuntime> _projectRuntimes = new Dictionary<int,ProjectRuntime>();
		public static readonly RecentProjects RecentProjects = new RecentProjects(new BundleManager());

		static int _uniqueId;

		static IReflection _reflection;
		public static IReflection Reflection
		{
			get
			{
				if(_reflection == null)
					_reflection = CreateReflection();
				return _reflection;
			}
		}

		static IReflection CreateReflection()
		{
			if defined(DotNet)
				return new DotNetReflection();
			if defined(CPLUSPLUS)
				return new NativeReflection(new SimpleTypeMap());
		}

		public static int AddProjectRuntime(int projectId, ISimulatorClient client)
		{
			_projectRuntimes[++_uniqueId] = new ProjectRuntime(projectId, client);
			return _uniqueId;
		}

		public static ProjectRuntime GetProjectRuntime(int id)
		{
			return _projectRuntimes[id];
		}

		public static int RestartRuntime(int id)
		{
			var oldRuntime = _projectRuntimes[id];
			KillClient(id);
			var newClient = oldRuntime.Client.Clone();
			return AddProjectRuntime(oldRuntime.ProjectId, newClient);
		}

		public static void KillClient(int id)
		{
			if(!_projectRuntimes.ContainsKey(id))
				return;

			var runtime = _projectRuntimes[id];
			if(runtime.IsDisposed)
				return;

			runtime.Dispose();
			Logger.Info("Client was killed: " + id);
		}
	}
}