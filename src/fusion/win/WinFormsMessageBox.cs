using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Interop;
using IWin32Window = System.Windows.Forms.IWin32Window;

namespace Outracks.Fusion.Windows
{
	class WinFormsMessageBox : IMessageBox
	{
		public static void Initialize()
		{
			Dialogs.MessageBox.Implementation = new WinFormsMessageBox();
		}

		public void BringToFront()
		{
			System.Windows.Application.Current.Dispatcher.Invoke(() => {
				// Bring all open windows to front.
				foreach (var window in OpenedWindows())
				{
					// https://stackoverflow.com/questions/257587/bring-a-window-to-the-front-in-wpf
					window.Activate();

					if (!window.Topmost)
					{
						window.Topmost = true;  // important
						window.Topmost = false; // important
					}

					window.Focus();
				}
			});
		}

		public bool ShowConfirm(string text, string caption, MessageBoxType type)
		{
			return System.Windows.Application.Current.Dispatcher.Invoke(() => {
				var owner = Owner();
				var result = MessageBox.Show(owner, text, caption,
					System.Windows.Forms.MessageBoxButtons.YesNo, Icon(type));
				return result == System.Windows.Forms.DialogResult.Yes;
			});
		}

		public void Show(string text, string caption, MessageBoxType type)
		{
			System.Windows.Application.Current.Dispatcher.Invoke(() => {
				var owner = Owner();
				MessageBox.Show(owner, text, caption,
					System.Windows.Forms.MessageBoxButtons.OK, Icon(type));
			});
		}

		static MessageBoxIcon Icon(MessageBoxType type)
		{
			switch (type)
			{
			case MessageBoxType.Error:
				return MessageBoxIcon.Error;
			case MessageBoxType.Information:
				return MessageBoxIcon.Information;
			default:
				return 0;
			}
		}

		static IWin32Window Owner()
		{
			// Last window is top-most (after BringToFront).
			var window = OpenedWindows().LastOrDefault();
			return window != null
				? new WindowWrapper(window)
				: null;
		}

		static IEnumerable<System.Windows.Window> OpenedWindows()
		{
			foreach (var obj in System.Windows.Application.Current.Windows)
				if (obj is System.Windows.Window wnd && wnd.IsVisible &&
						// FIXME: Hardcoded string.
						wnd.Title.Contains("fuse X"))
					yield return wnd;
		}

		class WindowWrapper : IWin32Window
		{
			public IntPtr Handle { get; }

			public WindowWrapper(System.Windows.Window window)
			{
				Handle = new WindowInteropHelper(window).Handle;
			}
		}
	}
}
