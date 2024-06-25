using AppKit;

namespace Outracks.Fusion.Mac
{
	class MessageBoxImplementation : IMessageBox
	{
		public static void Initialize()
		{
			Dialogs.MessageBox.Implementation = new MessageBoxImplementation();
		}

		public void BringToFront()
		{
			NSApplication app = NSApplication.SharedApplication;
			app.ActivateIgnoringOtherApps(true);

			// FIXME: Enumerate windows and give focus?
		}

		public bool ShowConfirm(string text, string caption, MessageBoxType type)
		{
            return MessageBox.ShowMessageBox(text, caption,
					MessageBoxButtons.YesNo,
					MessageBoxType.Information,
					MessageBoxDefaultButton.Yes)
				== DialogResult.Yes;
		}

		public void Show(string text, string caption, MessageBoxType type)
		{
			MessageBox.ShowMessageBox(text, caption,
					MessageBoxButtons.Ok,
					MessageBoxType.Information,
					MessageBoxDefaultButton.Ok);
		}
	}
}
