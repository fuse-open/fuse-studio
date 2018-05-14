using System;
using System.Collections.Generic;
using System.IO;
using Outracks;
using Outracks.IO;
using Outracks.Simulator.Bytecode;
using Outracks.Simulator.Protocol;
using Uno;
using Uno.Build;
using Uno.Compiler.API.Backends;
using Uno.Compiler.Backends.CIL;
using Uno.Compiler.Backends.OpenGL;
using Uno.ProjectFormat;
using Outracks.Simulator;
using StringSplitting = Outracks.Simulator.StringSplitting;

namespace Fuse.Preview
{
	public class SimulatorBuilder
	{
		readonly IFileSystem _fileSystem;
		readonly CacheCleaner _cacheCleaner;
		readonly bool _isHost;
		readonly Action<LockFile> _registerLock;
		readonly OutputDirGenerator _outputDirGenerator;

		public SimulatorBuilder(
			IFileSystem fileSystem, 
			CacheCleaner cacheCleaner, 
			bool isHost, Action<LockFile> registerLock)
		{
			_fileSystem = fileSystem;
			_cacheCleaner = cacheCleaner;
			_isHost = isHost;
			_registerLock = registerLock;
			_outputDirGenerator = new OutputDirGenerator(_fileSystem);
		}

		/// <exception cref="Exception"></exception>
		/// <exception cref="FailedToCreateOutputDir"></exception>
		public SimulatorUnoProject CreateSimulatorProject(BuildProject args)
		{
			return CreateSimulatorProject(
				args, 
				PreviewTarget.Local, 
				false,
				false);
		}

		/// <exception cref="Exception"></exception>
		/// <exception cref="FailedToCreateOutputDir"></exception>
		public SimulatorUnoProject CreateSimulatorProject(BuildProject args, PreviewTarget target, bool directToDevice, bool quitAfterApkLaunch)
		{
			var project = AbsoluteFilePath.Parse(args.ProjectPath);
			var buildProject = Project.Load(project.NativePath);
			var projectDir = project.ContainingDirectory;
			
			var basePath = AbsoluteDirectoryPath.Parse(buildProject.BuildDirectory) / target.ToString();

			var outputDir = FindOutputDir(args, basePath);

			var preambleDir = outputDir / "preamble";
			var cacheDir = outputDir / "cache";

			_cacheCleaner.CleanIfNecessary(cacheDir);
			SetCacheDir(buildProject, projectDir, cacheDir);

			var applicationClassName = TypeName.Parse("Outracks.Simulator.GeneratedApplication");
			var applicationClass = ApplicationClassGenerator.CreateApplicationClass(args, buildProject.Name, applicationClassName);
			var dependencies = ApplicationClassGenerator.Dependencies;

			AddPreamble(buildProject, preambleDir, projectDir, applicationClass, dependencies);
			AddIcons(buildProject, preambleDir, projectDir);
			ChangePackageName(buildProject);
			ChangeTitle(buildProject, oldTitle => oldTitle + " (preview)");			

			var buildOptions = new BuildOptions
			{
				OutputDirectory = outputDir.NativePath,
				Configuration = BuildConfiguration.Preview,
				MainClass = applicationClassName.FullName,
				Strip = target != PreviewTarget.Local,
			};

			foreach (var define in args.Defines.UnionOne("Designer"))
				buildOptions.Defines.Add(define);

			if (target == PreviewTarget.iOS && !directToDevice)
			{
				// 17.12.15 - Prevent double building when exporting to iOS. (Uno bug)
				buildOptions.RunArguments = "debug";
				buildOptions.Native = false;
			}

			if (quitAfterApkLaunch)
			{
				buildOptions.RunArguments += " -L";
			}

			return new SimulatorUnoProject(buildProject, buildOptions, "", args.Verbose, args.BuildLibraries);
		}

		AbsoluteDirectoryPath FindOutputDir(BuildProject args, AbsoluteDirectoryPath basePath)
		{
			if (string.IsNullOrWhiteSpace(args.OutputDir))
			{
				var outputDirWithLock =
					_outputDirGenerator.CreateOrReuseOutputDir(_isHost ? basePath / "PreviewHost" : basePath / "Preview");
				_registerLock(outputDirWithLock.LockFile);
				return outputDirWithLock.OutputDir;
			}
			else
			{
				return AbsoluteDirectoryPath.Parse(args.OutputDir);
			}
		}

		public void ChangePackageName(Project buildProject)
		{
			var androidPreviewPackageName = buildProject.GetString("Android.PreviewPackage");
			if(!string.IsNullOrEmpty(androidPreviewPackageName))
				buildProject.MutableProperties["Android.Package"] = new SourceValue(buildProject.Source, androidPreviewPackageName);

			var iosPreviewBundleIdName = buildProject.GetString("iOS.PreviewBundleIdentifier");
			if (!string.IsNullOrEmpty(iosPreviewBundleIdName))
				buildProject.MutableProperties["iOS.BundleIdentifier"] = new SourceValue(buildProject.Source, iosPreviewBundleIdName);
		}

		void ChangeTitle(Project buildProject, Func<string,string> title)
		{
			buildProject.MutableProperties["Title"] = new SourceValue(buildProject.Source, title(buildProject.GetString("Title")));
		}

		void SetCacheDir(Project buildProject, AbsoluteDirectoryPath projectDir, AbsoluteDirectoryPath cacheDir)
		{
			buildProject.MutableProperties["CacheDirectory"] = new SourceValue(buildProject.Source, cacheDir.RelativeTo(projectDir).NativeRelativePath.ToUnixPath());
		}

		void AddPreamble(Project buildProject, AbsoluteDirectoryPath preambleDir, AbsoluteDirectoryPath projectDir, string preamble, IEnumerable<string> dependencies)
		{
			foreach (var dependency in dependencies)
				buildProject.MutablePackageReferences.Add(Uno.ProjectFormat.PackageReference.FromString(dependency));

			var preambleCode = preamble;
			var preambleCodeFile = preambleDir / new FileName("$.uno");

			_fileSystem.Create(preambleCodeFile.ContainingDirectory);
			_fileSystem.ForceWriteText(preambleCodeFile, preambleCode);

			var relativeGeneratedCodeFile = preambleCodeFile.RelativeTo(projectDir);
			buildProject.MutableIncludeItems.Add(
				new IncludeItem(
					buildProject.Source,
					IncludeItemType.SourceFile,
					relativeGeneratedCodeFile.NativeRelativePath.ToUnixPath()));
		}

		void AddIcons(Project buildProject, AbsoluteDirectoryPath preambleDir, AbsoluteDirectoryPath projectDir)
		{
			var iconsAsm =  typeof (SimulatorBuilder).Assembly;

			var icons = new[]
			{
				"Android_HDPI.png",
				"Android_LDPI.png",
				"Android_MDPI.png",
				"Android_XHDPI.png",
				"Android_XXHDPI.png",
				"Android_XXXHDPI.png",
				"iOS_iPad_29_1x.png",
				"iOS_iPad_29_2x.png",
				"iOS_iPad_40_2x.png",
				"iOS_iPad_76_1x.png",
				"iOS_iPad_76_2x.png",
				"iOS_iPhone_29_2x.png",
				"iOS_iPhone_29_3x.png",
				"iOS_iPhone_40_2x.png",
				"iOS_iPhone_40_3x.png",
				"iOS_iPhone_60_2x.png",
				"iOS_iPhone_60_3x.png",
			};

			foreach (var icon in icons)
			{
				try
				{
					var fileName = new FileName(icon);
					var dstPath = preambleDir / fileName;
					var srcResourceName = "Fuse.Preview.Icons." + icon;

					using (_fileSystem.BackupAndDeleteFile(dstPath))
					{
						using (var dst = _fileSystem.CreateNew(dstPath))
						using (var src = iconsAsm.GetManifestResourceStream(srcResourceName))
						{
							if (src == null) throw new Exception("Embedded resource not found: " + srcResourceName);
							src.CopyTo(dst);
						}
					}

					var relativeIconPath = dstPath.RelativeTo(projectDir).NativeRelativePath.ToUnixPath();

					var platform = StringSplitting.BeforeFirst(icon, "_");
					var name = StringSplitting.BeforeLast(StringSplitting.AfterFirst(icon, "_"), ".");

					buildProject.MutableProperties[platform + ".Icons." + name] = new SourceValue(buildProject.Source, relativeIconPath);

				}
				catch (Exception)
				{
					// TODO: probably report something?
				}
			}
		}

	}

	public class LocalSimulatorTarget : global::Uno.Build.BuildTarget
	{
		public override string Identifier
		{
			get { return "DotNetDll"; }
		}

		public override string Description
		{
			get { return ".NET/GL bytecode and library."; }
		}

		public override Backend CreateBackend()
		{
			return new CilBackend(new GLBackend());
		}
	}

	public class SimulatorUnoProject
	{
		public readonly Project Project;		
		public readonly BuildOptions Options;
		public readonly string Tag;
		public readonly bool IsVerboseBuild;
		public readonly bool BuildLibraries;

		public SimulatorUnoProject(Project project, BuildOptions options, string tag, bool isVerboseBuild, bool buildLibraries)
		{
			Project = project;
			Options = options;
			Tag = tag;
			IsVerboseBuild = isVerboseBuild;
			BuildLibraries = buildLibraries;
		}
	}
}
