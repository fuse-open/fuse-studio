using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Outracks.Fuse.Protocol;

namespace Outracks.Fuse.Daemon
{
	public class DaemonRunner
	{
		readonly EnsureSingleUser _ensureSingleUser;

		bool _isRunning = true;

		readonly IFuseLauncher _fuse;
		readonly LocalSocketServer _localServer;
		readonly bool _isDebug;
		readonly Subject<IEventMessage<IEventData>> _broadcastEvents;
		readonly IConnectableObservable<IEventMessage<IEventData>> _replayBroadcastEvents;
		readonly PluginClients _pluginClients;
		readonly ServiceRunnerFactory _serviceRunnerFactory;
		Optional<ServiceRunner> _serviceRunner;
		readonly bool _runServices;
		readonly IReport _report;
		readonly DaemonKey _daemonKey;

		const string RunningAt = "127.0.0.1 port 12122";

		public DaemonRunner(
			EnsureSingleUser ensureSingleUser,
			LocalSocketServer localSocketServer,
			bool isDebug,
			bool startServices,
			IFuseLauncher fuse,
			ServiceRunnerFactory serviceRunnerFactory,
			IReport report)
		{
			_ensureSingleUser = ensureSingleUser;
			_runServices = startServices;
			_fuse = fuse;
			_localServer = localSocketServer;
			_isDebug = isDebug;
			_daemonKey = ensureSingleUser.GetDaemonKey();
			_broadcastEvents = new Subject<IEventMessage<IEventData>>();
			_replayBroadcastEvents = _broadcastEvents.Replay(TimeSpan.FromMinutes(5));
			_pluginClients = new PluginClients();
			_serviceRunnerFactory = serviceRunnerFactory;
			_report = report;
		}

		/// <exception cref="SocketException"></exception>
		public void Run()
		{
			using (var ensureSingleInstance = EnsureSingleInstanceFactory.Create("Fuse.Daemon", _report))
			{
				HandleMultipleUsers();

				if (ensureSingleInstance.IsAlreadyRunning())
				{
					var alreadyRunning = "Already running at " + RunningAt;
					_report.Info(alreadyRunning, ReportTo.LogAndUser);
					return;
				}

				try
				{
					var serializer = new Serializer();

					var clientConnected = _localServer.ClientConnected;
					var clientDisconnected = _localServer.ClientDisconnected;

					if (_isDebug)
					{
						clientConnected.Subscribe(
							c => _report.Info("A client connected at: " + DateTime.Now.ToShortTimeString(), ReportTo.LogAndUser));
						clientDisconnected.Subscribe(
							c => _report.Info("A client disconnected at: " + DateTime.Now.ToShortTimeString(), ReportTo.LogAndUser));
					}

					clientConnected.Subscribe(
						async c =>
						{
							var disposables = new ConcurrentStack<IDisposable>();
							c.Disconnected.Subscribe(t => Dispose(disposables)); // Dispose client stuff on OnCompleted

							var messagesOut = c.OutgoingMessages.Serialized(serializer);
							var messagesIn = c.IncomingMessages.TryDeserialize(serializer, _report);

							var requestReceiver = new RequestReceiver(
								_report,
								messagesIn.OfType<IRequestMessage<UnresolvedMessagePayload>>(),
								messagesOut);

							// Handle kill request
							disposables.Push(
								requestReceiver.SubscribeToRequest<KillRequest, KillResponse>(HandleKillRequest));
							try
							{
								await Task.Run(() => HelloStep(requestReceiver, c, disposables, messagesIn, messagesOut));
							}
							catch (Exception e)
							{
								_report.Error(e);
								c.Close();
							}
						});

					clientDisconnected.Subscribe(_pluginClients.Remove);

					_localServer.Host(12122);
					var runningAt = "Running at " + RunningAt;
					_report.Info(runningAt, ReportTo.LogAndUser);

					if (_runServices)
						Task.Run(() => _serviceRunnerFactory.Start())
							.ContinueWith(t => _serviceRunner = t.Result);

					//Task.Run(() => Proxy.Initialize());

					Console.CancelKeyPress += (sender, args) =>
					{
						DoPreTermination();
						ScheduleTerminate();
						args.Cancel = true;
					};

					using(Observable.Interval(TimeSpan.FromMinutes(10)).Subscribe(duration =>
						_report.Info("Daemon is still running")))
					using (_replayBroadcastEvents.Connect())
					using (_localServer)
					//using (simulatorProxyServer)
					{
						while (_isRunning)
						{
							Thread.Sleep(1000);
						}
					}
				}
				finally
				{
					DoPreTermination();
				}
			}
		}

		static void Dispose(ConcurrentStack<IDisposable> disposables)
		{
			IDisposable disposable;
			while (disposables.TryPop(out disposable))
				disposable.Dispose();
		}

		void HelloStep(
			RequestReceiver requestReceiver,
			IMessageConnection c,
			ConcurrentStack<IDisposable>
			disposables,
			IObservable<IMessage> messagesIn,
			IObserver<IMessage> messagesOut)
		{
			messagesOut.OnNext(
				Event.Create(new Welcome()
				{
					Message = "You have successfully connected to the Fuse Daemon. You can find API docs on our webpage."
				}));

			var taskResult = new TaskCompletionSource<Unit>();
			using(requestReceiver.SubscribeToRequest<HelloRequest, HelloResponse>(
				helloRequest =>
				{
					try
					{
						HandleHello(helloRequest, c, disposables, requestReceiver, messagesIn, messagesOut);
						taskResult.SetResult(Unit.Default);
						return new HelloResponse();
					}
					catch (Exception e)
					{
						taskResult.SetException(e);
						throw;
					}
				}))
			{
				c.StartRead();
				taskResult.Task.Wait();
			}
		}

		void HandleHello(
			HelloRequest helloReq,
			IMessageConnection c,
			ConcurrentStack<IDisposable> disposables,
			RequestReceiver requestReceiver,
			IObservable<IMessage> messagesIn,
			IObserver<IMessage> messagesOut)
		{
			if (helloReq == null)
			{
				throw new FuseRequestErrorException(ErrorCode.InvalidData, "Expected data to not be empty.");
			}

			// TODO: Enforce daemonkey to be present in the future
			if (!string.IsNullOrWhiteSpace(helloReq.DaemonKey)
				&& DaemonKey.Deserialize(helloReq.DaemonKey) != _daemonKey)
			{
				throw new FuseRequestErrorException(
						ErrorCode.WrongDaemonKey,
						"Daemon key was not right, maybe because daemon possesses wrong local user.");
			}

#pragma warning disable 0618
			var pluginClient = new PluginClient(
				messagesOut,
				helloReq.Identifier,
				helloReq.Implements,
				helloReq.EventFilter);
#pragma warning restore 0618

			_pluginClients.Add(c, pluginClient);

			_report.Info("Client connected: " + helloReq.Identifier, ReportTo.LogAndUser);
			c.Disconnected.Subscribe(d => _report.Info("Client disconnected: " + helloReq.Identifier, ReportTo.LogAndUser));

			// Broadcast events to clients that wants the events.
			disposables.Push(messagesIn.OfType<IEventMessage<IEventData>>().Subscribe(_broadcastEvents.OnNext));
			disposables.Push(_broadcastEvents
					.Subscribe(pluginClient.HandleEvent));

			// Handle Subscribe to event request.
			disposables.Push(
				messagesIn
				.OfType<IRequestMessage<UnresolvedMessagePayload>>()
				.Deserialize<SubscribeRequest>(_report)
				.Subscribe(
					r => disposables.Push(
						pluginClient.SubscribeToEvent(
							r,
							hotMessages: _broadcastEvents,
							replayMessages: _replayBroadcastEvents))
				));

			disposables.Push(
				requestReceiver.SubscribeToRequest<PublishServiceRequest, PublishServiceResponse>(pluginClient.AppendSupportedRequests));

			// Send requests to clients that implements the request.
			disposables.Push(
				messagesIn.OfType<IRequestMessage<IRequestData>>()
					.Where(r => r.Name != "Subscribe" && r.Name != "Fuse.KillDaemon" && r.Name != "PublishService")
					.Subscribe(r => Task.Run(() => _pluginClients.PassRequestToAnImplementor(pluginClient, r))));

			// Handle the response from the one that implemented the request
			disposables.Push(
				messagesIn.OfType<IResponseMessage<IResponseData>>().Subscribe(pluginClient.HandleResponse));
		}

		Optional<KillResponse> HandleKillRequest(KillRequest k)
		{
			try
			{
				_report.Info("Shutting down because " + k.Reason, ReportTo.LogAndUser);

				DoPreTermination();

				return new KillResponse();
			}
			finally
			{
				ScheduleTerminate();
			}
		}

		void ScheduleTerminate()
		{
			_isRunning = false;
		}

		void DoPreTermination()
		{
			_serviceRunner.Do(r => r.Dispose());
		}

		void HandleMultipleUsers()
		{
			var currentUser = _ensureSingleUser.GetUserName();
			var lastUserWhoRanDaemon = _ensureSingleUser.GetUserWhoRanDaemonLastTime();
			if (lastUserWhoRanDaemon != currentUser)
			{
				try
				{
					var client = _fuse.Connect();

					if (!client.Request(new KillRequest() { Reason = "you are possessing wrong user" })
						.Wait(10000))
					{
						_report.Error(
							"Failed to kill daemon which possesses another user.\n" +
								"Try to sign out from all users on your computer.", ReportTo.LogAndUser);
					}

					// We must wait a bit since it takes time to kill a daemon.
					Thread.Sleep(TimeSpan.FromSeconds(3));
				}
				catch (AggregateException e)
				{
					if (e.InnerException is SocketException)
					{
						_report.Info("SocketException in HandleMultipleUsers '" + e.Message + "'");
					}
					else
					{
						_report.Info(e.InnerException.Message);
					}
				}
				catch (Exception e)
				{
					_report.Info(e.Message);
				}
			}

			_ensureSingleUser.SetUserThatRunsDaemon(currentUser);
		}
	}
}
