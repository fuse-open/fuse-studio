using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Fuse.Preview;
using Outracks.Diagnostics;
using Outracks.Fuse.Hierarchy;
using Outracks.Fuse.Live;
using Outracks.Fuse.Protocol;
using Outracks.Fuse.Protocol.Messages;
using Outracks.Fuse.Protocol.Preview;
using Outracks.Fuse.Refactoring;
using Outracks.Fuse.Setup;
using Outracks.Fuse.Stage;
using Outracks.Fuse.Theming.Themes;
using Outracks.Fusion;
using Outracks.Fusion.AutoReload;
using Outracks.IO;
using Outracks.IPC;
using Outracks.Simulator.Protocol;
using Uno.Diagnostics;

namespace Outracks.Fuse.Studio
{
	class OpenProject
	{
		readonly IFuse _fuse;
		readonly IMessagingService _daemon;
		readonly IObservable<Exception> _errors;
		readonly Subject<string> _hostRequestMessages = new Subject<string>();
		readonly PreviewService _previewService;
		readonly IShell _shell;
		readonly SetupGuide _setupGuide;
		readonly CheckForUpdates _checkForUpdates;
		readonly Help _help;
		readonly Subject<Unit> _mainWindowFocused = new Subject<Unit>();

		public OpenProject(
			PreviewService previewService,
			IShell shell,
			IFuse fuse,
			IMessagingService daemon,
			IObservable<Exception> errors,
			Activate activate)
		{
			_previewService = previewService;
			_shell = shell;
			_fuse = fuse;
			_daemon = daemon;
			_errors = errors;

			var doneLoading = _mainWindowFocused.FirstAsync();
			_setupGuide = new SetupGuide(fuse, doneLoading);
			_checkForUpdates = new CheckForUpdates(fuse.Version);
			_help = new Help();

			// Global default menu for macOS.
			Application.DefaultMenu = MainMenu.Create(
				_fuse,
				_daemon,
				_shell,
				null,
				_help,
				new Menu(),
				new Menu(),
				null,
				null,
				_setupGuide,
				_checkForUpdates,
				new Menu(),
				new Debug(null),
				activate);

			RespondToFocusRequests.Start(_daemon, _projectsOpen);
		}

		readonly BehaviorSubject<ImmutableList<ProjectHost>> _projectsOpen =
			new BehaviorSubject<ImmutableList<ProjectHost>>(ImmutableList.Create<ProjectHost>());


		public Window Execute(Fusion.IDocument document, string[] args)
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
			var export = new Export(project, _fuse, buildArgs);

			var usbMode = new USBMode(new AndroidPortReverser(), Observable.Return(preview.Port), _previewService);
			var previewOnDevice = new PreviewOnDevice(_fuse, project, buildArgs);
			var build = new Build(project, preview, previewOnDevice, usbMode.EnableUsbModeCommand, buildArgs, export);

			var projHost = new ProjectHost(build.BuildArguments, preview, project, self =>
				_projectsOpen.OnNext(_projectsOpen.Value.Remove(self)));
			_projectsOpen.OnNext(_projectsOpen.Value.Add(projHost));

			var codeView = new CodeView(preview.AccessCode, NetworkHelper
				.GetInterNetworkIps()
				.ToArray());

			// Viewport stuff

			var selectionEnabled = Property.Create(false);
			var activate = new Activate(_fuse.License, _daemon);

			var glVersion = new Subject<OpenGlVersion>();

			var viewport = new ViewportFactory(
				preview,
				selectionEnabled,
				preview.Port,
				project,
				_fuse,
				glVersion,
				activate.Show);

			var mode = UserSettings.Enum<Mode>("WindowMode").Or(Mode.Normal);
			var previewDevices = new PreviewDevices(project, _shell);
			var workspace = new StageController(
				viewport,
				previewDevices,
				selectionEnabled,
				_fuse.License);

			var topMost = UserSettings.Bool("TopMost").Or(false);
			var windowMenu =
				  Menu.Toggle(
					name: Texts.SubMenu_Window_CompactMode,
					toggle: mode.Convert(
						convert: m => m == Mode.Compact,
						convertBack: b => b ? Mode.Compact : Mode.Normal),
					hotkey: HotKey.Create(ModifierKeys.Meta, Key.M))
				+ Menu.Separator
				+ Menu.Toggle(
					name: Texts.SubMenu_Window_KeepWindowOnTop,
					toggle: topMost,
					hotkey: HotKey.Create((Platform.IsWindows ? ModifierKeys.Shift : ModifierKeys.Alt) | ModifierKeys.Meta, Key.T))
				+ Menu.Separator
				+ Menu.Submenu(Texts.SubMenu_Window_Language, LanguageMenu.Menu)
				+ Menu.Separator
				+ Menu.Option(
					value: Themes.OriginalLight,
					name: Texts.SubMenu_Window_LightTheme,
					property: Theme.CurrentTheme)
				+ Menu.Option(
					value: Themes.OriginalDark,
					name: Texts.SubMenu_Window_DarkTheme,
					property: Theme.CurrentTheme);

			var messages = preview.Messages.Replay(TimeSpan.FromSeconds(2)).RefCount();

			project.FilePath.SubscribeUsing(projPath =>
				PushEventsToDaemon.Start(
					messages: messages,
					daemon: _daemon,
					projectPath: projPath,
					projectId: ProjectIdComputer.IdFor(projPath),
					target: BuildTarget.DotNet));

			var sketchConverter = new SketchWatcher(_fuse.Report, _shell);

			var classExtractor = new ClassExtractor(project);

			var buildStartedOrLicenseChanged = project.FilePath
				.CombineLatest(build.Rebuilt.StartWith(Unit.Default));

			var logMessages = new ReplayQueueSubject<string>();

			var allLogMessages = messages.ToLogMessages()
				.Merge(logMessages)
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

			logMessages.OnNext(UnoVersion.LongHeader + "\n" +
							   UnoVersion.Copyright.Replace("(C)", "©") + "\n\n");

			var stopPreview = preview.Start(build.BuildArguments);

			// start with a viewport
			workspace.NewViewport.ExecuteOnce();

			var projectMenu =
				  ProjectMenu.CommandItems(project.FilePath.Select(Optional.Some), _shell)
				+ Menu.Separator
				+ ProjectMenu.FileItems(project, _shell)
				+ Menu.Separator
				+ Menu.Item(Texts.SubMenu_Project_SketchImport, sketchConverter.ShowDialog());

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
					_daemon,
					_shell,
					workspace,
					_help,
					elementContext.CreateMenu(project.Context.CurrentSelection),
					projectMenu,
					build,
					export,
					_setupGuide,
					_checkForUpdates,
					windowMenu,
					debug,
					activate),
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
				codeView: codeView,
				license: _fuse.License);

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
