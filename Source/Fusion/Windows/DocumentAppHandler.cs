using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Pipes;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Outracks.Fusion.Windows
{
	using IO;
	using IPC;

	class Document
	{
		public readonly AbsoluteFilePath ProjectPath;
		public readonly IDialog<object> Window;

		public Document(AbsoluteFilePath projectPath, IDialog<object> dialog)
		{
			ProjectPath = projectPath;
			Window = dialog;
		}
	}

	class ObservableDocument : IDocument
	{
		public ObservableDocument(AbsoluteFilePath path)
		{
			FilePath = Observable.Return(Optional.Some(path));
		}

		public IObservable<Optional<AbsoluteFilePath>> FilePath { get; private set; }
		public IObservable<byte[]> Content { get; private set; }
		public void Write(byte[] content) { }
		public IDialog<object> Window { get; set; }
	}

	class DocumentAppHandler
	{
		readonly IReport _reporter;
		readonly string[] _args;
				
		// DON'T REMOVE: We have to keep a reference to prevent GC collection
		// ReSharper disable once NotAccessedField.Local
		Mutex _mutex;
		readonly PipeName _pipeName;
		readonly List<Document> _projects = new List<Document>(); 

		public DocumentAppHandler(IReport reporter, string applicationName, string[] args)
		{
			_reporter = reporter;
			_args = args;
			_pipeName = new PipeName(applicationName);
		}

		public readonly ISubject<Window> DocumentOpened = new Subject<Window>();
		
		public bool IsOnlyInstance()
		{
			bool isOnlyInstance;
			_mutex = new Mutex(true, _pipeName.ToString(), out isOnlyInstance);
			return isOnlyInstance;
		}

		public void RunClient()
		{
			var createClient = Pipe.Connect(_pipeName);
			if (!createClient.Wait(TimeSpan.FromSeconds(6)))
			{
				var errorMessage = "Unable to create Fuse document client. Please try again.";
				MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				_reporter.Error(errorMessage, ReportTo.Log | ReportTo.Headquarters);
				return;
			}

			using (var client = createClient.Result)
			using (var streamWriter = new StreamWriter(client))
			{
				foreach (var arg in _args)
				{
					streamWriter.WriteLine(arg);
				}
			}
		}

		public void RunHost()
		{
			if (_args.IsEmpty())
			{
				Fusion.Application.LaunchedWithoutDocuments();
			}
			else
			{
				foreach (var arg in _args)
					OpenDocumentWindow(arg);
			}

			var thread = new Thread(
				() =>
				{
					while (true)
					{
						try
						{
							var projectPaths = new List<string>();
							using (var host = (NamedPipeServerStream)Pipe.Host(_pipeName).Result)
							using (var reader = new StreamReader(host))
							{
								string line;
								while((line = reader.ReadLine()) != null)
									projectPaths.Add(line);
							}

							if (projectPaths.IsEmpty())
							{
								// Then focus the last opened Window if we have one
								_projects.LastOrNone().Do(doc => doc.Window.Focus());
							}
							else
							{
								foreach (var project in projectPaths)
								{
									OpenDocumentWindow(project);
								}
							}
						}
						catch (Exception e)
						{		
							_reporter.Exception("Failed to host document app.", e, ReportTo.Log | ReportTo.Headquarters);
						}
					}
				})
			{
				IsBackground = true
			};
			thread.Start();
		}

		void OpenDocumentWindow(string projectPath)
		{
			try
			{
				var projPath = AbsoluteFilePath.Parse(projectPath);
				OpenDocumentWindow(projPath);
			}
			catch (Exception e)
			{
				_reporter.Exception("Failed to load project '" + projectPath + "'", e, ReportTo.Log | ReportTo.Headquarters);
			}			
		}

		public Task<ObservableDocument> OpenDocumentWindow(AbsoluteFilePath projectPath)
		{
			var tcs = new TaskCompletionSource<ObservableDocument>();
			var observableDoc = new ObservableDocument(projectPath);
			var existingDocument = _projects.FirstOrNone(project => project.ProjectPath == projectPath);
			if (existingDocument.HasValue)
			{
				existingDocument.Value.Window.Focus();
				tcs.SetResult(observableDoc);
			}
			else
			{
				var dummyWindow = new WindowsDialog<object>();
				dummyWindow.ShowDialog<object>(self =>
				{
					observableDoc.Window = self;
					var window = Fusion.Application.CreateDocumentWindow(observableDoc);
					var document = new Document(projectPath, self);
					window.Closed = window.Closed.Then(Command.Enabled(() =>
						_projects.Remove(document)));
					_projects.Add(document);
					DocumentOpened.OnNext(window);
					window.Content.IsRooted.Take(1).Subscribe(_ => 
						tcs.SetResult(observableDoc));
					return window;
				}).ContinueWith(e => { });
			}
			return tcs.Task;
		}
	}
}