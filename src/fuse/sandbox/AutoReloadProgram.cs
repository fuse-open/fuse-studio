using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using System.Threading;
using Outracks.Diagnostics;
using Outracks.Extensions;
using Outracks.Fuse.Theming.Themes;
using Outracks.Fusion;
using Outracks.Fusion.AutoReload;
using Outracks.IO;

namespace Outracks.Fuse
{
	static class AutoReloadProgram
	{
		[STAThread]
		public static void Main(string []argv)
		{
			Thread.CurrentThread.SetInvariantCulture();

			Application.Initialize(argv);

			var log = new Subject<string>();
			var shell = new Shell();
			var writer = new ObservableStreamWriter();
			writer.Chunks.Subscribe(log);
			Console.SetOut(writer);

			var autoReloadFiles = new List<AbsoluteFilePath>();

			foreach (var arg in argv)
			{
				var path = (AbsoluteFilePath)shell.ResolveAbsolutePath(arg);
				if (path.Name.Extension == ".cs")
				{
					autoReloadFiles.Add(AbsoluteFilePath.Parse(arg));
				}
				else
				{
					throw new Exception("Invalid argument, expected filepaths which ends with '.cs'.");
				}
			}

			UserSettings.Settings = PersistentSettings.Load(
				usersettingsfile: AbsoluteDirectoryPath.Parse(Environment.CurrentDirectory) / new FileName("autoreload-config.json"));

			if (autoReloadFiles.Count == 0)
			{
				if (Platform.IsMac)
					autoReloadFiles.Add(AbsoluteFilePath.Parse(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../../AutoReloadContent.cs"));
				else
					autoReloadFiles.Add(AbsoluteFilePath.Parse("../../AutoReloadContent.cs"));
			}

			var createDebugWindow = new Action(() =>
				Application.Desktop.CreateSingletonWindow(
					Observable.Return(true),
					dialog =>
						new Window()
						{
							Title = Observable.Return("Debug"),
							Size = Property.Create<Optional<Size<Points>>>(new Size<Points>(600, 500)).ToOptional(),
							Content = Debugging.CreateDebugControl(),
							Background = Theme.Background,
							Border = Stroke.Create(1, Color.White),
							Foreground = Color.White,
						}
			));

			Application.Desktop.CreateSingletonWindow(
				isVisible: Observable.Return(true),
				window: window =>
				{
					return new Window
					{
						Title = Observable.Return("Fuse - Autoreload"),
						Menu = Menu.Item("Debug", createDebugWindow) +
							Menu.Submenu("Themes",
								Menu.Option(
									value: Themes.OriginalLight,
									name: "Light theme",
									property: Theme.CurrentTheme) +
								Menu.Option(
									value: Themes.OriginalDark,
									name: "Dark theme",
									property: Theme.CurrentTheme)),
						Content = Layout.Dock()
							.Bottom(
								LogView.Create(log.Select(s => s), Observable.Return(Color.Black))
									.WithHeight(200)
									.WithBackground(Color.White))
							.Fill(AutoreloadControl(shell, autoReloadFiles, log.ToProgress()).Switch()),
						Background = Theme.Background,
						Border = Stroke.Create(1, Color.White),
						Foreground = Color.White,
					};
				});

			Application.Run();
		}

		class ObservableStreamWriter : TextWriter
		{
			readonly Subject<string> _chunks = new Subject<string>();

			public override Encoding Encoding
			{
				get { return Encoding.UTF8; }
			}

			public IObservable<string> Chunks
			{
				get { return _chunks; }
			}

			public override void Write(char value)
			{
				_chunks.OnNext(value.ToString());
			}
		}

		static IObservable<IControl> AutoreloadControl(IFileSystem shell, IList<AbsoluteFilePath> filesToReload, IProgress<string> log)
		{
			var currentAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			var fuseDesignerPath = Path.Combine(currentAssemblyPath, "fuse-studio.exe");

			var referencedAssemblies = new List<string>()
			{
				"System.dll",
				"System.Runtime.dll",
				"System.Core.dll",
				"System.Xml.dll",
				"System.Xml.Linq.dll",
				Path.Combine(currentAssemblyPath, "Outracks.Fusion.dll"),
				Path.Combine(currentAssemblyPath, "Outracks.dll"),
				Path.Combine(currentAssemblyPath, "Outracks.Math.dll"),
				Path.Combine(currentAssemblyPath, "System.Reactive.dll"),
				Path.Combine(currentAssemblyPath, "System.Collections.Immutable.dll"),
				Path.Combine(currentAssemblyPath, "Outracks.Fuse.dll"),
				Path.Combine(currentAssemblyPath, "Outracks.Simulator.dll"),
				fuseDesignerPath
			};

			return filesToReload.Select(shell.Watch)
				.CombineLatest()
				.Select(x => Unit.Default)
				.Throttle(TimeSpan.FromMilliseconds(500))
				.StartWith(Unit.Default)
				.Select(_ => ControlFactory.Compile(filesToReload, log, referencedAssemblies))
				.NotNone()
				.Select(asm => ControlFactory.CreateControl(asm, "Create", new object[0], log))
				.NotNone();
		}
	}
}
