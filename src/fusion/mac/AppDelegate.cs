using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using AppKit;
using Foundation;

namespace Outracks.Fusion
{
	class ShutdownException : Exception
	{
	}

	public class AppDelegate : NSApplicationDelegate
	{
		public static bool ThrowOnTerminate { get; set; }

		readonly Subject<Unit> _terminates = new Subject<Unit>();
		public IObservable<Unit> Terminates
		{
			get { return _terminates; }
		}

		readonly bool _isDocumentApp;
		public AppDelegate(bool isDocumentApp)
		{
			_isDocumentApp = isDocumentApp;
		}

		public AppDelegate(IntPtr handle)
			: base(handle)
		{
		}

		public override void WillTerminate(NSNotification notification)
		{
			_terminates.OnNext(Unit.Default);
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
		{
			return true;
		}

		public override bool ApplicationShouldOpenUntitledFile(NSApplication sender)
		{
			if(_isDocumentApp)
				Application.LaunchedWithoutDocuments();

			return false;
		}

		public override NSApplicationTerminateReply ApplicationShouldTerminate (NSApplication sender)
		{
			if (!ThrowOnTerminate)
				return NSApplicationTerminateReply.Now;

			Application.MainThread.Schedule(() =>
			{
				throw new ShutdownException();
			});

			return NSApplicationTerminateReply.Later;
		}
	}

	/*
	 *
	 *
			void SetWindowSize(Size<Points> windowSize)
		{
			var currentFrame = _window.Frame;
			var origin = new PointF(currentFrame.X, currentFrame.Y + currentFrame.Height);

			var titleBarHeight = currentFrame.Height - _window.ContentView.Frame.Height;

			var sizeWithBar = new SizeF((float)windowSize.Width, (float)windowSize.Height + titleBarHeight);
			origin.Y -= sizeWithBar.Height;

			_resizeNotDoneByUser = true;
			try
			{
				_window.SetFrame(new RectangleF(origin, sizeWithBar), true, true);
			}
			finally
			{
				_resizeNotDoneByUser = false;
			}
		}
	 *
	 *
	public partial class AppDelegate : NSApplicationDelegate, IMainThread
	{
		void Initialize()
        {
        }

        partial void HandleFileOpen(NSObject sender)
        {
            var dialog = new NSOpenPanel()
            {
                Title = "Select your .unoproj",
                Prompt = "Build & Run",
                AllowedFileTypes = new[] { "unoproj" },
            };

            dialog.Begin(result => {
            });
        }

        partial void HandleFileClose(NSObject sender)
        {
            var keyWindow = NSApplication.SharedApplication.KeyWindow;

            if (keyWindow != null)
                keyWindow.Close();
        }

        partial void HandleViewRefresh(NSObject sender)
        {
        }

        partial void HandleViewEnableFullscreen(NSObject sender)
        {
        }

        partial void HandleWindowOutput(NSObject sender)
        {
        }

        public override void FinishedLaunching(NSObject notification)
        {
            Initialize();
        }

        public override void WillTerminate(NSNotification notification)
        {
        }

        public override bool OpenFile(NSApplication sender, string filename)
        {
            return true;
        }

        void IMainThread.EnsureMainThread()
        {
            NSApplication.EnsureUIThread();
        }
    }
    */
}
