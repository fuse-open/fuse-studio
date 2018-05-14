using Uno;
using Uno.IO;
using Uno.Net;
using Uno.Net.Sockets;
using Uno.Collections;
using Uno.Compiler.ExportTargetInterop;
using Fuse;
using Uno.Diagnostics;
using Fuse.Controls;
using Fuse.Elements;
using Fuse.Input;

namespace Outracks.Simulator.Client
{
	using Bytecode;
	using Protocol;
	using Runtime;
	using UnoHost;


	// Uninitialized state

	sealed class Uninitialized : State
	{
		public override State OnUpdate()
		{
			if (((Application)Uno.Application.Current).Reflection != null)
				return new ConnectingToProxy();

			return this;
		}

		public override State OnException(Exception e)
		{
			return new Faulted(isOnline: false, exception: e, buttons: new DialogButton("Try again", this));
		}
	}

	// Connecting states

	abstract class Connecting : State
	{
		public override State OnException(Exception e)
		{
			if (e is Outracks.Simulator.DesignerNotRunning)
				return new DesignerNotRunning();
			return new FailedToConnect(e);
		}
	}


	sealed class ConnectingToProxy : Connecting
	{
		readonly IPEndPoint[] _proxyEndpoints;
		Task<IPEndPoint[]> _connecting;

		public ConnectingToProxy(IPEndPoint[] proxyEndpoints)
		{
			_proxyEndpoints = proxyEndpoints;
		}

		public ConnectingToProxy()
		{
			_proxyEndpoints = Context.ProxyEndpoints;
		}

		public override State OnEnterState()
		{
			if defined(!MOBILE)
			{
				var args = Uno.Environment.GetCommandLineArgs();
				var index = Array.IndexOf(args, "--host");
				if (index != -1)
				{
					return new ConnectingToHost(new [] { new IPEndPoint(IPAddress.Parse(args[index+1]), int.Parse(args[index+2])) });
				}
			}

			_connecting = ProxyClient.GetSimulatorEndpoint(_proxyEndpoints, Context.Project, Context.Defines);
			if defined(MOBILE)
			{
				LoadingScreen.Show(Context.App, "Connecting", "Connecting to computer...");
			}
			else
			{	
				LoadingScreen.Show(Context.App, "Loading", "Loading project...");
			}
			return this;
		}

		public override State OnUpdate()
		{
			if (_connecting.IsCompleted)
				return new ConnectingToHost(_connecting.Result);
			
			return this;
		}
	}

	sealed class ConnectingToHost : Connecting
	{
		readonly IPEndPoint[] _simulatorEndpoints;

		public ConnectingToHost(IEnumerable<IPEndPoint> simulatorEndpoints)
		{
			_simulatorEndpoints = simulatorEndpoints.ToArray();
		}

		Task<Socket> _connecting;

		public override State OnEnterState()
		{
			_connecting = ConnectToFirstRespondingEndpoint.Execute(_simulatorEndpoints);
			if defined (MOBILE)
			{
				LoadingScreen.Show(Context.App, "Connecting", "Fetching project data from computer");
			}
			else
			{
				LoadingScreen.Show(Context.App, "Loading", "Loading data...");
			}

			return this;
		}

		public override State OnUpdate()
		{
			if (_connecting.IsCompleted)
			{
				var client = new SimulatorClient(_connecting.Result);
				Uno.Platform.CoreApp.Terminating += Closure.ToAppStateChangeHandler(client.Dispose);
				return new Idle(client);
			}
			
			return this;
		}
	}

	// Connected states

	abstract class Connected : State
	{
		protected readonly ISimulatorClient Client;

		protected Connected(
			ISimulatorClient client)
		{
			Client = client;
		}

		public override State OnEnterState()
		{
			Debug.SetLogHandler(SendDebugLog);
			Client.Send(new RegisterName(DeviceInfo.GUID, DeviceInfo.Name));
			Fuse.Diagnostics.DiagnosticReported += OnDiagnosticReported;
			Fuse.Diagnostics.DiagnosticDismissed += OnDiagnosticDismissed;
			return this;
		}

		public override void OnLeaveState()
		{
			Fuse.Diagnostics.DiagnosticReported -= OnDiagnosticReported;
			Fuse.Diagnostics.DiagnosticDismissed -= OnDiagnosticDismissed;
		}

		public override State OnException(Exception e)
		{
			var exceptions = Exceptions.Unpack(e);
			Exceptions.Send(Client, exceptions);
			var f = exceptions.Count == 1 ? exceptions[0] : e;
			return OnFaulted(f);
		}

		protected abstract State OnFaulted(Exception e);

		public override State OnUpdate()
		{
			IBinaryMessage message;
			while (Client.IncommingMessages.TryDequeue(out message))
			{
				var newState = NextState(message);
				if (newState != this) 
					return newState;
			}

			return this;
		}

		State NextState(IBinaryMessage message)
		{
			var bcg = message.TryParse(BytecodeGenerated.MessageType, (Func<System.IO.BinaryReader, BytecodeGenerated>)BytecodeGenerated.ReadDataFrom);

			if (bcg.HasValue)
				return OnReify(bcg.Value);

			var bu = message.TryParse(BytecodeUpdated.MessageType, (Func<System.IO.BinaryReader, BytecodeUpdated>)BytecodeUpdated.ReadDataFrom);
			if (bu.HasValue)
				return OnExecute(bu.Value);

			var e = message.TryParse(Error.MessageType, (Func<System.IO.BinaryReader, Error>)Error.ReadDataFrom);
			if (e.HasValue)
				return OnConnectionError(e.Value);

			return this;
		}

		protected virtual State OnReify(BytecodeGenerated reify)
		{
			return new Reifying(Client, reify);
		}

		protected virtual State OnExecute(BytecodeUpdated args)
		{
			VirtualMachine.Execute(args.Function);
			return this;
		}

		protected virtual State OnConnectionError(Error error)
		{
			return new ConnectionLost(error.Exception);
		}

		static Dictionary<Fuse.Diagnostic, Protocol.Diagnostic> _activeDiagnostics = new Dictionary<Fuse.Diagnostic, Protocol.Diagnostic>();
		static object _mutex = new object();

		private void OnDiagnosticReported(Fuse.Diagnostic d)
		{
			lock(_mutex)
			{
				var diag = new Protocol.Diagnostic(DeviceInfo.GUID, DeviceInfo.Name, d.Message, d.ToString(), d.FilePath, d.LineNumber, 0);
				Client.Send(diag);
				_activeDiagnostics.Add(d, diag);
			}
		}

		private void OnDiagnosticDismissed(Fuse.Diagnostic d)
		{
			lock(_mutex)
			{
				Protocol.Diagnostic diag;
				if (_activeDiagnostics.TryGetValue(d, out diag))
				{
					Client.Send(new DismissDiagnostic(DeviceInfo.GUID, DeviceInfo.Name, diag.DiagnosticId));
					_activeDiagnostics.Remove(d);
				}
			}
		}

		public void DismissAllActiveDiagnostics()
		{
			lock (_mutex)
			{
				foreach (var k in _activeDiagnostics)
				{
					Client.Send(new DismissDiagnostic(DeviceInfo.GUID, DeviceInfo.Name, k.Value.DiagnosticId));
				}
				_activeDiagnostics.Clear();
			}
		}

		private void SendDebugLog(string text, Uno.Diagnostics.DebugMessageType type)
		{
			Client.Send(new DebugLog(DeviceInfo.GUID, DeviceInfo.Name, text));
		}

	}

	sealed class Idle : Connected
	{
		public Idle(ISimulatorClient client) 
			: base(client) 
		{ }

		public override State OnEnterState()
		{
			base.OnEnterState();
			LoadingScreen.Show(Context.App, "Loading", "Starting project...");
			return this;
		}

		protected override State OnFaulted(Exception e)
		{
			return new Faulted(Client.IsOnline, e, new DialogButton("Try again", this));
		}

	}

	sealed class Reifying : Connected
	{
		readonly BytecodeGenerated _reify;

		public Reifying(ISimulatorClient client, BytecodeGenerated reify)
			: base(client)
		{
			_reify = reify;
		}

		public override State OnEnterState()
		{
			base.OnEnterState();
			DismissAllActiveDiagnostics();

			object previousState = null;
			if (Context.App.RootViewport != null)
				previousState = Context.App.RootViewport.SavePreviewState();

			//prepare for new nodes
			UserAppState.Default.ApplyTo(Context.App);
			if (Context.App.RootViewport != null)
				Context.App.RootViewport.RestorePreviewState(previousState);

			VirtualMachine.Execute(_reify.Bytecode.Reify, Context.App);
			var userState = UserAppState.Save(Context.App);

			return new Running(Client, userState, _reify);
		}

		protected override State OnFaulted(Exception e)
		{
			return new Faulted(Client.IsOnline, e, new DialogButton("Try again", new ConnectingToProxy()));
		}
	}

	// HACK: This avoids missing entity causing https://github.com/fusetools/Fuse/issues/3149
	[Require("Entity", "Uno.UX.SimulatedProperties.Set(Uno.UX.PropertyObject,string,object,Uno.UX.IPropertyListener)")]
	[Require("Entity", "Uno.UX.SimulatedProperties.Get(Uno.UX.PropertyObject,string)")]
	public static class VirtualMachine
	{

		public static object Execute(Lambda lambda, params object[] arguments)
		{
			return Execute(((Application)Uno.Application.Current).Reflection, lambda, arguments);
		}

		public static object Execute(IReflection reflection, Lambda lambda, params object[] arguments)
		{
			return new ScopeClosure(new Outracks.Simulator.Runtime.Environment(Optional.None<Outracks.Simulator.Runtime.Environment>()), reflection).Execute(lambda, arguments);
		}
	}

	sealed class Running : Connected
	{
		readonly BytecodeGenerated _runningReify;
		float _zoomWhenRooted;

		UserAppState _userApp;

		public Running(
			ISimulatorClient client,
			UserAppState userApp,
			BytecodeGenerated runningReify)
			: base(client)
		{
			_userApp = userApp;
			_runningReify = runningReify;
		}

		public override State OnEnterState()
		{
			base.OnEnterState();
			_userApp.ApplyTo(Context.App);
			_zoomWhenRooted = QueryDensity();
			return this;
		}

		public override void OnLeaveState()
		{
			base.OnLeaveState();
			_userApp = UserAppState.Save(Context.App);
		}

		public override State OnUpdate()
		{
			var currentZoom = QueryDensity();
			if (currentZoom != _zoomWhenRooted)
			{
				debug_log "Density changed, reifying";
				return new Reifying(Client, _runningReify);
			}
				
			return base.OnUpdate();
		}

		protected override State OnFaulted(Exception e)
		{
			return new Faulted(Client.IsOnline, e, new DialogButton("Restart", new ConnectingToProxy()));
		}

		static float QueryDensity()
		{
			return Uno.Platform.Displays.MainDisplay.Density;
		}
	}

	internal static class Exceptions
	{
		public static void Send(ISimulatorClient client, List<Exception> exceptions)
		{
			for (var i = 0; i < exceptions.Count; ++i)
			{
				var unpacked = exceptions[i];
				client.Send(new UnhandledException(DeviceInfo.GUID, DeviceInfo.Name, unpacked.Message, unpacked.StackTrace, unpacked.GetType().ToString()));
			}
		}

		public static List<Exception> Unpack(Exception e)
		{
			if (e is AggregateException)
			{
				var aggregate = (AggregateException)e;

				var exceptions = new List<Exception>();
				foreach (var inner in aggregate.InnerExceptions)
					exceptions.AddRange(Unpack(inner));
				return exceptions;
			}
			else if (e.InnerException != null)
			{
				return Unpack(e.InnerException);
			}
			else
			{
				return new List<Exception> { e };
			}
		}
	}

	// Error states

	sealed class Faulted : ShowingModalDialog
	{
		public Faulted(bool isOnline, Exception exception, params DialogButton[] buttons)
			: base(
				"Oops! Something went wrong here.",
				exception.Message + (isOnline ? "\n\nPlease refer to the log for details." : ""),
				"",
				buttons)
		{ }
	}

	sealed class ConnectionLost : ShowingModalDialog 
	{
		public ConnectionLost(Exception exception)
			: this(ExceptionInfo.Capture(exception))
		{ }

		public ConnectionLost(ExceptionInfo exception)
			: base(
				"Connection lost",
				exception.Message,
				"",
				new DialogButton("Change IP", new ChangeIp()),
				new DialogButton("Reconnect", new ConnectingToProxy()))
		{ }
	}

	sealed class FailedToConnect : ShowingModalDialog
	{
		public FailedToConnect(Exception exception)
			: base(
				"Failed to connect",
				"Please check that this device is connected to the same network as your computer.",
				exception.Message,
				new DialogButton("Change IP", new ChangeIp()),
				new DialogButton("Try again", new ConnectingToProxy()))
		{ }
	}

	sealed class DesignerNotRunning : ShowingModalDialog
	{
		public DesignerNotRunning()
			: base(
				"Fuse not running",
				"Please check that this project is open in Fuse on your computer.",
				"",
				new DialogButton("Change IP", new ChangeIp()),
				new DialogButton("Try again", new ConnectingToProxy()))
		{ }
	}

	sealed class ChangeIp : ShowingPrompt
	{
		public ChangeIp(string body = "")
			: base("Connect to IP", body + "\nE.g. '192.168.1.1'")
		{
		}

		protected override State OnOk(string input)
		{
			var endpoints = new[]
			{
				new IPEndPoint(IPAddress.Parse(input), Context.ProxyEndpoints[0].Port)
			};
			return new ConnectingToProxy(endpoints);
		}

		protected override State OnCancel()
		{
			return new ConnectingToProxy();
		}

		public override State OnException(Exception e)
		{
			return new ChangeIp(e.Message);
		}
	}



}
