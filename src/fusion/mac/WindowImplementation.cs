using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using AppKit;
using CoreGraphics;
using Foundation;
using Outracks.Fuse.Analytics;

namespace Outracks.Fusion.Mac
{
	static class CustomWindowNotifications
	{
		public static string WindowMakeFirstResponder { get { return "WindowMakeFirstResponder"; } }
	}

	public class CustomWindow : NSWindow
	{
		readonly Subject<Unit> _focused = new Subject<Unit>();

		readonly BehaviorSubject<bool> _isShowing = new BehaviorSubject<bool>(true);
		public IObservable<bool> IsShowing { get { return _isShowing; }}

		public CustomWindow(IntPtr handle) : base(handle)
		{
		}

		public CustomWindow(Command focused)
		{
			focused.Execute.Sample(_focused).Subscribe(c => c());
		}

		public override bool MakeFirstResponder(NSResponder aResponder)
		{
			var result = base.MakeFirstResponder(aResponder);
			if (result)
			{
				var notification = NSNotification.FromName(CustomWindowNotifications.WindowMakeFirstResponder, aResponder);
				NSNotificationCenter.DefaultCenter.PostNotification(notification);
			}
			return result;
		}

		public override bool CanBecomeKeyWindow
		{
			get { return true; }
		}

		// Updated by the window controller when a window title is requested for the document name
		//  Note that this may not actually happen if our window doesn't have a document, but that's
		//  not really a case we care about.
		public readonly ISubject<string> DocumentTitle = new BehaviorSubject<string>("");

		public readonly ISubject<NSButton> DocumentIconButton = new ReplaySubject<NSButton>(1);


		public override void MakeKeyAndOrderFront(NSObject sender)
		{
			NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
			base.MakeKeyAndOrderFront(sender);

			// This icon never seems to be available at window creation; it gets created sometime later, but
			//  before MakeKeyAndOrderFront is called it seems
			var button = StandardWindowButton(NSWindowButton.DocumentIconButton);

			// Hide title bar (note that other elements like the traffic light buttons will still be shown)
			//  This must be done _after_ we grab the DocumentIconButton from the window, otherwise we're unable
			//  to get a reference to it in the first place.
			TitleVisibility = NSWindowTitleVisibility.Hidden;

			if (button != null) // This shouldn't happen for a document window, but can happen.
				DocumentIconButton.OnNext(button);

			_focused.OnNext(Unit.Default);
			_isShowing.OnNext(true);
		}

		public override void OrderOut(NSObject sender)
		{
			base.OrderOut(sender);
			_isShowing.OnNext(false);
		}

		public readonly ISubject<bool> IsRooted = new ReplaySubject<bool>(1);
	}

	class DocumentWindowController : NSWindowController
	{
		public DocumentWindowController(CustomWindow window)
			: base(window)
		{
		}

		public DocumentWindowController(IntPtr handle)
			: base(handle)
		{
		}

		public override string WindowTitleForDocumentDisplayName(string displayName)
		{
			var res = base.WindowTitleForDocumentDisplayName(displayName);

			var window = (CustomWindow)Window;
			window.DocumentTitle.OnNext(res);

			return res;
		}
	}

	public static class WindowImplementation

	{
		static int _WindowCount = 0;

		public static CustomWindow Create(Window model, Optional<ObservableNSDocument> document)
		{
			var dispatcher = Fusion.Application.MainThread;

			model.Title = model.Title.Select(title => model.HideTitle ? "" : title);

			var sizeFeedback = model.Size
				.Or(Property.Create(Optional.None<Size<Points>>()))
				.Or(Size.Create<Points>(800, 600))
				.AutoInvalidate(TimeSpan.FromSeconds(2));

			var size = sizeFeedback.PreventFeedback();

			var content = new NSDefaultView();

			if (model.DragOperation.HasValue)
			{
				content.RegisterForDraggedTypes(new string[] { NSPasteboard.NSFilenamesType });
				content.AddDropOperation(model.DragOperation.Value);
			}

			var window = new CustomWindow(model.Focused)
			{
				BackgroundColor = Color.FromBytes(0x31,0x34,0x3a).ToNSColor(),
				ContentView = content,
				StyleMask = NSWindowStyle.TexturedBackground | /*NSWindowStyle.Utility |*/ NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable,
				HidesOnDeactivate = false,
				Restorable = false,
			};

			window.IsOpaque = false;
			content.WantsLayer = true;

			var fusionContent = model.Content;
			var desiredTitleBarHeight = new ReplaySubject<IObservable<Points>>(1);

			switch (model.Style)
			{
				case WindowStyle.Regular:
					// Render window content beneath title bar area
					window.StyleMask |= NSWindowStyle.FullSizeContentView;

					// Make title bar transparent
					window.TitlebarAppearsTransparent = true;

					// Build custom title bar content and dock it on top of existing fusionContent
					var titleTextColor = Color.White;

					var titleBarContent = document.MatchWith(_ =>
					{
						// If we have a document, we'll create a container to hijack the window's DocumentIconButton later
						var titleText = Label.Create(text: window.DocumentTitle.AsText(), color: titleTextColor);
						var documentIconButtonContainer = DocumentIconButtonContainer.Create(window).WithPadding(right: Optional.Some<Points>(4));
						return Layout.StackFromLeft(documentIconButtonContainer, titleText);
					},
					() => Label.Create(text: model.Title.AsText(), color: titleTextColor));

					Action zoom = () =>
						Fusion.Application.MainThread.Schedule(() => window.Zoom(window));

					titleBarContent = titleBarContent
						.WithPadding(top: Optional.Some<Points>(2), bottom: Optional.Some<Points>(3))
						.CenterHorizontally();

					desiredTitleBarHeight.OnNext(titleBarContent.DesiredSize.Height);

					fusionContent = Layout.DockTop(
						titleBarContent,
						fusionContent
					).OnMouse(doubleClicked: Command.Enabled(zoom));
					break;

				case WindowStyle.Fat:
					// Render window content beneath title bar area
					window.StyleMask |= NSWindowStyle.FullSizeContentView;

					//Create a toolbar
					window.Toolbar = new NSToolbar("toolbar");

					// Make title bar transparent
					window.TitlebarAppearsTransparent = true;

					window.Toolbar.ShowsBaselineSeparator = false;

					window.WillUseFullScreenPresentationOptions = (nsWindow, options) => options | NSApplicationPresentationOptions.AutoHideToolbar;

					// Build custom title bar content and dock it on top of existing fusionContent
					var titleTextColorFat = model.Foreground;

					var titleBarContentFat = document.MatchWith(_ =>
						{
							// If we have a document, we'll create a container to hijack the window's DocumentIconButton later
							var titleText = Label.Create(
									text: window.DocumentTitle.AsText(),
									color: titleTextColorFat,
									font: Font.SystemDefault(11),
									lineBreakMode: LineBreakMode.TruncateTail)
								.WithWidth(140)
								.Center();// Ensures the doc name can never run over the controls in compact mode

							return  titleText;
						},
						() => Label.Create(text: model.Title.AsText(), color: titleTextColorFat));

					// For some reason the toolbar sometimes causes double zoom events, this is a workaround
					bool zoomExpected = false;
					Action zoomFat = () =>
						Fusion.Application.MainThread.Schedule(() =>
						{
							try
							{
								zoomExpected = true;
								window.Zoom(window);
							}
							finally
							{
								zoomExpected = false;
							}
						});

					window.ShouldZoom = (_, __) => zoomExpected;


					titleBarContentFat = Layout.StackFromLeft(
												Control.Empty
													.WithWidth(80)
													.HideOnWindows(),
												Control.Empty
													.WithWidth(16)
													.HideOnMac(),
												titleBarContentFat)
											.WithPadding(top: Optional.Some<Points>(12))
											.DockTopLeft();

					desiredTitleBarHeight.OnNext(Observable.Return<Points>(0.0));

					fusionContent = fusionContent.OnMouse(doubleClicked: Command.Enabled(zoomFat)).WithOverlay(titleBarContentFat);
					break;

				case WindowStyle.None:
					window.StyleMask = NSWindowStyle.TexturedBackground | NSWindowStyle.Borderless;
					window.MovableByWindowBackground = true;
					window.BackgroundColor = NSColor.Clear;

					desiredTitleBarHeight.OnNext(Observable.Return<Points>(0.0));

					content.Layer.Frame = content.Frame;
					content.Layer.CornerRadius = 5.0f;
					content.Layer.MasksToBounds = true;
					break;
				case WindowStyle.Sheet:
					desiredTitleBarHeight.OnNext(Observable.Return<Points>(0.0));
					break;
				default:
					throw new NotImplementedException();
			}

			model.Size.Do(s =>
				s.IsReadOnly.ObserveOn(dispatcher).Subscribe(isReadOnly =>
				{
					if (isReadOnly)
						window.StyleMask &= ~NSWindowStyle.Resizable;
					else
						window.StyleMask |= NSWindowStyle.Resizable;
				}));

			var sizeFeedbackObservable = sizeFeedback.AsObservable();
			var sizeObservable = size.AsObservable();

			model.Title.ObserveOn(dispatcher).Subscribe(title => window.Title = title);
			sizeObservable.CombineLatest(
				desiredTitleBarHeight.Switch(),
				(s,h) => new Size<Points>(s.Width, s.Height + h)).ObserveOn(dispatcher).Subscribe(s => window.SetContentSize(s.ToSize()));

			window.WillClose += (sender, args) =>
			{
				// HACK: Kill the current process immediately.
				if (--_WindowCount == 0)
					Fusion.Application.Exit(0);
#if false
				// TODO: The following hangs for a long period of time, using 100% CPU.
				model.Closed.ExecuteOnce();
#endif
			}; // Window closed by user

			var observer = new WindowObserver();
			observer.DangerousRetain();
			window.AddObserver(observer, new NSString("visible"), NSKeyValueObservingOptions.New, IntPtr.Zero);

			window.DidResize += (s, a) => desiredTitleBarHeight.Switch()
                .Subscribe(titleBarHeight =>
                	size.Write((content.Frame.Size - new CGSize(0, (float)titleBarHeight)).ToFusion()));

			var transize = sizeFeedbackObservable.CombineLatest(
				desiredTitleBarHeight.Switch(),
				(s,h) => new Size<Points>(s.Width, s.Height + h)).Transpose();
			fusionContent.Mount(new MountLocation.Mutable
			{
				AvailableSize = transize,
				NativeFrame = ObservableMath.RectangleWithSize(transize),
				IsRooted = window.IsShowing
			});

			Fusion.Application.MainThread.Schedule(() =>
			{
				var nativeContent = fusionContent.NativeHandle as NSView;
				if (nativeContent != null)
				{
					content.AddSubview(nativeContent);
				}
			});

			var systemId = SystemGuidLoader.LoadOrCreateOrEmpty();
			model.Menu.Do(menu =>
				MenuBuilder
					.CreateMenu(menu, ReportFactory.GetReporter(systemId, Guid.NewGuid(), "Menu"))
					.ToObservable()
					.Subscribe(m =>
						window.Menu = m));

			window.Center();
			var centerposition = new Point<Points>((double)window.Frame.X, (double)window.Frame.Y);
			var position = model.Position
				.Or(Property.Create(Optional.None<Point<Points>>()))
				.Or(centerposition)
				.AutoInvalidate(TimeSpan.FromSeconds(2))
				.PreventFeedback();

			model.TopMost.Do(topMost =>
				topMost.Subscribe(t =>
					{
						window.Level = t ? NSWindowLevel.Floating : NSWindowLevel.Normal;
					}));

			position.ObserveOn(dispatcher).Subscribe(p =>
			{
				window.SetFrameOrigin(new CGPoint(p.X, p.Y));
			});
			window.DidMove += (s, a) =>
			{
				position.Write(new Point<Points>((double)window.Frame.Left, (double)window.Frame.Top));
			};

			window.DidBecomeMain += (s, a) => {
				if (window.Menu != null)
					NSApplication.SharedApplication.MainMenu = window.Menu;
				else
					NSApplication.SharedApplication.MainMenu = Application.GlobalMenu ?? new NSMenu();
			};

			_WindowCount++;
			return window;
		}
	}

	// This container will hijack the window's existing DocumentIconButton and place it inside here. This assumes there
	//  will always be _exactly one_ of these per window and that the window will actually have a DocumentIconButton
	//  when this factory creates the container view. It should _not_ be created if the window isn't expected to have
	//  a document associated with it.
	static class DocumentIconButtonContainer
	{
		public static IControl Create(CustomWindow window)
		{
			return Control.Empty; // TODO
			/*
			var size = new BehaviorSubject<Size<IObservable<Points>>>(ObservableMath.ZeroSize);
			return Control.Create(self =>
			{
				var button = window.DocumentIconButton.FirstAsync().Subscribe(s =>

				var container = Fusion.Dispatcher.MainThread.InvokeAsync(() =>
				{
					size.OnNext(Size.Create(Observable.Return<Points>((double)button.Bounds.Width), Observable.Return<Points>((double)button.Bounds.Height)));

					return new DocumentIconButtonContainerView(button);
				});

				self.BindNativeDefaults(container, Fusion.Dispatcher.MainThread);

				return container;
			})
			.WithSize(size.Switch());
			*/
		}
	}

	class DocumentIconButtonContainerView : NSView
	{
		readonly NSButton _documentIconButton;

		public DocumentIconButtonContainerView(IntPtr handle) : base(handle)
		{
		}

		public DocumentIconButtonContainerView(NSButton documentIconButton)
		{
			_documentIconButton = documentIconButton;

			var resizeMessages = new[] {
				"NSWindowDidResizeNotification",
				"NSWindowDidMoveNotification",
			};
			foreach (var message in resizeMessages)
			{
				// TODO: Leaky
				NSNotificationCenter.DefaultCenter.AddObserver(new NSString(message), _ => HijackDocumentIconButton());
			}
		}

		public override void ViewWillDraw()
		{
			HijackDocumentIconButton();

			base.ViewWillDraw();
		}

		void HijackDocumentIconButton()
		{
			AddSubview(_documentIconButton);
			_documentIconButton.SetFrameOrigin(new CGPoint());
		}
	}

	class WindowObserver : NSObject
	{
		public WindowObserver(IntPtr handle) : base(handle)
		{ }

		public WindowObserver()
		{ }

		public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
		{
			var customWindow = ofObject as CustomWindow;
			if (customWindow != null && keyPath == "visible")
			{
				try
				{
					customWindow.IsRooted.OnNext(customWindow.IsVisible);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
		}
	}
}
