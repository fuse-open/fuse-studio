using Uno;
using Fuse.Scripting;
using Uno.Net;
using Uno.Net.Sockets;
using Uno.Threading;
using Uno.IO;
using Outracks;
using Outracks.Simulator.Protocol;
using Outracks.Simulator.Runtime;
using Uno.Collections;

namespace Fuse.Simulator 
{
	delegate T ResultFactory<T>(object[] args);
	class FactoryClosure<T>
	{
		ResultFactory<T> _factory;
		object[] _args;
		Promise<T> _promise;

		public FactoryClosure(ResultFactory<T> factory, object[] args, Promise<T> promise)
		{
			_factory = factory;
			_args = args;
			_promise = promise;
		}

		public void Run()
		{
			T res = default(T);
			try
			{
				res = _factory(_args);
			}
			catch (Exception e)
			{
				_promise.Reject(e);
				return;
			}

			_promise.Resolve(res);
		}
	}

	class RecentProjectsCallback
	{
		readonly Context _ctx;
		readonly Function _callback;
		Project[] _projects;

		public RecentProjectsCallback(Context ctx, Function callback)
		{
			_ctx = ctx;
			_callback = callback;
		}

		public void Update(IEnumerable<Project> projects)
		{
			_projects = FilterOnlyCachedProjects(projects);
			_ctx.Invoke(UpdateJS);
		}

		Project[] FilterOnlyCachedProjects(IEnumerable<Project> projects)
		{
			var projectsCached = new List<Project>();
			foreach(var proj in projects)
			{
				if(SimulatorManager.RecentProjects.IsCached(proj))
					projectsCached.Add(proj);
			}
			return projectsCached.ToArray();
		}

		void UpdateJS(Context ctx)
		{
			_callback.Call(ctx, Converter(ctx, _projects));
		}

		object Converter(Context c, Project []project)
		{
			var projObjs = new List<object>();
			foreach(var proj in project)
			{
				projObjs.Add(CreateProjectObject(proj, c));
			}
			return c.NewArray(projObjs.ToArray());
		}

		static Fuse.Scripting.Object CreateProjectObject(Project project, Context ctx)
		{
			var o = ctx.NewObject();
			o["id"] = project.Identifier;
			o["name"] = project.Name;
			return o;
		}
	}

	public class SimulatorJS : NativeModule
	{
		RecentProjectsCallback _recentProjectsCallback;
		public SimulatorJS()
		{
			AddMember(new NativePromise<ProxyConnection,object>("connectToProxy", ConnectToProxy, Converter));
			AddMember(new NativeFunction("listenForRecentProjects", ListenForRecentProjects));
			AddMember(new NativePromise<ClientInfo,object>("openProject", OpenProject, Converter));
			AddMember(new NativePromise<int,object>("restartProjectRuntime", RestartProjectRuntime, Converter));
			AddMember(new NativeFunction("requestScreenOrientation", RequestScreenOrientation));
		}

		ProxyConnection ConnectToProxy(object[] args)
		{
			if(args[0] is Fuse.Scripting.Array)
			{
				var rawAddresses = (Fuse.Scripting.Array)args[0];
				var addresses = new IPAddress[rawAddresses.Length];
				for(var i = 0;i < addresses.Length;++i)
				{
					addresses[i] = IPAddress.Parse((string)rawAddresses[i]);
				}

				return ProxyConnection.Connect(addresses);
			}
			else
			{
				return ProxyConnection.Connect(IPAddress.Parse((string)args[0]));
			}
		}

		object ListenForRecentProjects(Context c, object[] args)
		{
			var callback = (Function)args[0];
			var recentProjects = SimulatorManager.RecentProjects;
			if(_recentProjectsCallback != null)
				recentProjects.RecentProjectsChanged -= _recentProjectsCallback.Update;
			
			_recentProjectsCallback = new RecentProjectsCallback(c, callback);
			recentProjects.RecentProjectsChanged += _recentProjectsCallback.Update;
			_recentProjectsCallback.Update(recentProjects.Projects);
			return null;
		}

		Future<ClientInfo> OpenProject(object[] args)
		{
			var promise = new Promise<ClientInfo>();
			UpdateManager.PostAction(new FactoryClosure<ClientInfo>(OpenProjectInternal, args, promise).Run);
			return promise;
		}

		ClientInfo OpenProjectInternal(object[] args)
		{
			var id = Marshal.ToInt(args[0]);
			var proj = SimulatorManager.RecentProjects.GetProject(id);
			var bc = SimulatorManager.RecentProjects.GetCache(proj);
			return new ClientInfo(proj.Name, SimulatorManager.AddProjectRuntime(id, new OfflineClient(bc)));
		}

		object RequestScreenOrientation(Context c, object[] args)
		{
			var orientationRaw = (string)args[0];
			ScreenOrientationType orientation;
			if(!Enum.TryParse(orientationRaw, true, out orientation))
			{
				throw new Exception("Invalid orientation identifier: " + orientationRaw);
			}
			UpdateManager.PostAction(Outracks.Simulator.Closure.Apply<ScreenOrientationType>(ScreenOrientation.Request, orientation));
			return null;
		}

		Future<int> RestartProjectRuntime(object[] args)
		{
			var promise = new Promise<int>();
			UpdateManager.PostAction(new FactoryClosure<int>(RestartProjectRuntimeInternal, args, promise).Run);
			return promise;
		}

		int RestartProjectRuntimeInternal(object[] args)
		{
			return SimulatorManager.RestartRuntime(Marshal.ToInt(args[0]));
		}

		object Converter(Context c, ProxyConnection owner)
		{
			return c.Unwrap(owner);
		}

		object Converter(Context c, ClientInfo owner)
		{
			return owner.CreateScriptObject(c);
		}

		object Converter(Context c, int owner)
		{
			return owner;
		}
	}

	class ProxyConnection
	{
		readonly Socket _socket;
		readonly NetworkStream _networkStream;
		
		static ProxyConnection()
		{
			ScriptClass.Register(typeof(ProxyConnection),
				new ScriptPromise<ProxyConnection,ClientInfo,Fuse.Scripting.Object>("connectToHost", ExecutionThread.MainThread, ConnectToHost, Converter),
				new ScriptMethod<ProxyConnection>("disconnect", Disconnect));
		}

		ProxyConnection(Socket socket)
		{
			_socket = socket;
			_networkStream = new NetworkStream(_socket);
		}

		public static ProxyConnection Connect(IPAddress address)
		{
			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Connect(ToProxyEndpoint(address));
			return new ProxyConnection(socket);
		}

		public static ProxyConnection Connect(IPAddress []addresses)
		{
			try
			{
				var socket = ConnectToFirstRespondingEndpoint.Execute(addresses.Select(ToProxyEndpoint)).Result;
				return new ProxyConnection(socket);
			}
			catch(AggregateException e)
			{
				if(e.InnerExceptions != null && e.InnerExceptions.Count > 0)
					throw e.InnerExceptions.First();
				else
					throw;
			}
		}

		static IPEndPoint ToProxyEndpoint(IPAddress address)
		{
			return new IPEndPoint(address, 12124);
		}

		static void Disconnect(ProxyConnection owner, object []args)
		{
			owner._socket.Dispose();
		}

		static Future<ClientInfo> ConnectToHost(Context context, ProxyConnection owner, object []args)
		{
			var promise = new Promise<ClientInfo>();
			try
			{
				var code = (string)args[0];
				promise.Resolve(owner.ConnectToHostInner(code));
			}
			catch(Exception e)
			{
				promise.Reject(e);
			}
			return promise;
		}

		ClientInfo ConnectToHostInner(string code)
		{
			var writer = new System.IO.BinaryWriter(_networkStream);
			var reader = new System.IO.BinaryReader(_networkStream);
			writer.Write(code);
			writer.Write(""); // TODO: This is defines!

			var initialState = reader.ReadString();

			if("SUCCESS".Equals(initialState) == false)
				throw new Exception("Failed to request a preview host");

			if ("DESIGNER_NOT_RUNNING".Equals(initialState))
				throw new Exception("Incorrect code");
			
			var endpointCount = reader.ReadInt32();
			var endpoints = new IPEndPoint[endpointCount];
			for (int i = 0; i < endpoints.Length; i++)
			{
				var simulatorAddress = reader.ReadString();
				var simulatorPort = reader.ReadInt32();
				endpoints[i] = new IPEndPoint(IPAddress.Parse(simulatorAddress), simulatorPort);
			}

			var projectName = reader.ReadString();
			Logger.Info("Project Name: " + projectName);
			foreach(var endpoint in endpoints)
			{
				Logger.Info("Endpoint to host: " + endpoint.ToString());
			}
			
			var packageCount = reader.ReadInt32();
			for(var i = 0;i < packageCount;++i)
			{
				var name = reader.ReadString();
				var version = reader.ReadString();
				Logger.Info("Package: " + name + ", " + version);
			}

			var projectCount = reader.ReadInt32();
			for(var i = 0;i < projectCount;++i)
			{
				var name = reader.ReadString();
				Logger.Info("Project: " + name);
			}

			var hostSocket = ConnectToFirstRespondingEndpoint.Execute(endpoints).Result;
			Logger.Info("Made connection to an host!");
			
			var project = FindRecentProjectOrCreate(projectName); // TODO: Base it on a unique identifier...
			return new ClientInfo(projectName, SimulatorManager.AddProjectRuntime(project.Identifier, new SimulatorClient(hostSocket)));
		}

		static Project FindRecentProjectOrCreate(string projectName)
		{
			var recentProjects = SimulatorManager.RecentProjects;

			foreach(var proj in recentProjects.Projects)
			{
				if(proj.Name == projectName)
					return proj;
			}

			return recentProjects.AddProject(projectName);
		}

		static Fuse.Scripting.Object Converter(Context c, ClientInfo owner)
		{
			return owner.CreateScriptObject(c);
		}
	}

	class ClientInfo
	{
		readonly int _id;
		readonly string _projectName;

		public ClientInfo(string projectName, int id)
		{
			_id = id;
			_projectName = projectName;
		}

		public Fuse.Scripting.Object CreateScriptObject(Context c)
		{
			var o = c.NewObject();
			o["id"] = _id;
			o["projectName"] = _projectName;
			return o;
		}
	}
}
