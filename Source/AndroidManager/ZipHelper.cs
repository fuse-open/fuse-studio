using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Outracks.IO;

namespace Outracks.AndroidManager
{
	public enum DownloadResultStatus
	{
		Failed,
		Success
	}

	public static class ZipHelper
	{
		public static DownloadResultStatus DownloadZip(
			CancellationToken ct, 
			Uri downloadUrl, 
			AbsoluteDirectoryPath extractPath, 
			IProgress<InstallerEvent> progress,
			IFileSystem fs)
		{
			TryDeleteFolder(extractPath, fs, progress);
			fs.Create(extractPath);

			fs.CreateEmpty(extractPath / new FileName(".progress"));
			var downloadRes = Download(ct, downloadUrl, extractPath, progress);
			fs.Delete(extractPath / new FileName(".progress"));

			return downloadRes;
		}

		static DownloadResultStatus Download(CancellationToken ct, Uri downloadUrl, AbsoluteDirectoryPath extractPath, IProgress<InstallerEvent> progress)
		{			
			ct.ThrowIfCancellationRequested();			

			var client = new WebClient();
			using (var readStream = client.OpenRead(downloadUrl))
				ExtractZipTo(ct, readStream, extractPath, progress);

			return DownloadResultStatus.Success;
		}

		static void TryDeleteFolder(AbsoluteDirectoryPath folderToDelete, IFileSystem fs, IProgress<InstallerEvent> progress)
		{
			try
			{
				if (fs.Exists(folderToDelete))
				{
					progress.Report(new InstallerMessage(folderToDelete.NativePath + " already exists and will be overwritten."));
					fs.Delete(folderToDelete);
				}
			}
			catch (Exception)
			{
				// We ignore this, so use this function with care.			
			}			
		}

		static void ExtractZipTo(CancellationToken ct, Stream stream, AbsoluteDirectoryPath outFolder, IProgress<InstallerEvent> progress)
		{
			progress.Report(new InstallerStep("Extracting"));

			var zipIn = new ZipInputStream(stream);
			var zipEntry = zipIn.GetNextEntry();
			if (zipEntry.IsDirectory)
				zipEntry = zipIn.GetNextEntry();

			while (zipEntry != null)
			{
				ct.ThrowIfCancellationRequested();
				var entryFileName = zipEntry.Name;
				var parts = zipEntry.IsDirectory ? RelativeDirectoryPath.Parse(entryFileName.Substring(0, entryFileName.Length - 1)).Parts :
					RelativeFilePath.Parse(entryFileName).Parts;

				var newPathStr = "";
				var partsArray = parts.ToArray();
				for (var i = 1; i < partsArray.Length; ++i)
					newPathStr += partsArray[i] + (i + 1 == partsArray.Length ? "" : Path.DirectorySeparatorChar.ToString());

				var buffer = new byte[4096];
				var fullZipToPath = Path.Combine(outFolder.NativePath, newPathStr);
				var directoryName = zipEntry.IsFile ? Path.GetDirectoryName(fullZipToPath) : fullZipToPath;
				if (directoryName.Length > 0)
					Directory.CreateDirectory(directoryName);

				if (zipEntry.IsFile)
				{
					progress.Report(new InstallerMessage("Extracting: " + newPathStr));
					using (var streamWriter = File.Create(fullZipToPath))
					{
						StreamUtils.Copy(zipIn, streamWriter, buffer);
					}
				}

				zipEntry = zipIn.GetNextEntry();
			}
		}
	}
}