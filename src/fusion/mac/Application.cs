using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using ObjCRuntime;
using Outracks.Fuse;
using Outracks.Fuse.Analytics;
using Outracks.IO;

namespace Outracks.Fusion.Mac
{
	//using UnoHost;

	public class Application : IApplication
	{
		public ITrayApplication CreateTrayApplication(IReport errorHandler, IObservable<string> title, Menu menu, IObservable<Icon> icon)
		{
			return new MonoMacTrayApplication(errorHandler, icon, title, menu);
		}

		public bool InitializeDocumentApp(IList<string> args, string applicationName)
		{
			Initialize(args, true);
			return true;
		}

		public void Initialize(IList<string> args)
		{
			Initialize(args, false);
		}

		IScheduler _dispatcher;

		public void Initialize(IList<string> args, bool isDocumentApp)
		{
			Console.CancelKeyPress += (sender, e) => Exit(0);

			NSApplication.Init();

			_dispatcher = _dispatcher ??  new Dispatcher(Thread.CurrentThread);
			Fusion.Application.MainThread = _dispatcher;

			Fusion.Application.PerFrame = Observable.Interval(
				TimeSpan.FromSeconds(1.0 / 60.0),
				new SynchronizationContextScheduler(SynchronizationContext.Current));

			var app = new AppDelegate(isDocumentApp);
			NSApplication.SharedApplication.Delegate = app;

			//TODO do we really need to retain this object?
			#pragma warning disable 0219
			var documentController = new NSDocumentController();
			#pragma warning restore 0219

			NSApplication.CheckForIllegalCrossThreadCalls = false;
			AppDelegate.ThrowOnTerminate = false;

			Desktop = new MacDialog<object>();

			EffectsImplementation.Initialize(_dispatcher);

			// Make so that the window popups with focus.
			NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);

			MacEnvironment.Initialize(new MacEnvironmentImpl());
			DraggingImplementation.Initialize(_dispatcher);
			MenuBuilder.Initialize(_dispatcher);
			LayeringImplementation.Initialize(_dispatcher);
			LabelImplementation.Initialize(_dispatcher);
			SliderImplementation.Initialize(_dispatcher);
			LineImplementation.Initialize(_dispatcher);
			CircleImplementation.Initialize(_dispatcher);
			RectangleImplementation.Initialize(_dispatcher);
			TextBoxImplementation.Initialize(_dispatcher);
			CursorsImplementation.Initialize(_dispatcher);
			ToolTipImplementation.Initialize(_dispatcher);
			ContextMenuImplementation.Initialize(_dispatcher);
			DropDownImplementation.Initialize(_dispatcher);
			ButtonImplementation.Initialize(_dispatcher);
			ColorPickerImplementation.Initialize(_dispatcher);
			ScrollingImplementation.Initialize(_dispatcher);
			LogViewImplementation.Initialize(_dispatcher);
			Transformation.Initialize();
			ImageImplementation.Initialize(_dispatcher);
			OverlayImplementation.Initialize(_dispatcher);
			PointerImplementation.Initialize(_dispatcher);
			KeyboardImplementation.Initialize(_dispatcher);
			WebViewImplementation.Initialize(_dispatcher);
			LayoutTrackerImplementation.Initialize();
			MessageBoxImplementation.Initialize();
			FileDialogs.Initialize();

			// TODO: Fix this!!!
			Clipping.Initialize((control, clip) => control);

			// This notification occurs a _lot_, but is the most specific one I was able to find that would
			//  allow us to be notified when the first responder changes consistently
			NSNotificationCenter.DefaultCenter.AddObserver(new NSString("NSApplicationDidUpdateNotification"), _ => {
				var keyWindow = NSApplication.SharedApplication.KeyWindow;
				_firstResponder.OnNext(keyWindow != null ? keyWindow.FirstResponder : null);
			});

			app.Terminates.Subscribe(_ =>
			{
				if (Terminating != null) Terminating();
			});

			EditMenu =
				  /*CreateFirstResponderMenuItem(name: "Undo", hotkey: HotKey.Create(Fusion.ModifierKeys.Meta, Key.Z), selectorName: "undo:")
				+ CreateFirstResponderMenuItem(name: "Redo", hotkey: HotKey.Create(Fusion.ModifierKeys.Meta | Fusion.ModifierKeys.Shift, Key.Z), selectorName: "redo:")
				+ MenuItem.CreateSeparator()
				+ */CreateFirstResponderMenuItem(name: Strings.SubMenu_Edit_Cut, hotkey: HotKey.Create(ModifierKeys.Meta, Key.X), selectorName: "cut:")
				+ CreateFirstResponderMenuItem(name: Strings.SubMenu_Edit_Copy, hotkey: HotKey.Create(ModifierKeys.Meta, Key.C), selectorName: "copy:")
				+ CreateFirstResponderMenuItem(name: Strings.SubMenu_Edit_Paste, hotkey: HotKey.Create(ModifierKeys.Meta, Key.V), selectorName: "paste:")
				+ CreateFirstResponderMenuItem(name: "Paste and Match Style", hotkey: HotKey.Create(ModifierKeys.Meta | ModifierKeys.Shift, Key.V), selectorName: "pasteAsPlainText:")
				+ CreateFirstResponderMenuItem(name: Strings.SubMenu_Edit_Delete, hotkey: HotKey.Create(ModifierKeys.Meta, Key.Backspace), selectorName: "delete:")
				+ CreateFirstResponderMenuItem(name: Strings.SubMenu_Edit_SelectAll, hotkey: HotKey.Create(ModifierKeys.Meta, Key.A), selectorName: "selectAll:");
		}

		ISubject<NSResponder> _firstResponder = new BehaviorSubject<NSResponder>(null);

		Menu CreateFirstResponderMenuItem(string name, HotKey hotkey, string selectorName)
		{
			var selector = new Selector(selectorName);
			var command = _firstResponder
				.DistinctUntilChanged()
				.Switch(r =>
					Command.Create(
						action: () =>
						{
							// TODO: This is supposed to take an argument which I believe is the native
							//  NSMenuItem handle, but it appears to work fine without it. Though, when
							//  we have that available we should definitely pass it in here.
							Messaging.void_objc_msgSend(r.Handle, selector.Handle);
						},
						// TODO: This is not strictly correct, but we need the native NSMenuItem instance
						//  in order to implement the proper enabling procedure. For more info on how this
						//  is supposed to work, see:
						//  https://developer.apple.com/library/content/documentation/Cocoa/Conceptual/MenuList/Articles/EnablingMenuItems.html#//apple_ref/doc/uid/20000261-74653-BAJBGJHB
						isEnabled: r != null && r.RespondsToSelector(selector)));
			return Menu.Item(name, command, hotkey: hotkey);
		}

		public event Action Terminating;

		public Task<IDocument> OpenDocument(AbsoluteFilePath path, bool giveFocus = false)
		{
			return ObservableNSDocument.Open(path, giveFocus);
		}

		public IObservable<Window> DocumentOpened
		{
			get { return ObservableNSDocument.DocumentOpened; }
		}

		internal static NSMenu GlobalMenu;

		Optional<Menu> _defaultMenu;
		public Optional<Menu> DefaultMenu
		{
			get { return _defaultMenu; }
			set
			{
				_defaultMenu = value;
				var systemId = SystemGuidLoader.LoadOrCreateOrEmpty();
				_defaultMenu.Do(menu =>
					MenuBuilder
						.CreateMenu(menu, ReportFactory.GetReporter(systemId, Guid.NewGuid(), "Menu"))
						.ToObservable()
						.Subscribe(m =>
							GlobalMenu = m));
			}
		}

		public IDialog<object> Desktop { get; private set; }
		public Menu EditMenu { get; private set; }


		public void Run()
		{
			NSApplication.SharedApplication.Run();
		}

		public void Run(Window window)
		{
			NSApplication.SharedApplication.Run();
		}

		public void Exit(byte exitCode)
		{
			try
			{
				// HACK: Kill the current process immediately.
				Console.WriteLine("Killing current process...");
				System.Diagnostics.Process.GetCurrentProcess().Kill();
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e);
			}

			// TODO: The following hangs for a long period of time, using 100% CPU.
			_dispatcher.Schedule(() => NSApplication.SharedApplication.Terminate(NSApplication.SharedApplication));
		}

		public void ShowOpenDialog(FileFilter[] documentTypes)
		{
			_dispatcher.Schedule(() => NSDocumentController.SharedDocumentController.OpenDocument(null));
		}
	}

	class MacDialog<T> : IDialog<T>
	{
		public static void ShowDocumentWindow(ObservableNSDocument document, Func<IDialog<object>, Window> window)
		{
			Fusion.Application.MainThread.Schedule(() =>
			{
				var childWindow = new MacDialog<object>();

				var handle = WindowImplementation.Create(window(childWindow), Optional.Some(document));

				childWindow._parent = handle;
				childWindow._onClose = () =>
					Fusion.Application.MainThread.Schedule(document.Close);

				document.AddWindowController(new DocumentWindowController(handle));
			});
		}

		readonly ISubject<T> _result = new ReplaySubject<T>(1);
		Action _onClose = () => { };
		Optional<NSWindow> _parent;

		public void Focus()
		{
			_parent.Do(window =>
				Fusion.Application.MainThread.Schedule(() =>
					window.MakeKeyAndOrderFront(window)));
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
				var childWindow = new MacDialog<object>();

				var handle = WindowImplementation.Create(window(childWindow), Optional.None());
				handle.ReleasedWhenClosed = false;

				var close = new Subject<bool>();
				childWindow._parent = handle;
				childWindow._onClose = () => close.OnNext(false);

				isVisible.Merge(close).Subscribe(vis =>
					Fusion.Application.MainThread.Schedule(() =>
					{
						if (vis)
							handle.MakeKeyAndOrderFront(handle);
						else
							handle.OrderOut(handle);
					}));
			});
		}

		public async Task<TU> ShowDialog<TU>(Func<IDialog<TU>, Window> window)
		{
			return await await Fusion.Application.MainThread.InvokeAsync(() =>
				{
					var childWindow = new MacDialog<TU>();

					var model = window(childWindow);
					var handle = WindowImplementation.Create(model, Optional.None());

					childWindow._parent = handle;
					childWindow._onClose = () =>
						Fusion.Application.MainThread.Schedule(() =>
						{
							if (model.Style == WindowStyle.Sheet)
							{
								NSApplication.SharedApplication.EndSheet(handle);
								handle.OrderOut(handle);
							}
							else
							{
								handle.Close();
							}
						});

					if (model.Style == WindowStyle.Sheet)
						NSApplication.SharedApplication.BeginSheet(handle, _parent.Value);
					else
						handle.MakeKeyAndOrderFront(handle);

					return childWindow._result.FirstAsync();
				});
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
