using Uno;
using Uno.Collections;
using Outracks.Simulator.Bytecode;
using Outracks.Simulator.Protocol;
using Uno.Diagnostics;
using Outracks.Simulator;

namespace Fuse.Simulator
{
	class DiagnosticsManager : IDisposable
	{
		readonly ISimulatorClient _client;
		readonly object _mutex = new object();
		Dictionary<Fuse.Diagnostic, Outracks.Simulator.Protocol.Diagnostic> _activeDiagnostics = new Dictionary<Fuse.Diagnostic, Outracks.Simulator.Protocol.Diagnostic>();

		public event Action<Fuse.Diagnostic> DiagnosticReported;
		public event Action<Fuse.Diagnostic> DiagnosticDismissed;

		public DiagnosticsManager(ISimulatorClient client)
		{
			_client = client;
			Fuse.Diagnostics.DiagnosticReported += OnDiagnosticReported;
			Fuse.Diagnostics.DiagnosticDismissed += OnDiagnosticDismissed;
			Log.SetHandler(SendDebugLog);
		}

		void OnDiagnosticReported(Fuse.Diagnostic d)
		{
			lock(_mutex)
			{
				UpdateManager.PostAction(Closure.Apply<Fuse.Diagnostic>(FireDiagnosticReported, d));
				var diag = new Outracks.Simulator.Protocol.Diagnostic(DeviceInfo.GUID, DeviceInfo.Name, d.Message, d.ToString(), d.FilePath, d.LineNumber, 0);
				_client.Send(diag);
				_activeDiagnostics.Add(d, diag);
			}
		}

		void FireDiagnosticReported(Fuse.Diagnostic d)
		{
			if(DiagnosticReported != null)
				DiagnosticReported(d);
		}

		void OnDiagnosticDismissed(Fuse.Diagnostic d)
		{
			lock(_mutex)
			{
				UpdateManager.PostAction(Closure.Apply<Fuse.Diagnostic>(FireDiagnosticDismissed, d));
				Outracks.Simulator.Protocol.Diagnostic diag;
				if (_activeDiagnostics.TryGetValue(d, out diag))
				{
					_client.Send(new DismissDiagnostic(DeviceInfo.GUID, DeviceInfo.Name, diag.DiagnosticId));
					_activeDiagnostics.Remove(d);
				}
			}
		}

		void FireDiagnosticDismissed(Fuse.Diagnostic d)
		{
			if(DiagnosticDismissed != null)
				DiagnosticDismissed(d);
		}

		void DismissAllActiveDiagnostics()
		{
			lock (_mutex)
			{
				foreach (var k in _activeDiagnostics)
				{
					_client.Send(new DismissDiagnostic(DeviceInfo.GUID, DeviceInfo.Name, k.Value.DiagnosticId));
				}
				_activeDiagnostics.Clear();
			}
		}

		void SendDebugLog(LogLevel level, string message)
		{
			_client.Send(new DebugLog(DeviceInfo.GUID, DeviceInfo.Name, message));
		}

		public void Dispose()
		{
			Fuse.Diagnostics.DiagnosticReported -= OnDiagnosticReported;
			Fuse.Diagnostics.DiagnosticDismissed -= OnDiagnosticDismissed;
			Log.SetHandler(null);
			DismissAllActiveDiagnostics();
		}
	}
}