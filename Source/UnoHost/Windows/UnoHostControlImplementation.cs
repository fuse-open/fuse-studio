using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using SharpDX;
using SharpDX.Direct3D9;

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
			var useWindowBasedSurface = true;

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
					if (useWindowBasedSurface)
						return CreateWindowBasedSurface(dispatcher, location, contextMenu, unoHost, onFocused, windowCreated, initialize, control, gotVersion);
					else
						return CreateTextureBasedSurface(dispatcher, location, messagesToHost);
				});

			return control;
		}

		static object CreateWindowBasedSurface(
				IScheduler dispatcher,
				IMountLocation location,
				Menu contextMenu,
				UnoHostProcess unoHost,
				Command onFocused,
				IConnectableObservable<IntPtr> windowCreated,
				Action<IUnoHostControl> initialize,
				UnoHostControlImplementation control,
				Action<OpenGlVersion> gotVersion)
		{
			var dummyControl = Shapes.Rectangle();
			dummyControl.Mount(location);
			var dummyElement = (FrameworkElement)dummyControl.NativeHandle;

			location.IsRooted
				.ObserveOn(dispatcher)
				.SubscribeUsing(
					rooted => rooted
						? ContextMenuImplementation.AddMenuTemporarily(dummyElement, contextMenu, dispatcher)
						: Disposable.Empty);

			var mainWindow = DataBinding
				.ObservableFromNativeEvent<object>(dummyElement, "LayoutUpdated")
				.StartWith(new object())
				.Select(
					_ =>
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

			var mainWindowHandle = mainWindow.Select(
					currentWindow =>
					{
						var hwndSource = (HwndSource)PresentationSource.FromVisual(currentWindow);
						if (hwndSource == null)
							return Optional.None();

						return Optional.Some(hwndSource.Handle);
					})
				.NotNone()
				.Replay(1);

			mainWindowHandle.Connect();

			var focused = unoHost.Receieve(WindowFocusMessage.TryParse)
				.Where(focusState => focusState == FocusState.Focused);

			focused.CombineLatest(mainWindowHandle, (a, b) => b)
				.ObserveOn(dispatcher)
				.Subscribe(t => WinApi.ShowWindow(t, WinApi.ShowWindowEnum.ShowNoActivate));

			unoHost.Receieve(WindowContextMenuMessage.TryParse)
				.ObserveOn(dispatcher)
				.Subscribe(t => dummyElement.ContextMenu.IsOpen = t);

			unoHost.Receieve(WindowMouseScrollMessage.TryParse)
				.ObserveOn(dispatcher)
				.Subscribe(
					deltaWheel =>
					{
						var delta = deltaWheel;
						var routedEvent = Mouse.MouseWheelEvent;

						dummyElement.RaiseEvent(
							new MouseWheelEventArgs(
								Mouse.PrimaryDevice,
								Environment.TickCount,
								delta)
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
				.WithLatestFromBuffered(mainWindow, (t, w) => new { Keys = t, Window = w })
				.ObserveOn(dispatcher)
				.Subscribe(
					t =>
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
				.CombineLatest(
					windowCreated,
					mainWindowHandle,
					(viewportHwnd, mainHwnd) => new
					{
						ViewportHwnd = viewportHwnd,
						MainHwnd = mainHwnd,
					})
				.ObserveOn(dispatcher)
				.SubscribeUsing(
					t =>
						overlayForm.BindTo(t.ViewportHwnd, t.MainHwnd, dummyControl, dummyElement.GetDpi()));

			unoHost.Messages
				.TryParse(Ready.MessageType, Ready.ReadDataFrom)
				.Take(1)
				.Subscribe(_ => initialize(control));

			unoHost.Messages
				.SelectSome(OpenGlVersionMessage.TryParse)
				.Subscribe(gotVersion);

			unoHost.Messages.Connect();

			return dummyElement;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct Vertex
		{
			public Vector4 Position;
			public Vector2 UV;
			public SharpDX.Color Color;
		}

		// Create a texture based surface where a DX9 texture is shared with the UnoHost and used as backbuffer in the UnoHost.
		static FrameworkElement CreateTextureBasedSurface(IScheduler dispatcher, IMountLocation location, IObserver<IBinaryMessage> messagesToHost)
		{
			var d3dSource = new D3DImage(2.0 * 96.0, 2.0 * 96.0);
			var hInstance = Marshal.GetHINSTANCE(typeof(UnoHostControlImplementation).Module);
			var hwnd = CreateWindowEx(0, "button", "Dummy", WindowStyles.WS_OVERLAPPEDWINDOW, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);

			var device = new DeviceEx(
				new Direct3DEx(),
				0,
				DeviceType.Hardware,
				hwnd,
				CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve,
				new PresentParameters((int)1, (int)1)
				{
					Windowed = true,
					SwapEffect = SwapEffect.Discard,
					BackBufferFormat = Format.A8R8G8B8
				});

			var vertices = new VertexBuffer(device, 4 * 28, Usage.WriteOnly, VertexFormat.None, Pool.Default);

			vertices.Lock(0, 0, LockFlags.None).WriteRange(new[] {
				new Vertex() { Position = new Vector4(-1, -1, 0, 1.0f), UV = new Vector2(0,0), Color = SharpDX.Color.White },
				new Vertex() { Position = new Vector4(1, -1, 0, 1.0f), UV = new Vector2(1,0), Color = SharpDX.Color.White },
				new Vertex() { Position = new Vector4(-1, 1, 0, 1.0f), UV = new Vector2(0,1), Color = SharpDX.Color.White },
				new Vertex() { Position = new Vector4(1, 1, 0, 1.0f), UV = new Vector2(1,1), Color = SharpDX.Color.White },
			});
			vertices.Unlock();

			var vertexElems = new[] {
				new VertexElement(0, 0, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.Position, 0),
				new VertexElement(0, 16, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
				new VertexElement(0, 24, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0),
				VertexElement.VertexDeclarationEnd
			};
			var vertexDecl = new VertexDeclaration(device, vertexElems);

			var image = new System.Windows.Controls.Image
			{
				Source = d3dSource
			};

			device.SetRenderState(RenderState.Lighting, false);
			device.SetRenderState(RenderState.CullMode, Cull.Clockwise);

			var lastFrameTime = TimeSpan.FromSeconds(0);
			var currentSize = new Size<Pixels>();
			Observable.FromEventPattern(
					handler =>
					{
						CompositionTarget.Rendering += handler;
					},
					handler =>
					{
						CompositionTarget.Rendering -= handler;
					})
				.Subscribe(
					evt =>
					{
						var frameArgs = (RenderingEventArgs)evt.EventArgs;
						if (lastFrameTime == frameArgs.RenderingTime) return;

						lastFrameTime = frameArgs.RenderingTime;
						d3dSource.Lock();
						device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, SharpDX.Color.Black, 1.0f, 0);
						device.BeginScene();

						device.SetStreamSource(0, vertices, 0, 28);
						device.VertexDeclaration = vertexDecl;
						device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

						device.EndScene();
						d3dSource.AddDirtyRect(new Int32Rect(0, 0, (int)currentSize.Width, (int)currentSize.Height));
						d3dSource.Unlock();
					});

			location.NativeFrame
				.Size
				.Transpose()
				.ConnectWhile(location.IsRooted)
				.DistinctUntilChanged()
				.Where(s => !s.HasZeroArea())
				.CombineLatest(image.GetDpi(), (size, dpi) =>
				{
					var pixelSize = size * dpi;
					return new { pixelSize, dpi };
				})
				.ObserveOn(Fusion.Application.MainThread)
				.Subscribe(
					data =>
					{
						var size = data.pixelSize;
						device.Reset(new PresentParameters((int)size.Width, (int)size.Height)
						{
							Windowed = true,
							SwapEffect = SwapEffect.Discard,
							BackBufferFormat = Format.A8R8G8B8
						});

						var surface = Surface.CreateRenderTarget(device, (int)size.Width, (int)size.Height, Format.A8R8G8B8, MultisampleType.None, 0, false);
						device.SetRenderTarget(0, surface);

						d3dSource.Lock();
						d3dSource.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
						d3dSource.Unlock();

						var sharedTexHandle = IntPtr.Zero;
						var texture = new Texture(device, (int)size.Width, (int)size.Height, 1, Usage.NonSecure, Format.A8R8G8B8, Pool.Default, ref sharedTexHandle);
						device.SetTexture(0, texture);

						currentSize = size;
						messagesToHost.OnNext(SetSurfaceMessage.Compose(new TextureWithSize(sharedTexHandle, size, data.dpi)));
					});

			image.MouseDown += (s, a) => image.CaptureMouse();
			image.MouseUp += (s, a) => image.ReleaseMouseCapture();

			Observable.FromEventPattern<System.Windows.Input.MouseEventArgs>(image, "MouseDown")
				.Select(a => a.EventArgs.GetPosition(image).ToFusion())
				.WithLatestFromBuffered(image.GetDpi(), (loc, dpi) => loc * dpi)
				.ObserveOn(TaskPoolScheduler.Default)
				.Select(pixelLoc => MouseEventMessage.Compose(new MouseEventArgs(MouseEventType.MouseDown, pixelLoc)))
				.Subscribe(messagesToHost);

			Observable.FromEventPattern<System.Windows.Input.MouseEventArgs>(image, "MouseUp")
				.Select(a => a.EventArgs.GetPosition(image).ToFusion())
				.WithLatestFromBuffered(image.GetDpi(), (loc, dpi) => loc * dpi)
				.ObserveOn(TaskPoolScheduler.Default)
				.Select(pixelLoc => MouseEventMessage.Compose(new MouseEventArgs(MouseEventType.MouseUp, pixelLoc)))
				.Subscribe(messagesToHost);

			/*Observable.FromEventPattern<System.Windows.Input.MouseEventArgs>(image, "MouseMove")
				.Sample(TimeSpan.FromMilliseconds(1))
				.Select(a => a.EventArgs.GetPosition(image).ToFusion())
				.WithLatestFrom(image.GetDpi(), (loc, dpi) => loc * dpi)
				.ObserveOn(TaskPoolScheduler.Default)
				.Select(pixelLoc => MouseEventMessage.Compose(new MouseEventArgs(MouseEventType.MouseMove, pixelLoc)))
				.Subscribe(messagesToHost);*/

			location.BindNativeDefaults(image, dispatcher);
			return image;
		}

		[DllImport("user32.dll", SetLastError = true)]
		static extern IntPtr CreateWindowEx(
			uint dwExStyle,
			[MarshalAs(UnmanagedType.LPStr)] string lpClassName,
			[MarshalAs(UnmanagedType.LPStr)] string lpWindowName,
			WindowStyles dwStyle,
			int x,
			int y,
			int nWidth,
			int nHeight,
			IntPtr hWndParent,
			IntPtr hMenu,
			IntPtr hInstance,
			IntPtr lpParam);

	}

	[Flags()]
	enum WindowStyles : uint
	{
		/// <summary>The window has a thin-line border.</summary>
		WS_BORDER = 0x800000,

		/// <summary>The window has a title bar (includes the WS_BORDER style).</summary>
		WS_CAPTION = 0xc00000,

		/// <summary>The window is a child window. A window with this style cannot have a menu bar. This style cannot be used with the WS_POPUP style.</summary>
		WS_CHILD = 0x40000000,

		/// <summary>Excludes the area occupied by child windows when drawing occurs within the parent window. This style is used when creating the parent window.</summary>
		WS_CLIPCHILDREN = 0x2000000,

		/// <summary>
		/// Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message, the WS_CLIPSIBLINGS style clips all other overlapping child windows out of the region of the child window to be updated.
		/// If WS_CLIPSIBLINGS is not specified and child windows overlap, it is possible, when drawing within the client area of a child window, to draw within the client area of a neighboring child window.
		/// </summary>
		WS_CLIPSIBLINGS = 0x4000000,

		/// <summary>The window is initially disabled. A disabled window cannot receive input from the user. To change this after a window has been created, use the EnableWindow function.</summary>
		WS_DISABLED = 0x8000000,

		/// <summary>The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar.</summary>
		WS_DLGFRAME = 0x400000,

		/// <summary>
		/// The window is the first control of a group of controls. The group consists of this first control and all controls defined after it, up to the next control with the WS_GROUP style.
		/// The first control in each group usually has the WS_TABSTOP style so that the user can move from group to group. The user can subsequently change the keyboard focus from one control in the group to the next control in the group by using the direction keys.
		/// You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
		/// </summary>
		WS_GROUP = 0x20000,

		/// <summary>The window has a horizontal scroll bar.</summary>
		WS_HSCROLL = 0x100000,

		/// <summary>The window is initially maximized.</summary> 
		WS_MAXIMIZE = 0x1000000,

		/// <summary>The window has a maximize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary> 
		WS_MAXIMIZEBOX = 0x10000,

		/// <summary>The window is initially minimized.</summary>
		WS_MINIMIZE = 0x20000000,

		/// <summary>The window has a minimize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary>
		WS_MINIMIZEBOX = 0x20000,

		/// <summary>The window is an overlapped window. An overlapped window has a title bar and a border.</summary>
		WS_OVERLAPPED = 0x0,

		/// <summary>The window is an overlapped window.</summary>
		WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_SIZEFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,

		/// <summary>The window is a pop-up window. This style cannot be used with the WS_CHILD style.</summary>
		WS_POPUP = 0x80000000u,

		/// <summary>The window is a pop-up window. The WS_CAPTION and WS_POPUPWINDOW styles must be combined to make the window menu visible.</summary>
		WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,

		/// <summary>The window has a sizing border.</summary>
		WS_SIZEFRAME = 0x40000,

		/// <summary>The window has a sizing border. Same as the WS_SIZEBOX style.</summary>
		WS_THICKFRAME = 0x00040000,

		/// <summary>The window has a window menu on its title bar. The WS_CAPTION style must also be specified.</summary>
		WS_SYSMENU = 0x80000,

		/// <summary>
		/// The window is a control that can receive the keyboard focus when the user presses the TAB key.
		/// Pressing the TAB key changes the keyboard focus to the next control with the WS_TABSTOP style.  
		/// You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
		/// For user-created windows and modeless dialogs to work with tab stops, alter the message loop to call the IsDialogMessage function.
		/// </summary>
		WS_TABSTOP = 0x10000,

		/// <summary>The window is initially visible. This style can be turned on and off by using the ShowWindow or SetWindowPos function.</summary>
		WS_VISIBLE = 0x10000000,

		/// <summary>The window has a vertical scroll bar.</summary>
		WS_VSCROLL = 0x200000
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
