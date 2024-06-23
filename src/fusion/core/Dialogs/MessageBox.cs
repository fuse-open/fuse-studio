using System;

namespace Outracks.Fusion.Dialogs
{
	public static class MessageBox
	{
		public static IMessageBox Implementation { get; set; }

		public static void BringToFront()
		{
			if (Implementation == null)
				throw new NotImplementedException();

			Implementation.BringToFront();
		}

		public static bool ShowConfirm(string text, string caption, MessageBoxType type)
		{
			if (Implementation == null)
				throw new NotImplementedException();

			return Implementation.ShowConfirm(text, caption, type);
		}

		public static void Show(string text, string caption, MessageBoxType type)
		{
			if (Implementation == null)
				throw new NotImplementedException();

			Implementation.Show(text, caption, type);
		}
	}
}
