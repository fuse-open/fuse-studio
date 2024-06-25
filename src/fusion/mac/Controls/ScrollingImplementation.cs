using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;

namespace Outracks.Fusion.Mac
{
	class DocumentViewForOpenGL : NSView
	{
		public DocumentViewForOpenGL()
		{
		}

		public DocumentViewForOpenGL(IntPtr handle) : base(handle)
		{
		}

		public new static bool IsCompatibleWithResponsiveScrolling
		{
			[Export("isCompatibleWithResponsiveScrolling")]
			get { return false; }
		}
	}

	static class ScrollingImplementation
	{
		public static void Initialize(IScheduler dispatcher)
		{
			Scrolling.Implementation.Factory = (content, darkTheme, supportsOpenGL, zoomAttribs, onBoundsChanged, scrollToRectangle, verticalScrollBarVisible, horizontalScrollBarVisible) =>
				Control.Create(self =>
				{
					var documentView = supportsOpenGL
						? new DocumentViewForOpenGL()
						: new NSView();

					var view = new NSScrollView
					{
						ContentView = new NSFlippedClipView(),
						DocumentView = documentView,
						DrawsBackground = false,
						HasVerticalScroller = verticalScrollBarVisible,
						HasHorizontalScroller = horizontalScrollBarVisible,
						AutohidesScrollers = true
					};

					var scrollBounds =
						Observable.Create<Unit>(
							observer =>
							{
								int recursionDetector = 0;
								Action updateAction = () =>
								{
									try
									{
										// NOTE: At some point while implementing this I got problems with infinite recursion
										// I managed to avoid that; however to be on the safe side I'm keeping this check here
										// just in case.
										//
										// - karsten
										recursionDetector++;

										if (recursionDetector > 10)
										{
											Console.WriteLine("ERROR: Aborting recursion in ScrollingImplementation to avoid stack overflow.");
											return;
										}

										observer.OnNext(Unit.Default);
									}
									finally
									{
										recursionDetector--;
									}
								};

								return Disposable.Combine(
									NSView.Notifications.ObserveBoundsChanged(view.ContentView, (s, e) => updateAction()),
									NSView.Notifications.ObserveFrameChanged(view.ContentView, (s, e) => updateAction()),
									NSView.Notifications.ObserveBoundsChanged(documentView, (s, e) => updateAction()),
									NSView.Notifications.ObserveFrameChanged(documentView, (s, e) => updateAction()),
									NSView.Notifications.ObserveFrameChanged(view, (s, e) => updateAction())
								);
							})
						.StartWith(Unit.Default)
						.Select(_ => new ScrollBounds(ConvertRectangle(view.DocumentVisibleRect), ConvertRectangle(documentView.Frame)))
						.DistinctUntilChanged()
						.Replay(1)
						.RefCount();

					var documentVisibleSize = scrollBounds.Select(x => x.Visible.Size).Transpose();
					var contentNativeFrame = ObservableMath.RectangleWithSize(content.DesiredSize.Max(documentVisibleSize));

					content.Mount(new MountLocation.Mutable
					{
						IsRooted = self.IsRooted,
						NativeFrame = contentNativeFrame,
						AvailableSize = documentVisibleSize
					});

					self.BindNativeDefaults(view, dispatcher);

					var subview = content.NativeHandle as NSView;


					self.BindNativeProperty(dispatcher, "theme", darkTheme, isDark =>
							view.ScrollerKnobStyle = isDark ? NSScrollerKnobStyle.Light : NSScrollerKnobStyle.Dark);

					// HACK:
					// To work around layout feedback problems we set the size of the view to 1 pixel less than the
					// actual available space. See https://github.com/fusetools/Fuse/pull/4703 for details
					self.BindNativeProperty(dispatcher, "containerSize", contentNativeFrame.Size.Transpose(), size =>
						documentView.SetFrameSize(new CGSize(Math.Max(0.0, size.Width - 1), Math.Max(0.0, size.Height - 1))));

					if (subview != null)
						dispatcher.Schedule(() => documentView.AddSubview(subview));

					self.BindNativeProperty(
						dispatcher,
						"scrollTarget",
						scrollToRectangle,
						r =>
						{
							var nsrect = new CGRect(r.Position.X, r.Position.Y, r.Size.Width, r.Size.Height);
							documentView.ScrollRectToVisible(nsrect);
						});

					onBoundsChanged.Do(
						handler =>
						{
							scrollBounds
								.ConnectWhile(self.IsRooted)
								.Subscribe(handler);
						});

					// Zoom is disabled until we have more time to implement it properly (with zoom controls and shortcuts etc)
					// See https://github.com/fusetools/Fuse/issues/2643
					//zoomAttribs.Do(attribs => EnableMagnification(view, attribs));

					return view;
				});
		}

		static Rectangle<Points> ConvertRectangle(CGRect nsrect)
		{
			return Rectangle.FromPositionSize<Points>(
				left: (double) nsrect.X,
				top: (double) nsrect.Y,
				width: (double) nsrect.Width,
				height: (double) nsrect.Height);
		}


		static readonly IntPtr SelAllowsMagnification = Selector.GetHandle("setAllowsMagnification:");
		static readonly IntPtr SelSetMinMagnification = Selector.GetHandle("setMinMagnification:");
		static readonly IntPtr SelSetMaxMagnification = Selector.GetHandle("setMaxMagnification:");

		static void EnableMagnification(NSScrollView view, ZoomAttributes attribs)
		{
			Messaging.void_objc_msgSend_bool(view.Handle, SelAllowsMagnification, true);
			Messaging.void_objc_msgSend_float(view.Handle, SelSetMinMagnification, attribs.MinZoom);
			Messaging.void_objc_msgSend_float(view.Handle, SelSetMaxMagnification, attribs.MaxZoom);
		}
	}
}
