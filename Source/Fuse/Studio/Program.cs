using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using Fuse.Preview;
using Outracks.Diagnostics;
using Outracks.Fuse.Testing;

namespace Outracks.Fuse.Designer
{
	using Extensions;
	using Fusion;
	using IO;
	using Protocol;
	using Dashboard;

	public static class Program
	{
		[STAThread]
		public static void Main(string[] argsArray)
		{
			Thread.CurrentThread.SetInvariantCulture();

			// This is required to get all required build tools in path
			// See https://github.com/fusetools/Fuse/issues/4245 for details
			if (Platform.OperatingSystem == OS.Mac)
			{
				Environment.SetEnvironmentVariable("PATH", "/usr/local/bin:" + Environment.GetEnvironmentVariable("PATH"));
			}

			var argumentList = argsArray.ToList();
			var fuse = FuseApi.Initialize("Designer", argumentList);
			var shell = new Shell();
			var errors = new ReplaySubject<Exception>(1);

			if (!Application.InitializeAsDocumentApp(argumentList, "Fusetools.Fuse"))
				return;

			// Initialize console redirection early to show output during startup in debug window
			ConsoleOutputWindow.InitializeConsoleRedirection();

			UserSettings.Settings = PersistentSettings.Load(
				usersettingsfile: fuse.UserDataDir / new FileName("designer-config.json"), 
				onError: errors.OnNext);

			var daemon = fuse.ConnectOrSpawnAsync("Designer").ToObservable().Switch();

			//var previewPlatform = Platform.OperatingSystem == OS.Mac
			//	? (IPlatform)new MacPlatform(fuse.MonoExe.Value.NativePath)
			//	: (IPlatform)new WindowsPlatform();

			var previewService = new PreviewService();
			Application.Terminating += previewService.Dispose;

			var openProject = new OpenProject(
				previewService, 
				shell, fuse, daemon, 
				errors);

			var createProject = new CreateProject(fuse);			
			var splashWindow = new Dashboard(createProject, fuse);

			
			Application.CreateDocumentWindow = document => openProject.Execute(document, argumentList.ToArray());

			Application.LaunchedWithoutDocuments = splashWindow.Show;

			Application.Run();
		}
	}

	static class DocumentTypes
	{
		public static readonly FileFilter Project = new FileFilter("Fuse Project", "unoproj");
	}
}
