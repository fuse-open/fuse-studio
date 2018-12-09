using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Outracks.Diagnostics;
using Outracks.IO;

namespace Outracks.AndroidManager
{
	class JavaInstaller
	{
		readonly Uri _downloadUrlMac = new Uri("http://download.oracle.com/otn-pub/java/jdk/8u191-b12/2787e4a523244c269598db4e85c51e0c/jdk-8u191-macosx-x64.dmg");
		readonly Uri _downloadUrlWin32 = new Uri("http://download.oracle.com/otn-pub/java/jdk/8u191-b12/2787e4a523244c269598db4e85c51e0c/jdk-8u191-windows-i586.exe");
		readonly Uri _downloadUrlWin64 = new Uri("http://download.oracle.com/otn-pub/java/jdk/8u191-b12/2787e4a523244c269598db4e85c51e0c/jdk-8u191-windows-x64.exe");
		readonly IFileSystem _fs;
		readonly string _jdk_name = "JDK 8 Update 191";
		readonly AbsoluteDirectoryPath _pathToJdkMac = AbsoluteDirectoryPath.Parse("/Library/Java/JavaVirtualMachines/jdk1.8.0_191.jdk/Contents/Home/bin");

		public JavaInstaller(IFileSystem fs)
		{
			_fs = fs;
		}

		public void Install(CancellationToken ct, IDialog dialog, IProgress<InstallerEvent> installerProgress, InstallOptions opts)
		{
			if (opts.Noninteractive == false)
			{
				var result = dialog.ShowLicenseDialog(LicenseText);
				if (result.No)
				{
					throw new InstallerError(new Exception("Aborting because license was not accepted."));
				}
			}

			var installFunc = Platform.OperatingSystem == OS.Mac
				? (Action<CancellationToken, IProgress<InstallerEvent>>)InstallMacOS
				: (Action<CancellationToken, IProgress<InstallerEvent>>)InstallWin;

			installFunc(ct, installerProgress);
		}

		void InstallMacOS(CancellationToken ct, IProgress<InstallerEvent> progress)
		{
			var tempFilePath = DirectoryPath.GetTempPath() / new FileName("jdk" + ".dmg");

			progress.DoInstallerStep("Starting download of Java Development Kit",
				() => DownloadHelper.DownloadFileWithProgress(_downloadUrlMac, tempFilePath, ct, progress, "oraclelicense=accept-securebackup-cookie"),
				e => _fs.Delete(tempFilePath));

			Process.Start("open", tempFilePath.NativePath);

			progress.Report(new InstallerMessage("Please start the JDK installer by double clicking the icon inside the '" + _jdk_name + "' window."));
			progress.Report(new InstallerMessage("Waiting for the JDK installer to finish..."));
			while (!_fs.Exists(_pathToJdkMac))
			{
				Thread.Sleep(50);
			}
		}

		void InstallWin(CancellationToken ct, IProgress<InstallerEvent> installerProgress)
		{
			var tempFilePath = DirectoryPath.GetTempPath() / new FileName("java_8" + ".exe");

			try
			{
				DownloadStep(tempFilePath, installerProgress, ct);
				ct.ThrowIfCancellationRequested();
				StartInstallerStep(tempFilePath, ct, installerProgress);
				UpdateEnvironmentVariables(installerProgress);
				installerProgress.Report(new InstallerStep("Java Development Kit was successfully installed"));
			}
			catch (Win32Exception e)
			{
				if (e.NativeErrorCode == 193)
				{
					_fs.Delete(tempFilePath);
					throw new InstallerError("Couldn't start Java installer. May be a corrupt download, please try to download again.");
				}

				throw new InstallerError(e);
			}
		}

		static void UpdateEnvironmentVariables(IProgress<InstallerEvent> progress)
		{
			try
			{
				foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User))
					Environment.SetEnvironmentVariable(variable.Key.ToString(), variable.Value.ToString());
			}
			catch (Exception e)
			{
				progress.Report(new InstallerMessage("WARNING: Failed to update environment variables: " + e.Message));
			}			
		}

		void DownloadStep(AbsoluteFilePath tempFilePath, IProgress<InstallerEvent> installerProgress, CancellationToken ct)
		{
			if (!_fs.Exists(tempFilePath))
			{
				installerProgress.DoInstallerStep("Starting download of Java Development Kit",
					() => DownloadHelper.DownloadFileWithProgress(IntPtr.Size == 4 ? _downloadUrlWin32 : _downloadUrlWin64, tempFilePath, ct, installerProgress, "oraclelicense=accept-securebackup-cookie"),
					e => _fs.Delete(tempFilePath));
			}
		}

		static void StartInstallerStep(AbsoluteFilePath tempFilePath, CancellationToken ct, IProgress<InstallerEvent> installerProgress)
		{
			installerProgress.Report(new InstallerStep("Installing Java Development Kit and Java Runtime Environment"));

			// TODO: Find a way to specify both install path for jre and jdk.
			//"string.Format("/s /INSTALLDIRPUBJRE=\"{0}\"", (_installPath / new DirectoryName("jdk")).NativePath));
			var processStartInfo = new ProcessStartInfo()
			{
				FileName = tempFilePath.NativePath,
				Arguments = "/s",
			};

			var exitCode = ProcessHelper.StartProcess(
				processStartInfo,
				ct,
				process => { });

			if (exitCode != 0)
			{
				throw new InstallerError("Failed to install Java JDK, please try to do it manually from http://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html.");
			}
		}

		string LicenseText
		{
			get
			{
				return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Outracks.AndroidManager.java_license.txt"))
					.ReadToEnd();
			}
		}
	}
}