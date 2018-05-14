using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Outracks;
using Outracks.Fusion;
using Outracks.IO;
using Outracks.Simulator;
using Outracks.Simulator.Bytecode;
using Outracks.Simulator.Protocol;
using Outracks.Simulator.Runtime;
using Uno.ProjectFormat;

namespace Fuse.Preview
{
	// These are temporary interfaces while refactoring
	public interface IProjectLike
	{
		IObservable<string> Name { get; }

		IObservable<IBinaryMessage> Mutations { get; }

		IObservable<IEnumerable<DocumentSource>> LatestSource { get; }

		IObservable<AbsoluteDirectoryPath> BuildOutputDirectory { get; }

		IObservable<AbsoluteDirectoryPath> RootDirectory { get; }

		IObservable<IImmutableSet<AbsoluteFilePath>> BundleFiles { get; }

		IObservable<IImmutableSet<AbsoluteFilePath>> FuseJsFiles { get; }

		IObservable<IImmutableSet<PackageReference>> PackageReferences { get; }

		IObservable<IImmutableSet<ProjectReference>> ProjectReferences { get; }
	}


	public class DocumentSource
	{
		public IDocumentLike LiveDocument { get; set; }
		public UxFileContents FileContents { get; set; }
	}

	public interface IDocumentLike
	{
		void UpdateSimulatorIds();
	}

	public class ProjectPreview : IDisposable
	{
		readonly IProjectLike _project;
		readonly IConnectableObservable<SimulatorHost> _simulatorHost;
		readonly BuildOutputDirGenerator _buildOutputDirGenerator;
		readonly AssetsWatcher _assetsWatcher;
		readonly Subject<object> _rebuild = new Subject<object>();
		readonly Subject<object> _refresh = new Subject<object>();
		readonly Subject<string> _logMessages = new Subject<string>();
		readonly ProxyServer _proxy;

		bool _isDisposed;
		readonly IDisposable _dispose;

		public IObservable<string> LogMessages
		{
			get
			{
				return _assetsWatcher.LogMessages
					.Merge(_logMessages.Select(s => s + '\n'));
			}
		}
	
		public ProjectPreview(
			IProjectLike project,
			IFileSystem shell,
			BuildOutputDirGenerator buildOutputDirGenerator, 
			ProxyServer proxy)
		{
			Name = project.Name;
			var simulatorHost = ProjectProcess.SpawnAsync().Replay(1);

			var simulatorMessages = simulatorHost.Switch(p => p.Messages.RefCount()).Publish().RefCount();

			var assemblyBuilt = simulatorMessages.TryParse(AssemblyBuilt.MessageType, AssemblyBuilt.ReadDataFrom);
			var bytecodeGenerated = simulatorMessages.TryParse(BytecodeGenerated.MessageType, BytecodeGenerated.ReadDataFrom);
			var bytecodeUpdated = simulatorMessages.TryParse(BytecodeUpdated.MessageType, BytecodeUpdated.ReadDataFrom);

			_project = project;
			_buildOutputDirGenerator = buildOutputDirGenerator;
			_proxy = proxy;
			_simulatorHost = simulatorHost;
			_assetsWatcher = new AssetsWatcher(shell, project.RootDirectory, Scheduler.Default);
			
			var bytecode = bytecodeGenerated.Select(msg => msg.Bytecode);

			_coalesceEntryCache = new CoalesceEntryCache();
			var assets = project.BundleFiles.CombineLatest(project.FuseJsFiles, (a, b) => a.Concat(b));
			var reifyMessages = ReifyProject(bytecode, bytecodeUpdated, _coalesceEntryCache, assets);

			var dependencyMessages = _assetsWatcher.UpdateChangedDependencies(bytecode.Select(bc => bc.Dependencies.ToImmutableHashSet()));
			var bundleFileMessages = _assetsWatcher.UpdateChangedBundleFiles(project.BundleFiles);
			var fuseJsFileMessages = _assetsWatcher.UpdateChangedFuseJsFiles(project.FuseJsFiles);

			_dispose = Observable.Merge(
					bundleFileMessages,
					dependencyMessages,
					fuseJsFileMessages,
					reifyMessages)
				.Subscribe(e => _coalesceEntryCache.Add(e));
			
			var incommingMessages = new Subject<IBinaryMessage>();

			var clientAdded = new Subject<string>();
			var clientRemoved = new Subject<string>();

			var socketServer = SocketServer.Start(
				port: 0,
				clientRun: (clientStream, endPoint) =>
				{
					bool isDisconnected = false;

					var writeMessages = _coalesceEntryCache
						.ReplayFrom(-1)
						.ObserveOn(TaskPoolScheduler.Default)
						.Subscribe(cacheEntry =>
						{
							if (isDisconnected)
								return;

							try
							{
								using (var memoryStream = new MemoryStream())
								{
									using (var memoryStreamWriter = new BinaryWriter(memoryStream))
									{
										// ReSharper disable once AccessToDisposedClosure
										cacheEntry.Entry.BlobData.Do(message => message.WriteTo(memoryStreamWriter));
										clientStream.Write(memoryStream.ToArray(), 0, (int)memoryStream.Length);
									}
								}
							}
							catch (Exception)
							{
								isDisconnected = true;
							}
						});
					
					var clientInfo = Optional.None<RegisterName>();

					try
					{
						using (var binaryStream = new BinaryReader(clientStream))
						{
							while (true)
							{
								var msg = BinaryMessage.ReadFrom(binaryStream);

								if (!clientInfo.HasValue)
								{
									clientInfo = BinaryMessage.TryParse(msg, RegisterName.MessageType, RegisterName.ReadDataFrom);
									
									if (clientInfo.HasValue)
										clientAdded.OnNext(clientInfo.Value.DeviceId);
								}

								incommingMessages.OnNext(msg);
							}
						}
					}
					finally
					{
						if (clientInfo.HasValue)
							clientRemoved.OnNext(clientInfo.Value.DeviceId);

						writeMessages.Dispose();
					}

				});

			Assembly = assemblyBuilt;
			Port = socketServer.LocalEndPoint.Port;
			Bytecode = bytecodeGenerated;
			PackageReferences = project.PackageReferences;
			ProjectReferences = project.ProjectReferences;

			ClientAdded = clientAdded;
			ClientRemoved = clientRemoved;

			Messages = Observable.Merge(
				incommingMessages,
				simulatorMessages);
			AccessCode = CodeGenerator.CreateRandomCode(5);

		}
	
		public IDisposable Start(
			IObservable<BuildProject> buildArgs,
			IScheduler scheduler = null)
		{
			scheduler = scheduler ?? Scheduler.Default;

			var refresh = _refresh.Merge(Messages.TryParse(ReifyRequired.MessageType, ReifyRequired.ReadDataFrom));

			var disposable = _simulatorHost.SubscribeUsing(host =>
			{
				var endedMessages = host.Messages.TryParse(Ended.MessageType, Ended.ReadDataFrom);

				return Disposable.Combine(
					Disposable.Create(() => _proxy.RemoveProject(Port)),
					
					_project.Mutations
						.Merge(_project.Mutations
							.OfType<ReifyRequired>().StartWith(new ReifyRequired())
							.WithLatestFromBuffered(_project.LatestSource, (signal, data) => data.ToArray())
							.Do(
								docs =>
								{
									foreach (var doc in docs)
									{
										doc.LiveDocument.UpdateSimulatorIds();
									}
								})
							.Throttle(TimeSpan.FromSeconds(1.0 / 20.0), scheduler)
							.ObserveOn(Application.MainThread)
							.Select(docs => (IBinaryMessage)new GenerateBytecode(Guid.NewGuid(), List.ToImmutableList(docs.Select(doc => doc.FileContents)))))
						.Merge(_rebuild.StartWith(new object())
							.CombineLatest(buildArgs, (_, t) => t)
							.WithLatestFromBuffered(_project.BuildOutputDirectory, (args, buildDir) =>
							{
								var output = _buildOutputDirGenerator.Acquire(buildDir / new DirectoryName("Local") / new DirectoryName("Designer"));
								args.OutputDir = output.NativePath;
								args.Id = Guid.NewGuid();

								return args;
							})
							.Do(args => _proxy.AddProject(
								Port,
								AccessCode.ToString(),
								args.ProjectPath, 
								args.Defines.ToArray())))
						.Merge(refresh
							.WithLatestFromBuffered(_project.LatestSource, (signal, data) => data.ToArray())
							.Select(docs => new GenerateBytecode(Guid.NewGuid(), List.ToImmutableList(docs.Select(doc => doc.FileContents))))
							.SkipUntil(endedMessages))
						.Subscribe(host.Send),

					endedMessages.Subscribe(ended =>
					{
						if (ended.Command.Type == BuildProject.MessageType)
						{
							_buildOutputDirGenerator.Release(ended.BuildDirectory);
							Refresh();
						}
					}));
			});

			_simulatorHost.Connect();

			return disposable;
		}

		public void Refresh()
		{
			_refresh.OnNext(new object());
		}

		public void Rebuild()
		{
			_rebuild.OnNext(new object());
		}

		/// <summary>
		/// Prevents the specified build output directory from being overwritten by a rebuild, since that will fail if the assembly is loaded by a viewport
		/// </summary>
		public IDisposable LockBuild(AbsoluteDirectoryPath outputDir)
		{
			return _buildOutputDirGenerator.Lock(outputDir);
		}

		IObservable<CoalesceEntry> ReifyProject(IObservable<ProjectBytecode> bytecode, IObservable<BytecodeUpdated> bytecodeUpdated, CoalesceEntryCache cache, IObservable<IEnumerable<AbsoluteFilePath>> assets)
		{
			int idx = 0;
			var bytecodeUpdates = bytecodeUpdated.Select(m => m.ToCoalesceEntry(BytecodeUpdated.MessageType + (++idx)))
				.Publish()
				.RefCount();

			var clearOldUpdates = bytecodeUpdates
				.Buffer(bytecode)
				.Select(
					oldUpdates =>
					{
						var cachedUpdates = new List<CoalesceEntry>();
						foreach (var oldUpdate in oldUpdates)
						{
							cachedUpdates.Add(new CoalesceEntry()
							{
								BlobData = Optional.None(),
								CoalesceKey = oldUpdate.CoalesceKey
							});
						}
						return cachedUpdates;
					})
					.SelectMany(t => t);

			var reify = bytecode.WithLatestFromBuffered(assets, (bc, ass) =>
			{
				var waitForDependencies = Task.WaitAll(new [] 
				{
					bc.Dependencies.Select(d => cache.HasEntry(d.ToString()))
						.ToObservableEnumerable()
						.FirstAsync()
						.ToTask(),
					ass.Select(d => cache.HasEntry(d.ToString()))
						.ToObservableEnumerable()
						.FirstAsync()
						.ToTask()
				}, TimeSpan.FromSeconds(60));

				if (waitForDependencies == false)
				{
					throw new TimeoutException("Failed to load all assets dependencies.");
				}

				try
				{
					return new BytecodeGenerated(
							new ProjectBytecode(
								reify: new Lambda(
									Signature.Action(Variable.This),
									Enumerable.Empty<BindVariable>(),
									new[]
									{
										ExpressionConverter.BytecodeFromSimpleLambda(() => ObjectTagRegistry.Clear()),

										new CallLambda(bc.Reify, new ReadVariable(Variable.This)),
									}),
								metadata: bc.Metadata,
								dependencies: bc.Dependencies))
						.ToCoalesceEntry(BytecodeGenerated.MessageType);
				}
				catch (Exception)
				{
					return new BytecodeUpdated(
							new Lambda(
								Signature.Action(),
								Enumerable.Empty<BindVariable>(),
								Enumerable.Empty<Statement>()))
						.ToCoalesceEntry("invalid-byte-code");
				}
			})
			.CatchAndRetry(TimeSpan.FromSeconds(15),
				e =>
				{
					_logMessages.OnNext("Failed to refresh because: " + e.Message);
				});

			return Observable.Merge(clearOldUpdates, reify, bytecodeUpdates);
		}

		public IObservable<CoalesceEntry> GetPreviewCache()
		{
			return _coalesceEntryCache.ReplayFrom(-1).Select(c => c.Entry);
		}

		public void Dispose()
		{
			if (_isDisposed)
				return;

			_dispose.Dispose();
			_assetsWatcher.Dispose();

			_isDisposed = true;
		}

		public IObservable<BytecodeGenerated> Bytecode { get; private set; }

		public IObservable<AssemblyBuilt> Assembly { get; private set; }
	
		public int Port { get; private set; }

		public IObservable<string> ClientRemoved { get; set; }
		public IObservable<string> ClientAdded { get; set; }

		public IObservable<IBinaryMessage> Messages { get; private set; }

		public readonly Code AccessCode;
		readonly CoalesceEntryCache _coalesceEntryCache;

		public IObservable<IImmutableSet<PackageReference>> PackageReferences { get; private set; }

		public IObservable<IImmutableSet<ProjectReference>> ProjectReferences { get; private set; }

		public readonly IObservable<string> Name;
	}
}