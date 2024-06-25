using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Threading;
using Outracks.IO;

namespace Outracks.Fuse.Daemon
{
	class ServiceProcess
	{
		public readonly Service Service;
		public readonly Process Process;

		public ServiceProcess(Service service, Process process)
		{
			Service = service;
			Process = process;
		}

		public bool IsRunning()
		{
			return !Process.HasExited;
		}
	}

	public class Service
	{
		public readonly string Name;
		public readonly IExternalApplication Application;

		public Service(string name, IExternalApplication application)
		{
			Name = name;
			Application = application;
		}
	}

	public class ServiceRunnerFactory
	{
		readonly Service[] _services;

		public ServiceRunnerFactory(params Service[] services)
		{
			_services = services;
		}

		public ServiceRunner Start()
		{
			return new ServiceRunner(_services);
		}
	}

	public class ServiceRunner : IDisposable
	{
		ImmutableList<ServiceProcess> _serviceProcesses;
		readonly CancellationTokenSource _ctSource = new CancellationTokenSource();
		readonly Thread _thread;
		bool _isDisposed = false;

		public ServiceRunner(params Service[] services)
		{
			_serviceProcesses = StartAllServices(services);

			_thread = new Thread(Run) { IsBackground = true };
			_thread.Start(_ctSource.Token);
		}

		ImmutableList<ServiceProcess> StartAllServices(IEnumerable<Service> services)
		{
			return services.Select(StartService).ToImmutableList();
		}

		void Run(object data)
		{
			var ct = (CancellationToken) data;

			while (!ct.IsCancellationRequested)
			{
				var cloneOfServices = _serviceProcesses.ToArray();
				foreach(var runningService in cloneOfServices)
				{
					if (!runningService.IsRunning())
					{
						runningService.Process.Dispose();
						Console.Error.WriteLine("Service " + runningService.Service.Name + " died, attempting to restart...");
						_serviceProcesses = _serviceProcesses.Replace(runningService, StartService(runningService.Service));
					}
				}

				Thread.Sleep(100);
			}
		}

		ServiceProcess StartService(Service service)
		{
			var serviceProcess = service.Application.Start(
							new ProcessStartInfo()
							{
								UseShellExecute = false,
								WindowStyle = ProcessWindowStyle.Hidden,
								CreateNoWindow = true,
								RedirectStandardError = true,
								RedirectStandardInput = true,
								RedirectStandardOutput = true,
							});

			serviceProcess.ConsumeOutput(Observer.Create((string line) => {}));
			serviceProcess.ConsumeError(Observer.Create((string line) => {}));

			return new ServiceProcess(service, serviceProcess);
		}

		public void Dispose()
		{
			if (_isDisposed)
				return;
			_isDisposed = true;
			_ctSource.Cancel();
			_thread.Join();
		}
	}
}