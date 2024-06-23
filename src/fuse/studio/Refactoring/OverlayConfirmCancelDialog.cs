using Outracks.Fusion;

namespace Outracks.Fuse.Refactoring
{
	public static class OverlayConfirmCancelDialog
	{
		public static void Open(
			this IModalHost modalHost,
			Command confirm,
			IControl fill,
			Command? cancel = null,
			Text? confirmText = null,
			Text? cancelText = null,
			Text? confirmTooltip = null)
		{
			modalHost.OpenModal(
				close => Layout.Dock()
					.Top(
						ConfirmCancelControl.Create(close, confirm, fill, cancel, confirmText, cancelText, confirmTooltip)
							.WithBackground(Theme.WorkspaceBackground)
							.MakeHittable()
							.Control)
					.Fill(Control.Empty.WithBackground(Color.FromRgba(0x00000066))));
		}

	}
}