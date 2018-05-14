using System;
using System.IO;
using Outracks.Diagnostics;
using Outracks.IO;

namespace Outracks.Fuse.Analytics
{
	public class SystemGuidLoader
	{
		public static Guid LoadOrCreate()
		{
			return LoadOrCreate(GetSystemGuidPath());
		}


		public static Guid LoadOrCreate(AbsoluteFilePath guidPath)
		{
			return TryLoad(guidPath)
				.Or(() => Create(guidPath));
		}

		public static Guid LoadOrCreateOrEmpty()
		{
			try
			{
				return LoadOrCreate();
			}
			catch (Exception e)
			{
				ReportFactory.GetReporter(Guid.Empty, Guid.NewGuid(), "SystemGuidLoader").Exception("Failed to load system guid", e, ReportTo.Log | ReportTo.Headquarters);
				return Guid.Empty;
			}
		}

		static Optional<Guid> TryLoad(AbsoluteFilePath guidPath)
		{
			try
			{
				if (!File.Exists(guidPath.NativePath))
					return Optional.None();

				return Guid.Parse(ReadString(guidPath));
			}
			catch (Exception e)
			{
				Warn("Failed to load guid from disk", e);
				return Optional.None();
			}
		}

		static string ReadString(AbsoluteFilePath guidPath)
		{
			return RetryLoop.Try(5, () =>
			{
				using (var stream = File.OpenRead(guidPath.NativePath))
					return stream.ReadToEnd();
			});
		}

		static Guid Create(AbsoluteFilePath guidPath)
		{
			try
			{
				Directory.CreateDirectory(guidPath.ContainingDirectory.NativePath);
				using (var stream =  File.Open(guidPath.NativePath, FileMode.Create))
				using (var streamWriter = new StreamWriter(stream))
				{
					var guid = Guid.NewGuid();
					streamWriter.Write(guid);
					return guid;
				}
			}
			catch (Exception e)
			{
				// Unidentifiable systems are treated as Guid.Empty
				Warn("Failed to save system guid to disk, using Guid.Empty", e);
				return Guid.Empty;
			}
		}

		public static AbsoluteFilePath GetSystemGuidPath()
		{
			if (Platform.OperatingSystem == OS.Windows)
			{
				return AbsoluteDirectoryPath.Parse(
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) / "Fusetools" / "Fuse" /
					new FileName(".user");
			}
			else if (Platform.OperatingSystem == OS.Mac)
			{
				return AbsoluteDirectoryPath.Parse(
					Environment.GetFolderPath (Environment.SpecialFolder.Personal)) / ".fuse" /
					new FileName(".user");
			}

			throw new PlatformNotSupportedException("Not implemented on platform: " + Platform.OperatingSystem);
		}

		static void Warn(string description, Exception e)
		{
			// TODO 
		}
	}
}