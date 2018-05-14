using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Outracks.Diagnostics;

namespace Outracks {
	class LogFactory
	{

		static bool _isSetup;
		static bool _isFailed;
		internal static ILog GetLogger(string name)
		{
			Setup();
			return _isFailed ? new NullLogger() : LogManager.GetLogger(name);
		}

		static void Setup()
		{
			if (_isSetup)
			{
				return;
			}
			var random = new Random(Process.GetCurrentProcess().Id);
			var stopwatch = Stopwatch.StartNew();
			while (stopwatch.ElapsedMilliseconds < 1000)
			{
				try
				{
					TrySetup();
					return;
				}
				catch (Exception)
				{
					Thread.Sleep(TimeSpan.FromMilliseconds(100 + 100*random.NextDouble()));
				}
			}
			_isSetup = true;
			_isFailed = true;
		}

		static void TrySetup()
		{
			Directory.CreateDirectory(LogDirectory);
			GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id;
			var hierarchy = (Hierarchy)LogManager.GetRepository();

			var patternLayout = new PatternLayout
			{
				ConversionPattern = "%date{ISO8601} [%property{pid}:%thread] %-5level %logger - %message%newline"
			};
			patternLayout.ActivateOptions();

			var roller = new RollingFileAppender
			{
				AppendToFile = true,
				File = Path.Combine(LogDirectory, "fuse.log"),
				Layout = patternLayout,
				MaxSizeRollBackups = 5,
				MaximumFileSize = "256KB",
				RollingStyle = RollingFileAppender.RollingMode.Size,
				StaticLogFileName = true
			};
			roller.ActivateOptions();
			hierarchy.Root.AddAppender(roller);

			hierarchy.Root.Level = Level.Info;
			hierarchy.Configured = true;
			_isSetup = true;
		}

		public static void SetLogLevel(Level level)
		{
			((Hierarchy)LogManager.GetRepository()).Root.Level = level;
			((Hierarchy)LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);
		}

		private static string LogDirectory
		{
			get
			{
				switch (Platform.OperatingSystem)
				{
					case OS.Windows: return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Fusetools\Fuse\logs";
					case OS.Mac: return Environment.GetEnvironmentVariable("HOME") + "/.fuse/logs";
				}
				throw new Exception("Unsupported platform");
			}
		}

	}
}