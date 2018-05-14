using System;
using AppKit;
using Foundation;

namespace Outracks.UnoHost.OSX
{
	class ShutdownException : Exception
	{
	}
		
	public partial class AppDelegate : NSApplicationDelegate
	{
		public static bool ThrowOnTerminate { get; set; }

		public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
		{
			return true;
		}

		public override NSApplicationTerminateReply ApplicationShouldTerminate (NSApplication sender)
		{
			if (!ThrowOnTerminate)
				return NSApplicationTerminateReply.Now;

			NSApplication.SharedApplication.BeginInvokeOnMainThread(() => 
			{
				throw new ShutdownException();
			});
			
			return NSApplicationTerminateReply.Later;
		}
	}

	/* 
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
