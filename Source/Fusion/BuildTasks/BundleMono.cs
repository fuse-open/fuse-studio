using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Outracks.IO;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace Outracks.Fuse.BuildTasks
{
	public class BundleApp : Task
	{
		public override bool Execute ()
		{
			Log.LogMessage ("Starting bundling");

			var appPath = AbsoluteDirectoryPath.Parse (FinalDir) / new DirectoryName(AppName + ".app");
			var contents = appPath / new DirectoryName ("Contents");
			var nativeExePath = contents / new DirectoryName ("MacOS");
			Create (nativeExePath);

			Log.LogMessage ("Adding monostub");
			using(var stub = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("Outracks.Fuse.BuildTasks.monostub"))
			using(var xamStub = Create(
				nativeExePath
				/ new FileName(AppName)))
			{
				stub.CopyTo (xamStub);
			}

			Log.LogMessage ("Make monostub executable");
			var p = Process.Start ("chmod", "+x " + "\"" + (nativeExePath / new FileName(AppName)).NativePath + "\"");
			p.WaitForExit ();

			Log.LogMessage ("Copy Info.plist into app.");
			Copy (AbsoluteFilePath.Parse (InfoFile), contents / new FileName ("Info.plist"));

			if(string.IsNullOrWhiteSpace(MonoPath))
				MonoPath = "/Library/Frameworks/Mono.framework/Versions/Current";

			var relativeMonoPath = AbsoluteDirectoryPath.Parse(MonoPath).RelativeTo(contents);
			Log.LogMessage ("Adding .mono_root and set it to be: " + relativeMonoPath.NativeRelativePath);
			using (var file = Create (
				appPath
				/ new DirectoryName ("Contents")
				/ new FileName (".mono_root")))
			using (var streamWriter = new StreamWriter(file))
			{
				streamWriter.Write (relativeMonoPath.NativeRelativePath);
			}

			var rPath =  relativeMonoPath.BasePath != null
				?  "@executable_path/../" + relativeMonoPath.BasePath.NativeRelativePath
				: "@executable_path/../";
			p = Process.Start("install_name_tool", "-add_rpath " + rPath + " \"" + (nativeExePath / new FileName(AppName)).NativePath + "\"");
			p.WaitForExit();

			Log.LogMessage ("Copy mono bundle files");
			var monoBundle = contents / new DirectoryName ("MonoBundle");
			Create (monoBundle);
			CopyAllFilesRecursive (
				AbsoluteDirectoryPath.Parse (OutputDir),
				monoBundle);

			var resources = contents / new DirectoryName (ResourcePrefix);
			var projectDir = AbsoluteDirectoryPath.Parse (ProjectDir);
			Log.LogMessage ("Create and move resources to resources prefix");
			CreateOrClean (resources);
			Log.LogMessage ("Compile all xib files to nib");
			CompileAllXibFilesIn (projectDir, resources);
			CopyAllFilesRecursive (projectDir / new DirectoryName (ResourcePrefix), resources);

			return true;
		}

		static void CopyAllFilesRecursive(AbsoluteDirectoryPath dir, AbsoluteDirectoryPath outDir)
		{
			var files = Directory.GetFiles (dir.NativePath);
			foreach (var file in files)
			{
				File.Copy (file, (outDir / Path.GetFileName (file)).NativePath, true);
			}

			var childrenDirs = Directory.GetDirectories(dir.NativePath);
			foreach(var childrenDir in childrenDirs)
			{
				var childrenOutDir = outDir / new DirectoryName (Path.GetFileName (childrenDir));
				Create (childrenOutDir);
				CopyAllFilesRecursive (AbsoluteDirectoryPath.Parse(childrenDir), childrenOutDir);
			}
		}

		static Stream Create(AbsoluteFilePath file)
		{
			return File.Open(file.NativePath, FileMode.Create);
		}

		static void Create(AbsoluteDirectoryPath dir)
		{
			Directory.CreateDirectory (dir.NativePath);
		}

		static void CreateOrClean(AbsoluteDirectoryPath dir)
		{
			if(Directory.Exists(dir.NativePath))
				Directory.Delete(dir.NativePath, true);

			Directory.CreateDirectory (dir.NativePath);
		}

		void CompileAllXibFilesIn(AbsoluteDirectoryPath inDir, AbsoluteDirectoryPath toDir)
		{
			var xibFiles = Directory.GetFiles (inDir.NativePath, "*.xib");
			foreach(var xibFile in xibFiles)
			{
				var nibFilePath = toDir / new FileName (Path.GetFileNameWithoutExtension (xibFile) + ".nib");
				Log.LogMessage ("Compiling " + xibFile);
				var ibToolStartInfo = new ProcessStartInfo () {
					FileName = "ibtool",
					Arguments = "--compile " + "\"" + nibFilePath.NativePath + "\"" + " " + "\"" + xibFile + "\"",
				};
				var p = Process.Start (ibToolStartInfo);
				p.WaitForExit ();
				if (p.ExitCode != 0)
					Log.LogError ("Failed to compile xib: " + xibFile);
			}
		}

		static void Copy(AbsoluteFilePath fromFile, AbsoluteFilePath toFile)
		{
			File.Copy (fromFile.NativePath, toFile.NativePath, true);
		}

		[Required]
		public string AppName
		{
			get;
			set;
		}

		[Required]
		public string ResourcePrefix
		{
			get;
			set;
		}

		[Required]
		public string ProjectDir
		{
			get;
			set;
		}

		[Required]
		public string FinalDir
		{
			get;
			set;
		}

		[Required]
		public string OutputDir
		{
			get;
			set;
		}

		[Required]
		public string InfoFile
		{
			get;
			set;
		}

		public string MonoPath
		{
			get;
			set;
		}
	}
}
