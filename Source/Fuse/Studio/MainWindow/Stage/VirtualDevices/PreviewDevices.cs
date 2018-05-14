using System;
using System.Collections.Immutable;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security;

namespace Outracks.Fuse.Stage
{
	using Fusion;
	using IO;

	public class PreviewDevices
	{
		public IObservable<string> LogMessages { get { return _logMessages; }}
		readonly ISubject<string> _logMessages = new Subject<string>();
		readonly IProject _project;
		readonly IShell _fileSystem;

		public PreviewDevices(
			IProject project,
			IShell fileSystem)
		{
			_project = project;
			_fileSystem = fileSystem;
			Devicess = project.RootDirectory.Switch(path => WatchDevicesList().StartWith(LoadDevicesForProject(path)));
			DefaultDevice = Devicess.Select(devices => devices.FirstOrNone(info => info.IsDefault).Or(Stage.Devices.Default));
		}

		public IObservable<DeviceScreen> DefaultDevice { get; private set; }

		public IObservable<IImmutableList<DeviceScreen>> Devicess { get; private set; }

		public IObservable<IImmutableList<DeviceScreen>> WatchDevicesList()
		{
			return _project.RootDirectory.Switch(projDir => 
				_fileSystem.Watch(CustomDevicesFile(projDir))
					.StartWith(Unit.Default)
					.CatchAndRetry(delay: TimeSpan.FromSeconds(1))
					.Throttle(TimeSpan.FromSeconds(1.0 / 30.0))
					.Select(_ => LoadDevicesForProject(projDir)));
		}

		public IImmutableList<DeviceScreen> LoadDevicesForProject(AbsoluteDirectoryPath projDir)
		{
			return TryLoadCustomDevices(projDir).Or(() => Stage.Devices.LoadDefaultDevices());
		}

		public Command CustomizeDevices()
		{
			return _project.RootDirectory.Switch(projDir =>
				Command.Enabled(() =>
				{
					if (!HasCustomDevicesFile(projDir))
						CreateCustomDevices(projDir);

					var devicesFile = CustomDevicesFile(projDir);
					try
					{
						_fileSystem.OpenWithDefaultApplication(devicesFile);
					}
					catch (Exception e)
					{
						_logMessages.OnNext("Failed to open " + devicesFile + ": " + e.Message + "\n");
					}
				}));
		}

		Optional<ImmutableList<DeviceScreen>> TryLoadCustomDevices(AbsoluteDirectoryPath projDir)
		{
			if (!HasCustomDevicesFile(projDir))
				return Optional.None();

			var devicesFile = CustomDevicesFile(projDir);
			try
			{
				return LoadCustomDevices(devicesFile);
			}
			catch (MalformedDeviceInfo)
			{
				_logMessages.OnNext("Malformed " + devicesFile + "\n");
			}
			catch (FileNotFoundException)
			{
				_logMessages.OnNext("Could not find " + devicesFile + "\n");
			}
			catch (Exception e)
			{
				_logMessages.OnNext("Failed to load " + devicesFile + " : " + e.Message + "\n");
			}

			return Optional.None();
		}

		/// <param name="devicesFile"></param>
		/// <exception cref="MalformedDeviceInfo" />
		/// <exception cref="IOException" />
		/// <exception cref="UnauthorizedAccessException" />
		/// <exception cref="SecurityException" />
		ImmutableList<DeviceScreen> LoadCustomDevices(AbsoluteFilePath devicesFile)
		{
			using (var stream = _fileSystem.OpenRead(devicesFile))
			{
				return Stage.Devices.LoadDevicesFrom(stream);
			}
		}

		/// <exception cref="IOException" />
		/// <exception cref="UnauthorizedAccessException" />
		void CreateCustomDevices(AbsoluteDirectoryPath projDir)
		{
			using (var stream = _fileSystem.Create(CustomDevicesFile(projDir)))
			{
				Stage.Devices.SaveDefaultDevices(stream);
			}
		}

		bool HasCustomDevicesFile(AbsoluteDirectoryPath projDir)
		{
			return _fileSystem.Exists(CustomDevicesFile(projDir));
		}

		AbsoluteFilePath CustomDevicesFile(AbsoluteDirectoryPath projDir)
		{
			return projDir / new FileName("devices.json");
		}
	}
}