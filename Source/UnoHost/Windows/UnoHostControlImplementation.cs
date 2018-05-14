using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace Outracks.Fusion.Windows
{
	using IO;
	using UnoHost;
	using UnoHost.Windows.Protocol;

	public class UnoHostControlFactory : IUnoHostControlFactory
	{
		public IUnoHostControl Create(
			AbsoluteFilePath assemblyPath,
			Command onFocused,
			Menu contextMenu,
			AbsoluteFilePath userDataPath,
			Action<IUnoHostControl> initialize,
			Action<OpenGlVersion> gotVersion,
			params string[] arguments)
		{
			UnoHostProcess.Application = ExternalApplication.FromNativeExe(typeof(UnoHostControlFactory).Assembly.GetCodeBaseFilePath());
			
			var dispatcher = Fusion.Application.MainThread;

			var messagesToHost = new Subject<IBinaryMessage>();

			var unoHost = UnoHostProcess.Spawn(assemblyPath, messagesToHost, userDataPath, /*TODO*/new List<string>());
				
			unoHost.Process.Subscribe(process => new Job().AddProcess(process.Id));
				
			var windowCreated = unoHost.Receieve(WindowCreatedMessage.TryParse).Replay(1);
			windowCreated.Connect();

			var control = new UnoHostControlImplementation()
			{
				_disposable = unoHost,
				Messages = unoHost.Messages,
				MessagesTo = messagesToHost,
				Process = unoHost.Process,
			};

			control.Control = 
				Fusion.Control.Create(location =>
				{
					var dummyControl = Shapes.Rectangle();
					dummyControl.Mount(location);
					var dummyElement = (FrameworkElement)dummyControl.NativeHandle;
						
					location.IsRooted
						.ObserveOn(dispatcher)
						.SubscribeUsing(rooted => rooted 
							? ContextMenuImplementation.AddMenuTemporarily(dummyElement, contextMenu, dispatcher)
							: Disposable.Empty);

					var mainWindow = DataBinding
						.ObservableFromNativeEvent<object>(dummyElement, "LayoutUpdated")
						.StartWith(new object())
						.Select(_ =>
						{
							var hwndSource1 = PresentationSource.FromVisual(dummyElement);
							if (hwndSource1 == null)
								return Optional.None<System.Windows.Window>();

							var window = hwndSource1.RootVisual as System.Windows.Window;
							if (window == null)
								return Optional.None<System.Windows.Window>();

							return Optional.Some(window);
						})
						.DistinctUntilChanged()
						.NotNone()
						.Replay(1).RefCount();

					var mainWindowHandle = mainWindow.Select(currentWindow => 
						{
							var hwndSource = (HwndSource) PresentationSource.FromVisual(currentWindow);
							if (hwndSource == null)
								return Optional.None();

							return Optional.Some(hwndSource.Handle);
						})
						.NotNone()
						.Replay(1);

					mainWindowHandle.Connect();

					var focused = unoHost.Receieve(WindowFocusMessage.TryParse)
						.Where(focusState => focusState == FocusState.Focused);

					focused.CombineLatest(mainWindowHandle, (a,b) => b)
						.ObserveOn(dispatcher)
						.Subscribe(t => WinApi.ShowWindow(t, WinApi.ShowWindowEnum.ShowNoActivate));

					unoHost.Receieve(WindowContextMenuMessage.TryParse)
						.ObserveOn(dispatcher)
						.Subscribe(t => dummyElement.ContextMenu.IsOpen = t);

					unoHost.Receieve(WindowMouseScrollMessage.TryParse)
						.ObserveOn(dispatcher)
						.Subscribe(deltaWheel =>
						{
							var delta = deltaWheel;
							var routedEvent = Mouse.MouseWheelEvent;

							dummyElement.RaiseEvent(
								new MouseWheelEventArgs(
									Mouse.PrimaryDevice,
									Environment.TickCount, delta)
									{											
										RoutedEvent = routedEvent,
										Source = dummyElement
									});
						});


					// This is a fix for a commit that "fixed" handling the shortcuts, but broke just
					// plain typing in a TextInput.
					// See https://github.com/fusetools/Fuse/issues/3342
					// and https://github.com/fusetools/Fuse/issues/3887

					// Workaround to fix a problem with handling shortcut keys in the application while 
					// the viewport has focus and the user is typing in an TextInput in the app. 
					// We have give the main window focus to handle the shortcut keys, but then 
					// the viewport will lose focus. Current work around is to only do this when 
					// the user presses down Ctrl.

					unoHost.Receieve(WindowKeyDown.TryParse)
						.WithLatestFromBuffered(mainWindow, (t,w) => new { Keys = t, Window = w })
						.ObserveOn(dispatcher)
						.Subscribe(t =>
						{
							var modifiers = System.Windows.Input.Keyboard.PrimaryDevice.Modifiers;
							var alt = modifiers.HasFlag(System.Windows.Input.ModifierKeys.Alt);

							// Activate the main window if the Ctrl-key was pressed.
							// Ctrl + Alt means AltGr which we want to keep in the Uno host e.g. for the '@' key on Nordic keyboards.
							if (t.Keys == Keys.ControlKey && !alt)
								t.Window.Activate();
						});

					focused
						.WithLatestFromBuffered(onFocused.Execute.ConnectWhile(dummyControl.IsRooted), (_, c) => c)
						.Subscribe(c => c());

					var overlayForm = new OverlayForm();

					Observable
						.CombineLatest(windowCreated, mainWindowHandle, 
							(viewportHwnd, mainHwnd) => new
							{
								ViewportHwnd = viewportHwnd, 
								MainHwnd = mainHwnd,
							})
						.ObserveOn(dispatcher)
						.SubscribeUsing(t => 
							overlayForm.BindTo(t.ViewportHwnd, t.MainHwnd, dummyControl, dummyElement.GetDpi()));

					unoHost.Messages
						.SelectMany(m => m.TryParse(Ready.MessageType, Ready.ReadDataFrom))
						.Take(1)
						.Subscribe(_ => initialize(control));

					unoHost.Messages
						.SelectSome(OpenGlVersionMessage.TryParse)
						.Subscribe(gotVersion);

					unoHost.Messages.Connect();


					return dummyElement;
				});

			return control;
		}
	}

	public class UnoHostControlImplementation : IUnoHostControl
	{

		public IDisposable _disposable;
		public void Dispose()
		{
			_disposable.Dispose();
		}

		public IControl Control { get; set; }
		public IConnectableObservable<IBinaryMessage> Messages { get; set; }
		public IObserver<IBinaryMessage> MessagesTo { get; set; }
		public IObservable<Process> Process { get; set; }
	}
}