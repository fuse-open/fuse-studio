using Outracks.Fusion;

namespace Outracks.Fuse
{
    public static class ConfirmCancelControl
    {
        
        public static IControl Create(
            Command close,
            Command confirm,
            IControl fill,
            Command? cancel,
            Text? confirmText,
            Text? cancelText,
            Text? confirmTooltip)
        {
            var confirmButton = ThemedButton.Create(
                command: confirm.Then(close),
                label: confirmText ?? "OK",
                icon: Icons.Confirm(Theme.Active),
                tooltip: confirmTooltip ?? default(Text),
                hoverColor: Theme.Active);
			
            var cancelButton = ThemedButton.Create(
                command: (cancel ?? Command.Enabled(() => { }).Then(close)),
                label: cancelText ?? "Cancel",
                icon: Icons.Cancel(Theme.Cancel),
                tooltip: null,
                hoverColor: Theme.Cancel);
			
            var buttons = Layout.Dock()
                .Bottom(
                    Layout.Dock()
                        .Top(Separator.Medium)
                        .Fill(Layout.SubdivideHorizontallyWithSeparator(Separator.Medium, cancelButton, confirmButton)))
                .Fill(fill);
            return buttons;
        }
    }
}