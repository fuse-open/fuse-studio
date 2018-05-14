using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Fuse.Preview;
using Outracks.Fuse.Analytics;
using Outracks.IPC;
using Uno.ProjectFormat;

namespace Outracks.Fuse.Designer
{
	using IO;
	using Fusion;
	using Protocol;
	using Protocol.Preview;
	using Simulator.Protocol;
	using Live;
	using Protocol.Messages;
	using Fusion.AutoReload;
	using Diagnostics;
	using Hierarchy;
	using Refactoring;
	using Setup;
	using Stage;
	using Theming.Themes;

	class OpenProject
	{
		readonly IFuse _fuse;
		readonly IMessagingService _daemon;
		readonly IObservable<Exception> _errors;
		readonly Subject<string> _hostRequestMessages = new Subject<string>();
		readonly PreviewService _previewService;
		readonly IShell _shell;
		readonly SetupGuide _setupGuide;
		readonly Subject<Unit> _mainWindowFocused = new Subject<Unit>();

		public OpenProject(
			PreviewService previewService,
			IShell shell,
			IFuse fuse,
			IMessagingService daemon,
			IObservable<Exception> errors)
		{
			_previewService = previewService;
			_shell = shell;
			_fuse = fuse;
			_daemon = daemon;
			_errors = errors;

			var doneLoading = _mainWindowFocused.FirstAsync();
			_setupGuide = new SetupGuide(fuse, doneLoading);
			
			RespondToFocusRequests.Start(_daemon, _projectsOpen);
		}

		readonly BehaviorSubject<ImmutableList<ProjectHost>> _projectsOpen =
			new BehaviorSubject<ImmutableList<ProjectHost>>(ImmutableList.Create<ProjectHost>());

		
		public Window Execute(IDocument document, string[] args)
		{
			var hack = new ReplaySubject<IObservable<BytecodeGenerated>>(1); 
			var project = new LiveProject(document, _shell, hack.Switch().ObserveOn(Application.MainThread));
			var ignore = RecentProjects.Bump(document, project.Name.ObserveOn(Application.MainThread));

			var preview = _previewService.StartPreview(project);
			hack.OnNext(preview.Bytecode);

			#pragma warning disable 0219
			var garbage = ExternalSelection.UpdateProjectContext(_daemon, project);
			#pragma warning restore 0219

			var inspector = Fuse.Inspector.Inspector.Create(project);

			var buildArgs = new BuildArgs(args);

			var usbMode = new USBMode(new AndroidPortReverser(), Observable.Return(preview.Port), _previewService);
			var previewOnDevice = new PreviewOnDevice(_fuse, project, buildArgs);

			var build = new Build(project, preview, previewOnDevice, usbMode.EnableUsbModeCommand, buildArgs);
			var export = new Export(project, _fuse, buildArgs);

			var projHost = new ProjectHost(build.BuildArguments, preview, project, self => 
				_projectsOpen.OnNext(_projectsOpen.Value.Remove(self)));
			_projectsOpen.OnNext(_projectsOpen.Value.Add(projHost));

			var codeView = new CodeView(preview.AccessCode, NetworkHelper
				.GetInterNetworkIps()
				.ToArray());

			// Viewport stuff

			var selectionEnabled = Property.Create(false);


			var glVersion = new Subject<OpenGlVersion>();

			var viewport = new ViewportFactory(
				preview,
				selectionEnabled,
				preview.Port,
				project,
				_fuse,
				glVersion);

			var mode = Property.Create(Mode.Normal);
			var previewDevices = new PreviewDevices(project, _shell);
			var workspace = new StageController(
				viewport,
				previewDevices,
				selectionEnabled);

			var topMost = Property.Create(false);
			var windowMenu =
				  Menu.Toggle(
					name: "Compact mode",
					toggle: mode.Convert(
						convert: m => m == Mode.Compact,
						convertBack: b => b ? Mode.Compact : Mode.Normal),
					hotkey: HotKey.Create(ModifierKeys.Meta, Key.M))
				+ Menu.Separator
				+ Menu.Toggle(
					name: "Keep window on top",
					toggle: topMost,
					hotkey: HotKey.Create((_fuse.Platform == OS.Windows ? ModifierKeys.Shift : ModifierKeys.Alt) | ModifierKeys.Meta, Key.T))
				+ Menu.Separator
				+ Menu.Option(
					value: Themes.OriginalLight,
					name: "Light theme",
					property: Theme.CurrentTheme)
				+ Menu.Option(
					value: Themes.OriginalDark,
					name: "Dark theme",
					property: Theme.CurrentTheme);

			var messages = preview.Messages.Replay(TimeSpan.FromSeconds(2)).RefCount();

			project.FilePath.SubscribeUsing(projPath => 
				PushEventsToDaemon.Start(
					messages: messages,
					daemon: _daemon,
					projectPath: projPath,
					projectId: ProjectIdComputer.IdFor(projPath),
					target: BuildTarget.DotNetDll));

			var sketchConverter = new SketchWatcher(_fuse.Report, _shell);

			var classExtractor = new ClassExtractor(project);

			var buildStartedOrLicenseChanged = project.FilePath
				.CombineLatest(build.Rebuilt.StartWith(Unit.Default));

			var allLogMessages = messages.ToLogMessages()
				.Merge(_setupGuide.LogMessages)
				.Merge(export.LogMessages)
				.Merge(project.LogMessages)
				.Merge(previewOnDevice.LogMessages)
				.Merge(classExtractor.LogMessages)
				.Merge(_hostRequestMessages)
				.Merge(_errors.Select(e => e.Message))
				.Merge(AutoReload.Log.Select(msg => "[Fusion AutoReload]: " + msg)
				.Merge(glVersion.Take(1).ToLogMessages(_fuse.Report)))
				.Merge(preview.LogMessages)
				.Merge(_previewService.LogMessages)
				.Merge(sketchConverter.LogMessages)
				.Merge(previewDevices.LogMessages);

			var stopPreview = preview.Start(build.BuildArguments);

			// start with a viewport
			workspace.NewViewport.ExecuteOnce();

			var projectMenu = 
				  ProjectMenu.CommandItems(project.FilePath.Select(Optional.Some), _shell) 
				+ Menu.Separator
				+ ProjectMenu.FileItems(project, _shell)
				+ Menu.Separator
				+ Menu.Item("Sketch import", sketchConverter.ShowDialog());

			var help = new Help();

			var debug = new Debug(project);


			var elementContext = new ElementContext(project.Context, project, _daemon);

			var errorView = new ErrorView(messages, project.FilePath, _daemon, preview.ClientRemoved);

			var logView = new LogView(allLogMessages, messages, errorView);
			var sketchWatch = sketchConverter.Watch(project);
			var window = MainWindow.Create(
				projectName: project.Name,
				search: Control.Empty,//search.Control,
				outline: CreateLeftPane(project, elementContext, project.Context, _shell, classExtractor),
				bookmarks: Control.Empty,
				stage: workspace,
				notifications: Layout.StackFromTop(
					SimulatorNotifications.CreateBusyIndicator(messages),
					SimulatorNotifications.Create(messages, build.Rebuild, logView.IsExpanded)),
				inspector: inspector,
				attributes: Control.Empty,//DataContext.Create(workspace.Viewport, selection.Element),
				logview: logView,
				menu: MainMenu.Create(
					_fuse, 
					_shell, 
					workspace, 
					help, 
					elementContext.CreateMenu(project.Context.CurrentSelection),
					projectMenu, 
					build, 
					export,
					_setupGuide,  
					windowMenu,
					debug),
				closed: Command.Enabled(() =>
				{
					stopPreview.Dispose();
					preview.Dispose();
					projHost.Dispose();
					project.FilePath.Take(1).Subscribe(path => 
						_daemon.Broadcast(new ProjectClosed { ProjectId = ProjectIdComputer.IdFor(path) }));
					project.Dispose();
					workspace.Dispose();
					sketchWatch.Dispose();
				}),
				selectionEnabled: selectionEnabled,
				topMost: topMost,
				mode: mode,
				mainWindowFocused: _mainWindowFocused.Update(Unit.Default),
				context: project.Context,
				codeView: codeView);

			return window;
		}

		IControl CreateLeftPane(
			LiveProject project, 
			ElementContext elementContext, 
			IContext context,
			IShell fileSystem,
			IClassExtractor classExtractor)
		{
			return Modal.Host(modalHost =>
			{
				var makeClassViewModel = new ExtractClassButtonViewModel(
					project,
					context,
					dialogModel => ExtractClassView.OpenDialog(modalHost, dialogModel),
					fileSystem,
					classExtractor);

				var hierarchy = TreeView.Create(
					new TreeViewModel(
						context,
						makeClassViewModel.HighlightSelectedElement,
						elementContext.CreateMenu));

				return Layout.Dock()
					.Bottom(Toolbox.Toolbox.Create(project, elementContext))
					.Top(ExtractClassView.CreateButton(makeClassViewModel))
					.Fill(hierarchy);
			});
		}
	}

	enum Mode
	{
		Normal,
		Compact
	}
}
