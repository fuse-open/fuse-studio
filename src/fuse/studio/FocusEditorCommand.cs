using System;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Outracks.Fuse.Protocol;
using Outracks.Fusion;

namespace Outracks.Fuse.Studio
{
	public static class FocusEditorCommand
	{
		[DllImport("user32.dll")]
		public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

		public static Command Create(IContext context, IProject project, IMessagingService daemon)
		{
			return context.CurrentSelection
				.SourceReference
				.CombineLatest(project.FilePath, (maybeSrc, projectFile) =>
					maybeSrc.SelectMany(src =>
						src.Location.Select(location =>
							new FocusEditorRequest
							{
								File = src.File,
								Column = location.Character,
								Line = location.Line,
								Project = projectFile.NativePath
							})))
				.Switch(request =>
					Command.Create(
						isEnabled: request.HasValue,
						action: () => SendRequest(daemon, request.Value)));
		}

		public static void SendRequest(IMessagingService daemon, FocusEditorRequest request)
		{
			Task.Run(
				async () =>
				{
					try
					{
						var response = await daemon.Request(request);
						if (response.FocusHwnd.HasValue && Environment.OSVersion.Platform == PlatformID.Win32NT)
							SwitchToThisWindow(new IntPtr(response.FocusHwnd.Value), true);
					}
					catch (RequestFailed)
					{
						// Just ignore for now..
					}
				});
		}
	}
}