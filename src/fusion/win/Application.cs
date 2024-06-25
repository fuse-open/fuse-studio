using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Outracks.Fuse;
using Outracks.Fusion.Windows.Controls;
using Outracks.IO;
using Outracks.UnoHost.Windows;

namespace Outracks.Fusion.Windows
{
	public class Application : IApplication
	{
		public static bool MonitorAware = false;

		static Application()
		{
			if (MonitorAware)
				DpiAwareness.SetDpiAware(DpiAwareness.ProcessDpiAwareness.MonitorAware);
			else
				DpiAwareness.SetDpiAware(DpiAwareness.ProcessDpiAwareness.SystemAware);
		}

		System.Windows.Application _app;
		Optional<DocumentAppHandler> _documentApp;
		Dispatcher _dispatcher;

		public bool InitializeDocumentApp(IList<string> args, string applicationName)
		{
			_dispatcher = _dispatcher ?? new Dispatcher(Thread.CurrentThread);

			var documentApp = new DocumentAppHandler(ReportFactory.FallbackReport, applicationName, args.ToArray());
			_documentApp = documentApp;
			if (documentApp.IsOnlyInstance())
			{
				Initialize(args);
				return true;
			}

			documentApp.RunClient();
			return false;
		}

		public void Initialize(IList<string> args)
		{
			_dispatcher = _dispatcher ?? new Dispatcher(Thread.CurrentThread);
			Fusion.Application.PerFrame = _dispatcher.PerFrame;
			Fusion.Application.MainThread = _dispatcher;

			Desktop = new WindowsDialog<object>();

			DraggingImplementation.Initialize(_dispatcher);
			LabelImplementation.Initialize(_dispatcher, Observable.Return(new Ratio<Pixels, Points>(1.0)));
			//ListBoxImplementation.Initialize(dispatcher);
			TextBoxImplementation.Initialize(_dispatcher);
			ClippingImplementation.Initialize(_dispatcher);
			ScrollingImplementation.Initialize(_dispatcher);
			EffectsImplementation.Initialize(_dispatcher);
			ContextMenuImplementation.Initialize(_dispatcher);
			CursorsImplementation.Initialize(_dispatcher);
			ToolTipImplementation.Initialize(_dispatcher);
			ButtonImplementation.Initialize(_dispatcher);
			RectangleImplementation.Initialize(_dispatcher);
			CircleImplementation.Initialize(_dispatcher);
			ImageImplementation.Initialize(_dispatcher);
			OverlayImplementation.Initialize(_dispatcher);
			LineImplementation.Initialize(_dispatcher);
			DropDownImplementation.Initialize(_dispatcher);
			SliderImplementation.Initialize(_dispatcher);
			ColorPickerImplementation.Initialize(_dispatcher);
			WebViewImplementation.Initialize(_dispatcher);
			KeyboardImplementation.Initialize(_dispatcher);
			LogViewImplementation.Initialize(_dispatcher);
			PointerImplementation.Initialize(_dispatcher);
			Transformation.Initialize(_dispatcher);
			LayoutTrackerImplementation.Initialize();
			FileDialogs.Initialize();
			WinFormsMessageBox.Initialize();

			_app = new System.Windows.Application();
			_app.ShutdownMode = ShutdownMode.OnLastWindowClose;
			_app.Exit += (s, a) =>
			{
				if (Terminating != null) Terminating();
			};

			EditMenu =
				  /*FromWpfCommand(ApplicationCommands.Undo)
				+ FromWpfCommand(ApplicationCommands.Redo)
				+ MenuItem.CreateSeparator()
				+ */FromWpfCommand(Strings.SubMenu_Edit_Cut, ApplicationCommands.Cut)
				+ FromWpfCommand(Strings.SubMenu_Edit_Copy, ApplicationCommands.Copy)
				+ FromWpfCommand(Strings.SubMenu_Edit_Paste, ApplicationCommands.Paste)
				+ FromWpfCommand(Strings.SubMenu_Edit_Delete, ApplicationCommands.Delete)
				+ FromWpfCommand(Strings.SubMenu_Edit_SelectAll, ApplicationCommands.SelectAll);
		}

		static Menu FromWpfCommand(string commandText, RoutedUICommand command)
		{
			return Menu.Item(commandText, Observable
					.FromEventPattern(command,"CanExecuteChanged")
					.Select(_ => Unit.Default)
					.StartWith(Unit.Default)
					.Select(_ => command.CanExecute(null, null))
					.DistinctUntilChanged()
					.Switch(canExecute => Command.Create(
						isEnabled: canExecute,
						action: () => command.Execute(null, null))),
				hotkey: command.InputGestures.ToHotKey());
		}

		public void Run()
		{
			_documentApp.Do(d => d.RunHost());
			_app.Run();
		}

		public void Run(Window window)
		{
			_documentApp.Do(d => d.RunHost());
			_app.Run();
		}

		public void Exit(byte exitCode)
		{
			if (System.Windows.Forms.Application.MessageLoop)
			{
				Environment.ExitCode = exitCode;
				System.Windows.Forms.Application.Exit();
			}
			else
			{
				_app.Dispatcher.Invoke(() => _app.Shutdown(exitCode));
			}
		}

		public void ShowOpenDialog(FileFilter[] fileFilters)
		{
			Task.Run(
				async () =>
				{
					try
					{
						var fileDialogs = new FileDialogs(FocusedWindow);

						var result = await fileDialogs.OpenFile(new FileDialogOptions("Open", fileFilters));
						if (!result.HasValue)
						{
							return;
						}

						await OpenDocument(result.Value.Path, true);
					}
					catch (Exception e)
					{
						ReportFactory.FallbackReport.Exception("Failed to open file with file dialog.", e);
					}
				});
		}

		System.Windows.Window FocusedWindow
		{
			get
			{
				return _app.Dispatcher.Invoke(() =>
				{
					foreach (var windowObject in _app.Windows)
					{
						var window = windowObject as System.Windows.Window;
						if (window == null) continue;

						if (window.IsActive)
							return window;
					}
					return null;
				});
			}
		}

		public IObservable<Window> DocumentOpened
		{
			get { return _documentApp.Value.DocumentOpened; }
		}

		// Not used on Windows.
		public Optional<Menu> DefaultMenu { get; set; }

		public IDialog<object> Desktop { get; private set; }
		public Menu EditMenu { get; private set; }

		public event Action Terminating;

		public async Task<IDocument> OpenDocument(AbsoluteFilePath path, bool showWindow)
		{
			if(!_documentApp.HasValue)
				throw new NotImplementedException("Only document apps supports Open Document");

			var documentApp = _documentApp.Value;
			return await documentApp.OpenDocumentWindow(path);
		}

		// Legacy
		public ITrayApplication CreateTrayApplication(IReport errorHandler, IObservable<string> title, Menu menu, IObservable<Icon> icon)
		{
			return new WindowsTrayApplication(title, menu, icon);
		}
	}

	class WindowsDialog<T> : IDialog<T>
	{
		readonly ISubject<T> _result = new ReplaySubject<T>(1);
		Action _onClose;
		Optional<System.Windows.Window> _parent;
		BehaviorSubject<IImmutableList<IControl>> _modalDialogs;

		public void Focus()
		{
			_parent.Do(win =>
				win.Dispatcher.InvokeAsync(() =>
				{
					if (win.WindowState == System.Windows.WindowState.Minimized)
					{
						win.WindowState = System.Windows.WindowState.Normal;
					}
					win.Activate();
				}));
		}

		public void Close(T result = default(T))
		{
			_onClose();
			_result.OnNext(result);
		}

		public void CloseWithError(Exception e)
		{
			_onClose();
			_result.OnError(e);
		}

		public void CreateSingletonWindow(IObservable<bool> isVisible, Func<IDialog<object>, Window> window)
		{
			Fusion.Application.MainThread.Schedule(() =>
			{
				var modalDialogs = new BehaviorSubject<IImmutableList<IControl>>(ImmutableList<IControl>.Empty);

				var childWindow = new WindowsDialog<object>();

				var model = OverlayModalDialogs(window(childWindow), modalDialogs);

				var wnd = WindowImplementation.Initialize(model);

				childWindow._modalDialogs = modalDialogs;
				childWindow._parent = wnd;
				childWindow._onClose = () =>
					Fusion.Application.MainThread.Schedule(wnd.Hide);

				wnd.Closing += (s, a) =>
				{
					a.Cancel = true;
					wnd.Hide();
				};

				isVisible.Subscribe(vis =>
					Fusion.Application.MainThread.Schedule(() =>
					{
						if (vis)
							wnd.Show();
						if (!vis)
							wnd.Hide();
					}));
			});
		}


		public async Task<TU> ShowDialog<TU>(Func<IDialog<TU>, Window> window)
		{
			return await await Fusion.Application.MainThread.InvokeAsync(() =>
				{
					var childWindow = new WindowsDialog<TU>();

					var model = window(childWindow);

					if (model.Style == WindowStyle.Sheet)
					{
						var content = model.Size.HasValue
							? model.Content.WithSize(model.Size.Value.NotNone().Transpose())
							: model.Content;

						childWindow._modalDialogs = _modalDialogs;
						childWindow._parent = _parent;
						childWindow._onClose = () =>
							_modalDialogs.OnNext(_modalDialogs.Value.Remove(content));

						_modalDialogs.OnNext(_modalDialogs.Value.Add(content));
					}
					else
					{
						var modalDialogs = new BehaviorSubject<IImmutableList<IControl>>(ImmutableList<IControl>.Empty);

						var wnd = WindowImplementation.Initialize(OverlayModalDialogs(model, modalDialogs));

						childWindow._modalDialogs = modalDialogs;
						childWindow._parent = wnd;
						childWindow._onClose = () =>
						{
							Fusion.Application.MainThread.Schedule(wnd.Close);
						};

						wnd.Show();
					}

					return childWindow._result.FirstAsync();
				});
		}

		static Window OverlayModalDialogs(Window model, IObservable<IImmutableList<IControl>> modalDialogs)
		{
			model.Content = model.Content.WithNativeOverlay(
				modalDialogs
					.Select(dialogs =>
						dialogs.LastOrNone()
							.Select(dialog => dialog.Center().WithBackground(Color.Black.WithAlpha(a: 0.8f)))
							.Or(Control.Empty))
					.Switch());

			return model;
		}

		public Task<Optional<AbsoluteDirectoryPath>> BrowseForDirectory(AbsoluteDirectoryPath directory)
		{
			var dialogs = new FileDialogs(_parent.OrDefault());
			return dialogs.SelectDirectory(new DirectoryDialogOptions("Browse", directory));
		}

		public async Task<Optional<AbsoluteFilePath>> BrowseForFile(AbsoluteDirectoryPath directory, params FileFilter[] filters)
		{
			var dialogs = new FileDialogs(_parent.OrDefault());
			return from dialogResult in await dialogs.OpenFile(new FileDialogOptions("Browse", directory, filters))
				select dialogResult.Path;
		}
	}
}
