using System;
using System.Globalization;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using Fuse.Preview;
using Outracks.Diagnostics;
using Outracks.Extensions;
using Outracks.Fuse.Dashboard;
using Outracks.Fuse.Protocol;
using Outracks.Fuse.Testing;
using Outracks.Fusion;
using Outracks.Fusion.Dialogs;
using Outracks.IO;

namespace Outracks.Fuse.Studio
{
	public static class Program
	{
		static Program()
		{
			Thread.CurrentThread.SetInvariantCulture();
#if DEBUG
			ConsoleExtensions.RedirectStreamsToLogFiles("fuse-studio");
#endif
		}

		[STAThread]
		public static void Main(string[] argsArray)
		{
			LocaleCulture.Initialize(CultureInfo.CurrentUICulture);
			Thread.CurrentThread.SetInvariantCulture();

			// This is required to get all required build tools in path
			if (Platform.IsMac)
				Environment.SetEnvironmentVariable("PATH", "/usr/local/bin:/opt/local/bin:/opt/homebrew/bin:" + Environment.GetEnvironmentVariable("PATH"));

			var argumentList = argsArray.ToList();
			var fuse = FuseApi.Initialize("fuse-studio", argumentList, remoteLicense: true);
			var shell = new Shell();
			var errors = new ReplaySubject<Exception>(1);
			NagScreen.Initialize(fuse);

			if (!Application.InitializeAsDocumentApp(argumentList, "Fuse.Studio"))
				return;

			// Initialize console redirection early to show output during startup in debug window
			ConsoleOutputWindow.InitializeConsoleRedirection();

			UserSettings.Settings = PersistentSettings.Load(
				usersettingsfile: fuse.UserDataDir / new FileName("studio-settings.json"),
				onError: errors.OnNext);

			var daemon = fuse.ConnectOrSpawnAsync("fuse-studio").ToObservable().Switch();
			fuse.License.Subscribe(fuse.FuseExe, daemon);

			var previewService = new PreviewService();
			Application.Terminating += previewService.Dispose;

			var activate = new Activate(fuse.License, daemon);

			var openProject = new OpenProject(
				previewService,
				shell, fuse, daemon,
				errors, activate);

			var createProject = new CreateProject(fuse);
			var splashWindow = new Dashboard.Dashboard(createProject, fuse, daemon, activate);

			Application.CreateDocumentWindow = document => openProject.Execute(document, argumentList.ToArray());

			var launched = false;
			Application.LaunchedWithoutDocuments = () => {
				if (!launched)
				{
					launched = true;

					// Bring to front (#124).
					Application.MainThread.InvokeAsync(() => {
						Console.WriteLine("Bringing to front");
						MessageBox.BringToFront();
						return true;
					});
#if DEBUG
					// Start with a project open when DEBUG.
					Application.OpenDocument(fuse.FuseRoot.ContainingDirectory.ContainingDirectory / "empty" / new FileName("app.unoproj"), true);
					ConsoleOutputWindow.Create();
					return;
#endif
				}

				// Start with dashboard.
				splashWindow.Show();
			};

			var errorDialogIsOpen = false;
			fuse.License.Error += (_, __) => {
				if (errorDialogIsOpen)
					return;

				errorDialogIsOpen = true;
				MessageBox.BringToFront();
				MessageBox.Show(Strings.License_Error_Text, "fuse X", MessageBoxType.Error);
				errorDialogIsOpen = false;
			};

			LanguageMenu.Initialize();
			Application.Run();
		}
	}

	static class DocumentTypes
	{
		public static readonly FileFilter Project = new FileFilter("fuse X Project", "unoproj");
	}
}
