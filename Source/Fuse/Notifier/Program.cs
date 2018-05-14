using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Outracks.Diagnostics;
using Outracks.Extensions;
using Outracks.Fuse.Daemon;
using Outracks.Fuse.Protocol;
using Outracks.Fusion;

namespace Outracks.Fuse.Notifier
{
	static class Program 
	{
		static Program()
		{
			Thread.CurrentThread.SetInvariantCulture();
		}

		[STAThread]
		static void Main()
		{
			var args = Environment.GetCommandLineArgs().ToList();

			var fuse = FuseApi.Initialize("Tray", args);

			var log = fuse.Report;

			var native = Application.Initialize(args);

			var client = fuse.ConnectAsync("Fuse Tray");

			var app = native.CreateTrayApplication(
				log,
				title: Observable.Return(""),
				icon: Observable.Return(
					Icon.FromResource(
						Platform.OperatingSystem == OS.Mac
							? "Outracks.Fuse.Notifier.Resources.Fuse.icns"
							: "Outracks.Fuse.Notifier.Resources.Fuse.ico")),
				menu:
					Menu.Item(
						"Fuse v. " + fuse.Version,
						command: Command.Disabled)
					+ Menu.Separator
					+ Menu.Item(
						Platform.OperatingSystem == OS.Mac ? "Quit" : "Exit",
						async () => await Exit(client)));

			// Exit on connection lost
			client.ToObservable().SelectMany(c => c.ConnectionLost).Subscribe(d => Application.Exit(0));

			// Start dashboard on click
			app.UserClicked.Subscribe(clicks => fuse.Designer.Start());

			Application.Run();
		}
		
		static async Task Exit(Task<IMessagingService> clientTask)
		{
			try
			{
				var client = await clientTask;
				await client
					.Request(new KillRequest { Reason = "User quit from tray menu" })
					.TimeoutAfter(TimeSpan.FromSeconds(1));
			}
			catch (TimeoutException)
			{
				var processes = Process.GetProcessesByName("Fuse");
				foreach (var process in processes)
				{
					process.Kill();
				}
			}
			finally
			{
				Application.Exit(0);
			}
		}
	}
}
