using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Outracks.IO;
using Uno.Build;
using Uno.ProjectFormat;
using Uno.Build.Targets.Uno;
using Uno.Compiler;
using Uno.Compiler.API.Backends;
using Uno.Compiler.API.Domain.AST;
using Uno.Compiler.Core;
using Uno.Compiler.Frontend.Analysis;
using Uno.Logging;
using Uno.IO;

namespace Outracks.UnoDevelop.CodeNinja
{
	public interface IEditorManager
	{
		DateTime GetLastWriteTime(AbsoluteFilePath filePath);

		bool IsOpen(AbsoluteFilePath filePath);

		string ReadAllText(AbsoluteFilePath filePath);
	}

	public class DummyEditorManager : IEditorManager
	{
		public DateTime GetLastWriteTime(AbsoluteFilePath filePath)
		{
			return DateTime.Now;
		}

		public bool IsOpen(AbsoluteFilePath filePath)
		{
			return true;
		}

		public string ReadAllText(AbsoluteFilePath filePath)
		{
			return File.ReadAllText(filePath.NativePath);
		}
	}


	public interface ILog
	{
		TextWriter TextWriter { get; }

		void Mute();
		void Unmute();
		void Clear();
	}

	public class DummyLogger : ILog
	{
		public void Clear() { }

		public StringBuilder sb = new StringBuilder();

		public DummyLogger()
		{
			_tw = new System.IO.StringWriter(sb);
		}

		System.IO.StringWriter _tw;
		public System.IO.TextWriter TextWriter
		{
			get
			{
				return _tw;
			}
		}

		public void Show()
		{

		}

		public void Mute()
		{
		}

		public void Unmute()
		{
		}
	}

	[Flags]
	enum DirtyFlag
	{
		None,
		Dirty
	}

	public enum ExecuteStatus
	{
		Error,
		Fatal,
		NotDirty,
		Success
	}

	class ASTLocalCacheData
	{
		public string Name;
		public DateTime Timestamp;
		public List<AstDocument> Ast;
	}

	public class CodeNinjaBuild
	{
		readonly IEditorManager _editors;
		readonly ILog _logWriter;
		readonly Log _log;
		readonly Compiler _compiler;
		readonly string _extraCode;
		readonly AbsoluteFilePath _extraCodeSource;
		readonly List<RelativeFilePath> _filePaths;
		readonly AbsoluteDirectoryPath _projDir;
		static readonly List<SourcePackage> _packages = new List<SourcePackage>();

		public readonly SourcePackage ProjectPackage;

		public Compiler Compiler
		{
			get { return _compiler; }
		}

		public CodeNinjaBuild(ILog logWriter, Project project, IEditorManager editors, SourcePackage mainPackage, List<SourcePackage> referencedPackages, string extraCode = null, AbsoluteFilePath extraSourcePath = null)
		{
			_log = new Log(logWriter.TextWriter);
			_log.MaxErrorCount = 0;

			_logWriter = logWriter;
			_editors = editors;
			
			//var newPackages = new List<SourcePackage>();
			//mainPackage = Uno.Build.Packages.PackageResolver.ResolvePackages(_log, project, newPackages);            

			AddNotAddedPackages(referencedPackages);
			mainPackage.References.Clear();
			_packages.Each(p => mainPackage.References.Add(p));

			ProjectPackage = mainPackage;

			_extraCode = extraCode;
			_extraCodeSource = extraSourcePath;

			var configuration = new CodeNinjaBuildTarget();
			var backend = configuration.CreateBackend();

			var projectDir = project.RootDirectory;
			var rootDir = AbsoluteDirectoryPath.Parse(Path.Combine(projectDir, ".CodeNinja"));

			_compiler = new Compiler(
				_log,
				backend,
				ProjectPackage,
				new CompilerOptions
				{
					Debug = true,
					CodeCompletionMode = true,
					OutputDirectory = (rootDir / "Output").ToString(),
					BuildTarget = new DefaultBuild().Identifier,
					Strip = false
				});

			_projDir = AbsoluteDirectoryPath.Parse(ProjectPackage.SourceDirectory);
			_filePaths = project.SourceFiles.Select(x => x.UnixPath).Select(RelativeFilePath.Parse).ToList();
		}

		void AddNotAddedPackages(IEnumerable<SourcePackage> sourcePackages)
		{
			// TODO: Fix this hack...

			var sourcePackagesArr = sourcePackages as SourcePackage[] ?? sourcePackages.ToArray();
			foreach (var package in sourcePackagesArr)
			{
				var found = _packages.FirstOrDefault(p => PackageEqual(p, package));
				if(found == null)
					_packages.Add(package);
			}

			foreach (var package in _packages.ToArray())
			{
				var found = sourcePackagesArr.FirstOrDefault(p => PackageEqual(p, package));
				if (found == null)
					_packages.Remove(package);
			}
		}

		bool PackageEqual(SourcePackage a, SourcePackage b)
		{
			return a.Name == b.Name;
		}

		public IEnumerable<AbsoluteFilePath> FilePaths
		{
			get { return _filePaths.Select(f => _projDir / f); }
		}

		AbsoluteFilePath GetAbsolutePath(SourcePackage p, RelativeFilePath s)
		{
			return AbsoluteDirectoryPath.Parse(p.SourceDirectory) / s;
		}

		public void MuteLog()
		{
			_logWriter.Mute();
		}

		public void UnmuteLog()
		{
			_logWriter.Unmute();
		}

		public ExecuteStatus Execute()
		{
			_logWriter.Clear();

			try 
			{
				if (_extraCode != null)
				{
					var ast = new List<AstDocument>();
					var parser = new Parser(_log, ProjectPackage, _extraCodeSource.NativePath, _extraCode);
					parser.Parse(ast);

					// Don't check for error, just add the AST anyway
					_compiler.AstProcessor.AddRange(ast);
				}

				var dirtyFlag = DirtyFlag.None;
				foreach (var pack in _packages)
				{
					foreach (var file in pack.SourceFiles)
					{
						dirtyFlag |= TryParse(pack, GetAbsolutePath(pack, RelativeFilePath.Parse(file.UnixPath))) ? DirtyFlag.None : DirtyFlag.Dirty;
					}
				}

				BuildUX();

				foreach (var path in ProjectPackage.SourceFiles)
				{
					dirtyFlag |= TryParse(ProjectPackage, _projDir / RelativeFilePath.Parse(path.UnixPath)) ? DirtyFlag.None : DirtyFlag.Dirty;
				}

				if (!dirtyFlag.HasFlag(DirtyFlag.Dirty))
				{
					return ExecuteStatus.NotDirty;
				}

				_compiler.InitializeIL();
				_compiler.TypeBuilder.Build();              
				_compiler.BlockBuilder.Build();

				var errors = _compiler.Log.ErrorCount;
				return errors == 0 ? ExecuteStatus.Success : ExecuteStatus.Error;
			}
			catch (Exception e)
			{
				ReportFactory.FallbackReport.Exception("CodeNinjaBuild failed: ", e);
				return ExecuteStatus.Fatal;
			}
		}

		private void BuildUX()
		{
			ProjectPackage.SourceFiles.Clear();
			ProjectPackage.SourceFiles.AddRange(_filePaths.Select(f => (FileItem) f.NativeRelativePath));

			try
			{
				Uno.UX.Markup.CodeGeneration.UXProcessor.Build(Disk.Default, new[] {ProjectPackage});
			}
			catch (Exception)
			{
			}
		}

		bool TryParse(SourcePackage package, AbsoluteFilePath path)
		{
			var compilerPath = path.NativePath;

			var sourceTimestamp = _editors.GetLastWriteTime(path);
			var cachedData = _astCache.FirstOrDefault(x => x.Name == compilerPath);
			if (cachedData == null)
			{
				cachedData = new ASTLocalCacheData() { Name = compilerPath, Timestamp = sourceTimestamp };
				_astCache.Add(cachedData);
			}
			else
			{
				if (TryLoadParsedFileFromCache(sourceTimestamp, cachedData, _compiler))
					return true;
			}

			try
			{
				var parser = new Parser(_log, package, compilerPath, _editors.ReadAllText(path));
				var ast = new List<AstDocument>();

				parser.Parse(ast);
				_compiler.AstProcessor.AddRange(ast);
				cachedData.Ast = ast;
				return false;
			}
			catch
			{
				return false;
			}                     
		}

		// Temporary optimization, until compiler has better file management.
		readonly static List<ASTLocalCacheData> _astCache = new List<ASTLocalCacheData>();

		static bool TryLoadParsedFileFromCache(DateTime sourceTimestamp, ASTLocalCacheData cacheData, Compiler compiler)
		{
			if (cacheData.Timestamp < sourceTimestamp || cacheData.Ast == null) return false;

			compiler.AstProcessor.AddRange(cacheData.Ast);

			return true;
		}
	}


	class CodeNinjaBuildTarget : BuildTarget
	{
		public override string Identifier
		{
			get { return "CodeNinja"; }
		}

		public override Backend CreateBackend()
		{
			return new DefaultBackend();
		}
	}
}