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

namespace Outracks.Fuse.Tray
{
	public static class Program
	{
		static Program()
		{
			Thread.CurrentThread.SetInvariantCulture();
		}

		[STAThread]
		static void Main()
		{
			Start(Icon.FromResource("Outracks.Fuse.Tray.Fuse.ico"));
		}

		public static void Start(Icon icon)
		{
			var args = Environment.GetCommandLineArgs().ToList();

			var fuse = FuseApi.Initialize("fuse-tray", args);

			var log = fuse.Report;

			var native = Application.Initialize(args);

			var client = fuse.ConnectAsync("fuse-tray");

			var app = native.CreateTrayApplication(
				log,
				title: Observable.Return(""),
				icon: Observable.Return(icon),
				menu:
					Menu.Item(
						"fuse X v" + fuse.Version,
						command: Command.Disabled)
					+ Menu.Separator
					+ Menu.Item(
						Platform.IsMac ? "Quit" : "Exit",
						async () => await Exit(fuse, client)));

			// Exit on connection lost
			client.ToObservable().SelectMany(c => c.ConnectionLost).Subscribe(d => Application.Exit(0));

			// Start dashboard on click
			app.UserClicked.Subscribe(clicks => fuse.Studio.Start());

			Application.Run();
		}

		static async Task Exit(IFuse fuse, Task<IMessagingService> clientTask)
		{
			if (Platform.IsMac)
			{
				// HACK: Kill all processes immediately.
				new FuseKiller(fuse.Report, fuse.FuseRoot).Execute(ColoredTextWriter.Out);
			}

			try
			{
				var client = await clientTask;
				await client
					.Request(new KillRequest { Reason = "User quit from tray menu" })
					.TimeoutAfter(TimeSpan.FromSeconds(1));
			}
			catch (TimeoutException)
			{
				var processes = Process.GetProcessesByName("fuse")
						.Concat(Process.GetProcessesByName("fuse X"));

				foreach (var process in processes)
					process.Kill();
			}
			finally
			{
				Application.Exit(0);
			}
		}
	}
}
