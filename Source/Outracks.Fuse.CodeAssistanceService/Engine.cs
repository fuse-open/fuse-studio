using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Outracks.CodeCompletion;
using Outracks.IO;
using Outracks.UnoDevelop.CodeNinja;
using Uno.Build.Packages;
using Uno.ProjectFormat;
using Uno.Compiler;
using Uno.Compiler.Core;
using Uno.Logging;

namespace Outracks.Fuse.CodeAssistanceService
{
	public class Engine : IEngine, IDisposable
	{
		Project _currentProject;
		CodeNinjaBuild _lastBuild;
		readonly EditorManager _editorManager = new EditorManager();
		readonly Thread _buildThread;
		readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
		readonly AutoResetEvent _buildEvent = new AutoResetEvent(false);
		readonly ProjectDetector _projectDetector;


		public Compiler Compiler
		{
			get
			{
				return _lastBuild == null ? null : _lastBuild.Compiler;
			}
		}

		public SourcePackage MainPackage { get; private set; }

		public Engine(ProjectDetector projectDetector)
		{
			_projectDetector = projectDetector;
			_buildThread = new Thread(BuildThread) { Name = "Codeninja build thread", IsBackground = true };
			_buildThread.Start();
		}

		public bool Compile(Logger logger, string source, Optional<AbsoluteFilePath> filePath)
		{
			if (!filePath.HasValue)
				return true;			

			try
			{
				var projectPath = _projectDetector.GetProject(filePath.Value);
				if (NeedsToChangeProject(projectPath))
					return ChangeProject(projectPath.NativePath);				

				_editorManager.AddChangedFile(source, filePath.Value);	
				TriggerBuild();
			}
			catch (Exception e) {
                logger.TextWriter.WriteLine(e.ToString()); //This feels super weird but i need the error somehow
				return false;		
			}

			return true;
		}

		bool ChangeProject(string projectPath)
		{			
			_currentProject = Project.Load(projectPath);

			new Shell()
				.Watch(AbsoluteFilePath.Parse(projectPath).ContainingDirectory, "*.unoproj")
				.Where(e => e.Event == FileSystemEvent.Changed || e.Event == FileSystemEvent.Removed)
				.Subscribe(f => _currentProject = null);

			MainPackage = PackageCache.GetPackage(Log.Default, _currentProject);
			MainPackage.SetCacheDirectory((AbsoluteDirectoryPath.Parse(MainPackage.CacheDirectory) / new DirectoryName("CodeCompletion")).NativePath);

			TriggerBuild();

			return true;
		}

		bool NeedsToChangeProject(AbsoluteFilePath projectPath)
		{
			return _currentProject == null || projectPath != AbsoluteFilePath.Parse(_currentProject.FullPath);
		}

		public void Invalidate()
		{
			TriggerBuild();
		}

		void TriggerBuild()
		{
			_buildEvent.Reset();
			_buildEvent.Set();
		}

		void BuildThread()
		{
			while (true)
			{
				_buildEvent.WaitOne();
				
				if (_cancellationToken.IsCancellationRequested)
					return;

				Build();
			}
		}

		void Build()
		{
			var build = new CodeNinjaBuild(new Logger(), _currentProject, _editorManager, MainPackage, MainPackage.References.ToList());
			var result = build.Execute();
			if (result == ExecuteStatus.NotDirty || result == ExecuteStatus.Fatal)
				return;

			_lastBuild = build;
		}

		public IEnumerable<AbsoluteFilePath> AllSourceFiles 
		{
			get
			{
				var filePaths = _lastBuild != null ? _lastBuild.FilePaths : new AbsoluteFilePath[0];
				return !filePaths.Any() && _currentProject != null 
					? _currentProject.GetFlattenedItems().Where(f => f.Type != IncludeItemType.Folder).Select(f => AbsoluteFilePath.Parse(f.Value)) 
					: filePaths;
			}		
		}

		public void Dispose()
		{
			_cancellationToken.Cancel();
			_buildEvent.Set();			
		}
	}
}