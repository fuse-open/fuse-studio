using Uno;
using Uno.Platform;
using Fuse.Controls;
using Fuse.Drawing;
using Uno.Net;
using Uno.Net.Sockets;
using Uno.Collections;
using Outracks.Simulator;
using Uno.Threading;
using Outracks;
using System.IO;
using Outracks.Simulator.Protocol;
using Outracks.Simulator.Runtime;
using Outracks.Simulator.Bytecode;
using Uno.UX;
using Uno.Compiler.ExportTargetInterop;
using Fuse.Scripting;

namespace Fuse.Simulator
{
	enum UserErrorType
	{
		Normal,
		Fatal
	}

	class UserError
	{
		public readonly UserErrorType ErrorType;
		public readonly string Message;

		public UserError(UserErrorType errorType, string message)
		{
			ErrorType = errorType;
			Message = message;
		}
	}

	class ErrorToJSCallback
	{
		readonly Context _ctx;
		readonly Function _callback;
		Uno.Threading.ConcurrentQueue<UserError> _errorMessages
			= new Uno.Threading.ConcurrentQueue<UserError>();

		public ErrorToJSCallback(Context ctx, Function callback)
		{
			_ctx = ctx;
			_callback = callback;
		}

		public void Error(UserError error)
		{
			_errorMessages.Enqueue(error);
			_ctx.Invoke(UpdateJS);
		}

		void UpdateJS(Context ctx)
		{
			UserError error;
			while(_errorMessages.TryDequeue(out error))
				_callback.Call(_ctx, error.ErrorType.ToString(), error.Message);
		}
	}

	public class SimulatorViewHost : Panel
	{
		static SimulatorViewHost()
		{
			ScriptClass.Register(typeof(SimulatorViewHost),
				new ScriptMethod<SimulatorViewHost>("onError", OnError),
				new ScriptMethod<SimulatorViewHost>("emulateBackbutton", EmulateBackbutton));
		}

		ErrorToJSCallback _errorToJSCallback;
		static object OnError(Context ctx, SimulatorViewHost owner, object []args)
		{
			owner._errorToJSCallback = new ErrorToJSCallback(ctx, (Function)args[0]);
			return null;
		}

		static void EmulateBackbutton(SimulatorViewHost owner)
		{
			Fuse.Input.Keyboard.EmulateBackButtonTap();
		}

		int _simulatorId;
		public int SimulatorId
		{
			get
			{
				return _simulatorId;
			}
			set
			{
				if(_simulatorId != value)
				{
					if(_simulatorId != 0)
					{
						Cleanup(_simulatorId);
					}

					_simulatorId = value;
					var simulatorView = new SimulatorView(_simulatorId);
					simulatorView.DiagnosticReported += OnDiagnosticsReported;
					simulatorView.FatalException += OnFatalException;
					Children.Add(simulatorView);
				}
			}
		}

		void OnDiagnosticsReported(Fuse.Diagnostic d)
		{
			if(d.Type == DiagnosticType.UserError || d.Type == DiagnosticType.InternalError)
			{
				if(_errorToJSCallback != null)
					_errorToJSCallback.Error(new UserError(UserErrorType.Normal, d.Message.ToString()));
			}
		}

		void OnFatalException(Exception exception)
		{
			if(_errorToJSCallback != null)
				_errorToJSCallback.Error(new UserError(UserErrorType.Fatal, exception.Message));
		}

		protected override void OnUnrooted()
		{
			Cleanup(SimulatorId);
		}

		void Cleanup(int simId)
		{
			foreach(var child in Children)
			{
				if(child is IDisposable)
					((IDisposable)child).Dispose();
				if(child is SimulatorView)
				{
					((SimulatorView)child).DiagnosticReported -= OnDiagnosticsReported;
					((SimulatorView)child).FatalException -= OnFatalException;
				}
			}

			Children.Clear();
			SimulatorManager.KillClient(simId);
		}
	}

	[Require("Entity", "Uno.UX.SimulatedProperties.Set(Uno.UX.PropertyObject,string,object,Uno.UX.IPropertyListener)")]
	[Require("Entity", "Uno.UX.SimulatedProperties.Get(Uno.UX.PropertyObject,string)")]
	public class SimulatorView : Panel, IDisposable, Fuse.IRootVisualProvider
	{
		readonly ISimulatorClient _client;
		readonly IReflection _reflection;
		readonly Project _project;
		readonly List<BytecodeUpdated> _bytecodeUpdates = new List<BytecodeUpdated>();
		readonly DiagnosticsManager _diagnosticsManager;
		ProjectBytecode _lastBytecode;
		object _previousState = null;

		public event Action<Fuse.Diagnostic> DiagnosticReported
		{
			add { _diagnosticsManager.DiagnosticReported += value; }
			remove { _diagnosticsManager.DiagnosticReported -= value; }
		}

		public event Action<Fuse.Diagnostic> DiagnosticDismissed
		{
			add { _diagnosticsManager.DiagnosticDismissed += value; }
			remove { _diagnosticsManager.DiagnosticDismissed -= value; }
		}

		public event Action<Exception> FatalException;

		public SimulatorView(int id)
		{
			var runtime = SimulatorManager.GetProjectRuntime(id);
			_client = runtime.Client;
			_reflection = SimulatorManager.Reflection;
			_project = SimulatorManager.RecentProjects.GetProject(runtime.ProjectId);
			_diagnosticsManager = new DiagnosticsManager(_client);
			SendClientInfo();
			UpdateManager.AddAction(Update);
			ScreenIdle.Disable();
			AppInstance.UnhandledException += OnUnhandledException;
		}

		float4 _clearColor;
		public float4 ClearColor
		{
			get { return _clearColor; }
			set
			{
				base.Background = new SolidColor(value);
				_clearColor = value;
			}
		}

		new public float4 Background
		{
			get { return ClearColor; }
			set { ClearColor = value; }
		}

		Visual Fuse.IRootVisualProvider.Root { get { return RootViewport; } }

		public Fuse.App AppInstance
		{
			get { return (Fuse.App)CoreApp.Current; }
		}

		public RootViewport RootViewport
		{
			get { return AppInstance.RootViewport; }
		}

		void Reset()
		{
			Children.Clear();
			ClearColor = float4(1,1,1,1);
			_previousState = RootViewport.SavePreviewState();
		}

		void SendClientInfo()
		{
			_client.Send(new RegisterName(DeviceInfo.GUID, DeviceInfo.Name));
		}

		void Update()
		{
			IBinaryMessage message;
			while (_client.IncommingMessages.TryDequeue(out message))
			{
				var bcg = message.TryParse(BytecodeGenerated.MessageType, (Func<System.IO.BinaryReader, BytecodeGenerated>)BytecodeGenerated.ReadDataFrom);
				if (bcg.HasValue)
				{
					ReifyBytecode(bcg.Value);
					continue;
				}

				var bu = message.TryParse(BytecodeUpdated.MessageType, (Func<System.IO.BinaryReader, BytecodeUpdated>)BytecodeUpdated.ReadDataFrom);
				if (bu.HasValue)
				{
					UpdateBytecode(bu.Value);
					continue;
				}

				var err = message.TryParse(Outracks.Simulator.Protocol.Error.MessageType, (Func<System.IO.BinaryReader, Outracks.Simulator.Protocol.Error>)Outracks.Simulator.Protocol.Error.ReadDataFrom);
				if (err.HasValue)
					Logger.Error("Error: " + err.Value.ToString());
			}
		}

		void ReifyBytecode(BytecodeGenerated bytecodeGenerated)
		{
			try
			{
				Logger.Info("Got reify!");
				Reset();
				var reify = bytecodeGenerated.Bytecode.Reify;
				new ScopeClosure(new Outracks.Simulator.Runtime.Environment(Optional.None<Outracks.Simulator.Runtime.Environment>()), _reflection).Execute(reify, this);
				_lastBytecode = bytecodeGenerated.Bytecode;
				UpdateCache();
				RootViewport.RestorePreviewState(_previousState);
			}
			catch(Exception e)
			{
				ReportBytecodeExceptions(e);
			}
		}

		void UpdateBytecode(BytecodeUpdated bytecodeUpdated)
		{
			try
			{
				Logger.Info("Got update!");
				var updateFunc = bytecodeUpdated.Function;
				new ScopeClosure(new Outracks.Simulator.Runtime.Environment(Optional.None<Outracks.Simulator.Runtime.Environment>()), _reflection).Execute(updateFunc, this);
				_bytecodeUpdates.Add(bytecodeUpdated);
				UpdateCache();
			}
			catch(Exception e)
			{
				ReportBytecodeExceptions(e);
			}
		}

		void ReportBytecodeExceptions(Exception e)
		{
			if(e is TypeNotFound)
			{
				e = new Exception("Type '" + ((TypeNotFound)e).Type + "' could not be found. The preview app doesn't support custom Uno code.");
			}

			if(FatalException != null)
				FatalException(e);
		}

		void UpdateCache()
		{
			// TODO: Enabling caching for recent project list
			//if(_lastBytecode == null) return;

			//SimulatorManager.RecentProjects.UpdateCache(_project, new BytecodeCache(_lastBytecode, _bytecodeUpdates.ToArray()));
		}

		void OnUnhandledException(object sender, UnhandledExceptionArgs args)
		{
			if(FatalException != null)
				FatalException(args.Exception);
		}

		public void Dispose()
		{
			AppInstance.UnhandledException -= OnUnhandledException;
			ScreenIdle.Enable();
			_diagnosticsManager.Dispose();
		}
	}
}